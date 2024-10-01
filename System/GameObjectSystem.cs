using Microsoft.Xna.Framework;
using PandaGameLibrary.Components;
using System.Collections.Immutable;

namespace PandaGameLibrary.System
{
    public class GameObjectSystem
    {
        private ImmutableList<GameObject> allGameObjects = ImmutableList<GameObject>.Empty;

        public void AddGameObject(GameObject gameObject)
        {
            ImmutableInterlocked.Update(ref allGameObjects, list => list.Add(gameObject));
        }

        public void RemoveGameObject(GameObject gameObject)
        {
            ImmutableInterlocked.Update(ref allGameObjects, list => list.Remove(gameObject));
        }

        public void RemoveGameObjectWithChildrenAndParent(GameObject gameObject)
        {
            // Recursively remove all children
            RemoveChildrenRecursively(gameObject);

            // Remove the parent if it exists
            if (gameObject.Parent != null)
            {
                RemoveGameObject(gameObject.Parent);
            }

            // Remove the game object itself
            RemoveGameObject(gameObject);
        }

        private void RemoveChildrenRecursively(GameObject gameObject)
        {
            // Remove all children recursively
            foreach (var child in gameObject.Children)
            {
                RemoveChildrenRecursively(child);
                RemoveGameObject(child);
            }
        }

        public void RemoveGameObjectsByTag(string tag)
        {
            ImmutableInterlocked.Update(ref allGameObjects, list =>
            {
                var objectsToRemove = list.Where(go => go.Tag == tag).ToList();
                foreach (var gameObject in objectsToRemove)
                {
                    RemoveGameObjectWithChildrenAndParent(gameObject);
                }
                return list.RemoveRange(objectsToRemove);
            });
        }

        public void RemoveGameObjects(IEnumerable<GameObject> gameObjects)
        {
            ImmutableInterlocked.Update(ref allGameObjects, list => list.RemoveRange(gameObjects));
        }

        public ImmutableList<GameObject> GetAllGameObjects()
        {
            return allGameObjects;
        }

        public GameObject GetGameObjectById(Guid id)
        {
            return allGameObjects.FirstOrDefault(g => g.GameObjectId == id);
        }

        public GameObject GetGameObjectByTag(string tag)
        {
            return allGameObjects.FirstOrDefault(g => g.Tag == tag);
        }

        public ImmutableList<GameObject> GetGameObjectsListByTag(string tag)
        {
            return allGameObjects.Where(go => go.Tag == tag).ToImmutableList();
        }

        public void UpdateGameObjects(GameTime gameTime)
        {
            foreach (var gameObject in allGameObjects)
            {
                gameObject.Update(gameTime);
            }
        }
    }
}