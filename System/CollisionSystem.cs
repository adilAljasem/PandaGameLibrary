using Microsoft.Xna.Framework;
using PandaGameLibrary.Components;
using System.Collections.Immutable;
using System.Diagnostics;

namespace PandaGameLibrary.System
{
    internal class CollisionSystem
    {
        private float SMOOTHING_FACTOR = 500.4f; // Adjust this value between 0 and 1 for desired smoothness
        private static Random random = new Random();
        private GameTime gameTime1 = new GameTime();
        private const float MIN_VELOCITY = 0.01f;
        private const float MAX_VELOCITY = 10f;

        private float MAX_TIMESTEP = 1f / 5f; // Maximum allowed timestep
        private float accumulatedTime = 0f;

        public double CheckCollisions(ImmutableList<GameObject> gameObjects, GameTime gameTime)
        {
            gameTime1 = gameTime;
            Stopwatch stopwatch = Stopwatch.StartNew();
            MAX_TIMESTEP = 1f / 60f;
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            accumulatedTime += deltaTime;

            // Limit the accumulated time to prevent spiral of death
            //if (accumulatedTime > MAX_TIMESTEP)
            //{
            //    accumulatedTime = MAX_TIMESTEP;
            //}

            // Perform collision checks with the accumulated time
            if (accumulatedTime > MAX_TIMESTEP)
            {
                float timeStep = accumulatedTime;
                PerformCollisionChecks(gameObjects);
                accumulatedTime = 0;
            }

            stopwatch.Stop();
            return stopwatch.Elapsed.TotalMilliseconds;
        }

        private void PerformCollisionChecks(ImmutableList<GameObject> gameObjects)
        {
            var allColliders = new List<ColliderComponent>(gameObjects.Count * 2);

            foreach (var gameObject in gameObjects)
            {
                allColliders.AddRange(gameObject.GetComponents<ColliderComponent>());
            }

            int numberOfCores = Environment.ProcessorCount;
            int collidersPerTask = allColliders.Count / numberOfCores;
            List<Task> tasks = new List<Task>();

            for (int coreIndex = 0; coreIndex < numberOfCores; coreIndex++)
            {
                int start = coreIndex * collidersPerTask;
                int end = (coreIndex == numberOfCores - 1) ? allColliders.Count : start + collidersPerTask;

                var task = Task.Run(() =>
                {
                    for (int i = start; i < end; i++)
                    {
                        var colliderA = allColliders[i];
                        if (!colliderA.IsEnabled) continue;

                        for (int j = i + 1; j < allColliders.Count; j++)
                        {
                            var colliderB = allColliders[j];
                            if (!colliderB.IsEnabled) continue;

                            if (IsWithinDistance(colliderA, colliderB) &&
                                colliderA.gameObject.GameObjectId != colliderB.gameObject.GameObjectId &&
                                !IsRelated(colliderA.gameObject, colliderB.gameObject))
                            {
                                if (colliderA.Collision && colliderB.Collision)
                                    HandleCollision(colliderA, colliderB);

                                if (!colliderA.Transparent && !colliderB.Transparent)
                                {
                                    ResolveCollision(colliderA, colliderB);
                                }
                            }
                            else
                            {
                                CheckExitCollisions(colliderA, colliderB);
                            }
                        }
                    }
                });

                tasks.Add(task);
            }

            Task.WaitAll(tasks.ToArray());
        }

        // ... (other existing methods like HandleCollision, IsWithinDistance, IsRelated, etc.)

        
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

        private bool IsRelated(GameObject a, GameObject b, int maxDepth = 5)
        {
            if (a == b) return true;

            // Check immediate relationships first
            if (a.Parent == b || b.Parent == a) return true;
            if (a.Parent != null && a.Parent == b.Parent) return true;
            if (a.Children.Contains(b) || b.Children.Contains(a)) return true;

            // If maxDepth is 0, we don't check further
            if (maxDepth == 0) return false;

            // Check if b is a descendant of a
            if (IsDescendant(a, b, maxDepth)) return true;

            // Check if a is a descendant of b
            if (IsDescendant(b, a, maxDepth)) return true;

            return false;
        }

        private bool IsDescendant(GameObject ancestor, GameObject potential_descendant, int maxDepth)
        {
            if (maxDepth == 0) return false;

            foreach (var child in ancestor.Children)
            {
                if (child == potential_descendant) return true;
                if (IsDescendant(child, potential_descendant, maxDepth - 1)) return true;
            }

            return false;
        }

        private void ResolveCollision(ColliderComponent collider1, ColliderComponent collider2)
        {
            Vector2 direction = collider1.Center - collider2.Center;
            float currentDistance = direction.Length();
            var deltaTime = (float)gameTime1.ElapsedGameTime.TotalSeconds;
            // Handle the case where colliders are in the exact same position
            if (currentDistance == 0)
            {
                direction = new Vector2(0.5f, 0.5f);
                currentDistance = direction.Length();
            }

            float minimumDistance = collider1.Radius + collider2.Radius;
            float overlapDistance = minimumDistance - currentDistance;

            if (overlapDistance > 0)
            {
                Vector2 movementDirection = Vector2.Normalize(direction);
                Vector2 resolutionVector = movementDirection * overlapDistance;

                // Calculate the frame rate independent smoothing factor
                float smoothingFactor = 0.4f * (deltaTime / (1f / 60));

                if (collider1.IsDynamic && collider2.IsDynamic)
                {
                    ResolveDoubleDynamicCollision(collider1, collider2, resolutionVector, smoothingFactor , deltaTime);
                }
                else if (collider1.IsDynamic && !collider2.IsDynamic)
                {
                    ResolveDynamicStaticCollision(collider1, resolutionVector, smoothingFactor , deltaTime);
                }
                else if (!collider1.IsDynamic && collider2.IsDynamic)
                {
                    ResolveDynamicStaticCollision(collider2, -resolutionVector, smoothingFactor , deltaTime);
                }
                else if (!collider1.IsDynamic && !collider2.IsDynamic && collider1.ResolveWithStatic && collider2.ResolveWithStatic)
                {
                    ResolveDoubleStaticCollision(collider1, collider2, resolutionVector, smoothingFactor , deltaTime);
                }
            }
        }

        private void ResolveDoubleDynamicCollision(ColliderComponent collider1, ColliderComponent collider2, Vector2 resolutionVector, float smoothingFactor , float delta)
        {
            collider1.gameObject.Transform.Position += resolutionVector *  smoothingFactor;
            collider2.gameObject.Transform.Position -= resolutionVector *  smoothingFactor;
        }

        private void ResolveDynamicStaticCollision(ColliderComponent dynamicCollider, Vector2 resolutionVector, float smoothingFactor,float delta)
        {
            if (!dynamicCollider.TransparentWithStatic)
            {
                dynamicCollider.gameObject.Transform.Position += resolutionVector * smoothingFactor;
            }
        }

        private void ResolveDoubleStaticCollision(ColliderComponent collider1, ColliderComponent collider2, Vector2 resolutionVector, float smoothingFactor, float delta)
        {
            // Move both static objects slightly
            collider1.gameObject.Transform.Position += resolutionVector *  smoothingFactor;
            collider2.gameObject.Transform.Position -= resolutionVector *  smoothingFactor;
        }
        //private bool IsParentChildOrSiblings(GameObject a, GameObject b)
        //{
        //    if (a.Parent == b || b.Parent == a)
        //    {
        //        return true;
        //    }
        //    if (a.Parent != null && a.Parent == b.Parent)
        //    {
        //        return true;
        //    }
        //    if (a.Children.Contains(b) || b.Children.Contains(a))
        //    {
        //        return true;
        //    }
        //    return false;
        //}

        //private void ResolveCollision(ColliderComponent collider1, ColliderComponent collider2)
        //{
        //    // Case 1: Both colliders are dynamic
        //    if (collider1.IsDynamic && collider2.IsDynamic)
        //    {
        //        ResolveDoubleDynamicCollision(collider1, collider2);
        //    }
        //    // Case 2: Only collider1 is dynamic and it's not set to be transparent with static
        //    else if (collider1.IsDynamic && !collider2.IsDynamic && !collider1.TransparentWithStatic)
        //    {
        //        ResolveDynamicStaticCollision(collider1, collider2);
        //    }
        //    // Case 3: Only collider2 is dynamic and it's not set to be transparent with static
        //    else if (!collider1.IsDynamic && collider2.IsDynamic && !collider2.TransparentWithStatic)
        //    {
        //        ResolveDynamicStaticCollision(collider2, collider1);
        //    }
        //    else if (!collider1.IsDynamic && !collider2.IsDynamic && collider1.ResolveWithStatic && collider2.ResolveWithStatic)
        //    {
        //        ResolveDoubleStaticCollision(collider1, collider2);
        //    }
        //    // In all other cases (including when TransparentWithStatic is true), do nothing
        //}
        //private void ResolveDoubleDynamicCollision(ColliderComponent collider1, ColliderComponent collider2)
        //{
        //    Vector2 direction = collider1.Center - collider2.Center;
        //    float currentDistance = direction.Length();
        //    // Handle the case where players are in the exact same position
        //    if (currentDistance == 0)
        //    {
        //        // Generate a random direction
        //        direction = new Vector2(/*(float)random.NextDouble() -*/ 0.5f, /*(float)random.NextDouble() - */0.5f);
        //        currentDistance = direction.Length();
        //    }
        //    float minimumDistance = collider1.Radius + collider2.Radius;
        //    float overlapDistance = minimumDistance - currentDistance;
        //    if (overlapDistance > 2)
        //    {
        //        Vector2 movementDirection = Vector2.Normalize(direction);
        //        Vector2 resolutionVector = movementDirection * overlapDistance * 0.5f; // Split the resolution between both objects
        //        collider1.gameObject.Transform.Position += resolutionVector * 0.4f * FIXED_TIME_STEP * 60;

        //    }
        //}
        //private void ResolveDynamicStaticCollision(ColliderComponent dynamicCollider, ColliderComponent staticCollider)
        //{
        //    Vector2 direction = dynamicCollider.Center - staticCollider.Center;
        //    float currentDistance = direction.Length();
        //    // Handle the case where colliders are in the exact same position
        //    if (currentDistance == 0)
        //    {
        //        // Generate a random direction
        //        direction = new Vector2(/*(float)random.NextDouble() -*/ 0.5f, /*(float)random.NextDouble() -*/ 0.5f);
        //        currentDistance = direction.Length();
        //    }
        //    float minimumDistance = dynamicCollider.Radius + staticCollider.Radius;
        //    float overlapDistance = minimumDistance - currentDistance;
        //    if (overlapDistance > 2)
        //    {
        //        Vector2 movementDirection = Vector2.Normalize(direction);
        //        Vector2 resolutionVector = movementDirection * overlapDistance;
        //        dynamicCollider.gameObject.Transform.Position += resolutionVector * 0.4f * FIXED_TIME_STEP * 60;
        //    }
        //}

        //private void ResolveDoubleStaticCollision(ColliderComponent collider1, ColliderComponent collider2)
        //{
        //    Vector2 direction = collider1.Center - collider2.Center;
        //    float currentDistance = direction.Length();
        //    // Handle the case where colliders are in the exact same position
        //    if (currentDistance == 0)
        //    {
        //        direction = new Vector2(0.5f, 0.5f);
        //        currentDistance = direction.Length();
        //    }
        //    float minimumDistance = collider1.Radius + collider2.Radius;
        //    float overlapDistance = minimumDistance - currentDistance;
        //    if (overlapDistance > 2)
        //    {
        //        Vector2 movementDirection = Vector2.Normalize(direction);
        //        Vector2 resolutionVector = movementDirection * overlapDistance * 0.5f; // Split the resolution between both objects

        //        // Move both static objects slightly
        //        collider1.gameObject.Transform.Position += resolutionVector * 0.1f * FIXED_TIME_STEP * 60;
        //        collider2.gameObject.Transform.Position -= resolutionVector * 0.1f * FIXED_TIME_STEP * 60;

        //    }
        //}


    }
}
