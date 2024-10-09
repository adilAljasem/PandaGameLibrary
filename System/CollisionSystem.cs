using Microsoft.Xna.Framework;
using PandaGameLibrary.Components;
using System.Collections.Immutable;
using System.Diagnostics;

namespace PandaGameLibrary.System
{
    public class CollisionSystem
    {
        private float SMOOTHING_FACTOR = 0.4f; // Adjust this value between 0 and 1 for desired smoothness
        private static Random random = new Random();
        private GameTime gameTime1 = new GameTime();


        public double CheckCollisions(ImmutableList<GameObject> gameObjects, GameTime gameTime)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            gameTime1 = gameTime;
            var allColliders = new List<ColliderComponent>(gameObjects.Count * 2); // Estimate 2 colliders per object

            // Collect all collider components from the game objects
            foreach (var gameObject in gameObjects)
            {
                allColliders.AddRange(gameObject.GetComponents<ColliderComponent>());
            }

            // Get the number of CPU cores available
            int numberOfCores = Environment.ProcessorCount * 2;

            // Split the collider list into parts for multitasking
            int collidersPerTask = allColliders.Count / numberOfCores;
            List<Task> tasks = new List<Task>();

            for (int coreIndex = 0; coreIndex < numberOfCores; coreIndex++)
            {
                int start = coreIndex * collidersPerTask;
                int end = (coreIndex == numberOfCores - 1) ? allColliders.Count : start + collidersPerTask; // Last task gets any remaining colliders

                // Create a task for each chunk
                var task = Task.Run(() =>
                {
                    // Loop through the colliders assigned to this task
                    for (int i = start; i < end; i++)
                    {
                        var colliderA = allColliders[i];
                        if (!colliderA.IsEnabled) continue;

                        for (int j = i + 1; j < allColliders.Count; j++)
                        {
                            var colliderB = allColliders[j];
                            if (!colliderB.IsEnabled) continue;

                            // Check if the two colliders are within distance and not part of the same object or family
                            if (IsWithinDistance(colliderA, colliderB) &&
                                colliderA.gameObject.GameObjectId != colliderB.gameObject.GameObjectId &&
                                !IsParentChildOrSiblings(colliderA.gameObject, colliderB.gameObject))
                            {
                                // Handle collision if both colliders have the collision flag set
                                if (colliderA.Collision && colliderB.Collision)
                                    HandleCollision(colliderA, colliderB);

                                // Resolve collision unless both colliders are transparent
                                if (!colliderA.Transparent && !colliderB.Transparent)
                                {
                                    ResolveCollision(colliderA, colliderB);
                                    ResolveCollision(colliderB, colliderA);
                                }
                            }
                            else
                            {
                                // Check for exit collisions (if applicable)
                                CheckExitCollisions(colliderA, colliderB);
                            }
                        }
                    }
                });

                tasks.Add(task);
            }

            // Wait for all tasks to complete
            Task.WaitAll(tasks.ToArray());

            stopwatch.Stop();
            return stopwatch.Elapsed.TotalMilliseconds;
        }

        private void HandleCollision(ColliderComponent colliderA, ColliderComponent colliderB)
        {
            byte dummy;
            // Check if the HashSet already contains the colliderB ComponentId
            if (colliderA.CollidingObjects.TryAdd(colliderB.ComponentId, 0)) // Add returns false if already exists
            {
                colliderA.OnEnter(colliderB.gameObject);
                colliderB.OnEnter(colliderA.gameObject);
            }
            else
            {
                colliderA.OnCollide(colliderB.gameObject);
                colliderB.OnCollide(colliderA.gameObject);
            }
        }

        private void CheckExitCollisions(ColliderComponent colliderA, ColliderComponent colliderB)
        {
            byte dummy;
            if (colliderA.CollidingObjects.TryRemove(colliderB.ComponentId, out dummy))
            {
                colliderA.OnExit(colliderB.gameObject);
                colliderB.OnExit(colliderA.gameObject);
            }
            if (colliderB.CollidingObjects.TryRemove(colliderA.ComponentId, out dummy))
            {
                colliderA.OnExit(colliderB.gameObject);
                colliderB.OnExit(colliderA.gameObject);
            }
        }

        private bool IsWithinDistance(ColliderComponent colliderA, ColliderComponent colliderB)
        {
            float distance = Vector2.Distance(colliderA.Center, colliderB.Center);
            return distance < colliderA.Radius + colliderB.Radius;
        }

        private bool IsParentChildOrSiblings(GameObject a, GameObject b)
        {
            if (a.Parent == b || b.Parent == a)
            {
                return true;
            }
            if (a.Parent != null && a.Parent == b.Parent)
            {
                return true;
            }
            if (a.Children.Contains(b) || b.Children.Contains(a))
            {
                return true;
            }
            return false;
        }

        private void ResolveCollision(ColliderComponent collider1, ColliderComponent collider2)
        {
            // Case 1: Both colliders are dynamic
            if (collider1.IsDynamic && collider2.IsDynamic)
            {
                ResolveDoubleDynamicCollision(collider1, collider2);
            }
            // Case 2: Only collider1 is dynamic and it's not set to be transparent with static
            else if (collider1.IsDynamic && !collider2.IsDynamic && !collider1.TransparentWithStatic)
            {
                ResolveDynamicStaticCollision(collider1, collider2);
            }
            // Case 3: Only collider2 is dynamic and it's not set to be transparent with static
            else if (!collider1.IsDynamic && collider2.IsDynamic && !collider2.TransparentWithStatic)
            {
                ResolveDynamicStaticCollision(collider2, collider1);
            }
            else if (!collider1.IsDynamic && !collider2.IsDynamic && collider1.ResolveWithStatic && collider2.ResolveWithStatic)
            {
                ResolveDoubleStaticCollision(collider1, collider2);
            }
            // In all other cases (including when TransparentWithStatic is true), do nothing
        }
        private void ResolveDoubleDynamicCollision(ColliderComponent collider1, ColliderComponent collider2)
        {
            Vector2 direction = collider1.Center - collider2.Center;
            float currentDistance = direction.Length();
            // Handle the case where players are in the exact same position
            if (currentDistance == 0)
            {
                // Generate a random direction
                direction = new Vector2(/*(float)random.NextDouble() -*/ 0.5f, /*(float)random.NextDouble() - */0.5f);
                currentDistance = direction.Length();
            }
            float minimumDistance = collider1.Radius + collider2.Radius;
            float overlapDistance = minimumDistance - currentDistance;
            if (overlapDistance > 2)
            {
                Vector2 movementDirection = Vector2.Normalize(direction);
                Vector2 resolutionVector = movementDirection * overlapDistance * 0.5f; // Split the resolution between both objects
                collider1.gameObject.Transform.Position += resolutionVector * 0.4f * (float)gameTime1.ElapsedGameTime.TotalSeconds * 60;

            }
        }
        private void ResolveDynamicStaticCollision(ColliderComponent dynamicCollider, ColliderComponent staticCollider)
        {
            Vector2 direction = dynamicCollider.Center - staticCollider.Center;
            float currentDistance = direction.Length();
            // Handle the case where colliders are in the exact same position
            if (currentDistance == 0)
            {
                // Generate a random direction
                direction = new Vector2(/*(float)random.NextDouble() -*/ 0.5f, /*(float)random.NextDouble() -*/ 0.5f);
                currentDistance = direction.Length();
            }
            float minimumDistance = dynamicCollider.Radius + staticCollider.Radius;
            float overlapDistance = minimumDistance - currentDistance;
            if (overlapDistance > 2)
            {
                Vector2 movementDirection = Vector2.Normalize(direction);
                Vector2 resolutionVector = movementDirection * overlapDistance;
                dynamicCollider.gameObject.Transform.Position += resolutionVector * 0.4f * (float)gameTime1.ElapsedGameTime.TotalSeconds * 60;
            }
        }

        private void ResolveDoubleStaticCollision(ColliderComponent collider1, ColliderComponent collider2)
        {
            Vector2 direction = collider1.Center - collider2.Center;
            float currentDistance = direction.Length();
            // Handle the case where colliders are in the exact same position
            if (currentDistance == 0)
            {
                direction = new Vector2(0.5f, 0.5f);
                currentDistance = direction.Length();
            }
            float minimumDistance = collider1.Radius + collider2.Radius;
            float overlapDistance = minimumDistance - currentDistance;
            if (overlapDistance > 2)
            {
                Vector2 movementDirection = Vector2.Normalize(direction);
                Vector2 resolutionVector = movementDirection * overlapDistance * 0.5f; // Split the resolution between both objects

                // Move both static objects slightly
                collider1.gameObject.Transform.Position += resolutionVector * 0.1f * (float)gameTime1.ElapsedGameTime.TotalSeconds * 60;
                collider2.gameObject.Transform.Position -= resolutionVector * 0.1f * (float)gameTime1.ElapsedGameTime.TotalSeconds * 60;

            }
        }


    }
}
