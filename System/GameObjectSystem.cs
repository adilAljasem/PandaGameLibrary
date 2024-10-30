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
            //PandaCore.Instance.RenderSystem.RemoveGameObject(gameObject);
        }
        public void RemoveGameObject(Guid Id)
        {
            var gameObjectToRemove = GetGameObjectById(Id);
            if (gameObjectToRemove != null)
            {
                ImmutableInterlocked.Update(ref allGameObjects, list => list.Remove(gameObjectToRemove));
            }
        }

        public void RemoveGameObjectWithChildren(Guid id)
        {
            var gameObject = GetGameObjectById(id);
            if (gameObject != null)
            {
                // Recursively remove all children
                RemoveChildrenRecursively(gameObject);

                // Remove the game object itself
                RemoveGameObject(id);
            }
        }

        public void RemoveGameObjectWithChildrenAndParent(Guid id)
        {
            var gameObject = GetGameObjectById(id);
            if (gameObject != null)
            {
                // Recursively remove all children
                RemoveChildrenRecursively(gameObject);

                // Remove the parent if it exists
                if (gameObject.Parent != null)
                {
                    RemoveGameObject(gameObject.Parent);
                }

                // Remove the game object itself
                RemoveGameObject(id);
            }
        }

        public void RemoveGameObjectWithChildren(GameObject gameObject)
        {
            // Recursively remove all children
            RemoveChildrenRecursively(gameObject);

            // Remove the game object itself
            RemoveGameObject(gameObject);
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

        public void RemoveChildrenGameObjectsByTag(GameObject parent, string tag, bool searchAllLevels = true)
        {
            if (searchAllLevels)
            {
                // Get all children at all levels that match the tag
                var childrenToRemove = GetAllChildrenWithTag(parent, tag);
                foreach (var child in childrenToRemove)
                {
                    RemoveChildrenRecursively(child);
                    RemoveGameObject(child);
                }
            }
            else
            {
                // Only remove direct children with the tag
                var directChildrenToRemove = parent.Children.Where(child => child.Tag == tag).ToList();
                foreach (var child in directChildrenToRemove)
                {
                    RemoveChildrenRecursively(child);
                    RemoveGameObject(child);
                }
            }
        }

        private List<GameObject> GetAllChildrenWithTag(GameObject parent, string tag)
        {
            var result = new List<GameObject>();
            SearchChildrenWithTag(parent, tag, result);
            return result;
        }

        private void SearchChildrenWithTag(GameObject parent, string tag, List<GameObject> result)
        {
            foreach (var child in parent.Children)
            {
                if (child.Tag == tag)
                {
                    result.Add(child);
                }
                // Recursively search through this child's children
                SearchChildrenWithTag(child, tag, result);
            }
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

        public ImmutableList<GameObject> GetEnabledGameObjects()
        {
            return allGameObjects.Where(go => go.IsEnable).ToImmutableList();
        }

        public void UpdateGameObjects(GameTime gameTime)
        {
            foreach (var gameObject in GetEnabledGameObjects())
            {
                gameObject.Update(gameTime);
            }
        }
    }
}