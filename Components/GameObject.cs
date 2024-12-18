using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PandaGameLibrary.System;
using System.Collections.Immutable;
using System.Reflection;

namespace PandaGameLibrary.Components;

public sealed class GameObject
{
    public Guid GameObjectId { get; set; }
    public string GameObjectName { get; set; }
    public string Tag { get; set; }
    public Transform Transform { get; set; }
    private ImmutableList<Component> _components = ImmutableList<Component>.Empty;
    public GameObject Parent { get; set; }
    public bool isChild { get; set; }
    public ImmutableList<GameObject> Children { get; set; } = ImmutableList<GameObject>.Empty;
    public bool IsEnable { get; private set; } = true;
    private bool AwakeCalled = false;
    public Action OnGameObjectDestroy { get; set; }

    public GameObject()
    {
        Transform = new Transform();
        GameObjectId = Guid.NewGuid();
        PandaCore.Instance.GameObjectSystem.AddGameObject(this);
    }

    internal void InvokeGameObjectDestroy()
    {
        // Destroy all components
        foreach (var comp in _components.ToList())
        {
            comp.Destroy();
        }
        _components = ImmutableList<Component>.Empty;

        // Finally invoke the destroy event
        var handler = OnGameObjectDestroy;
        OnGameObjectDestroy = null;  // Clear the event to prevent memory leaks
        handler?.Invoke();
    }

    public void AddChild(GameObject child)
    {
        //if (Parent != null)
        //{
        //    Parent.AddChild(child);
        //}
        //if (!child.isChild && !isChild)
        //{
        if (child == null || child == this || child.Parent == this)
        {
            throw new Exception("Child is null or child trying to add it self or you trying to add parent as child");
        }

        Children = Children.Add(child);
        child.Parent = this;
        child.isChild = true;
        child.Transform = Transform;
        child.Children?.ForEach(x => x.Transform = Transform);
        //}
    }

    public void RemoveChild(GameObject child)
    {
        if (child == null) return;
        child.Parent = null;
        child.isChild = false;
        child.Transform = new Transform();
        child.Transform.Position = Transform.Position;
        Children = Children.Remove(child);
    }
    public void RemoveChildren(List<GameObject> children)
    {
        if (children == null || children.Count == 0) return;

        // Create a new immutable list by removing all children
        // ImmutableList builds incrementally, so we can use RemoveRange
        Children = Children.RemoveRange(children.Where(child => child != null));

        // Update each removed child's properties
        foreach (var child in children)
        {
            if (child == null) continue;

            child.Parent = null;
            child.isChild = false;
            child.Transform = new Transform();
            child.Transform.Position = Transform.Position;
        }
    }

    public void SetEnable(bool enable)
    {
        IsEnable = enable;
        foreach (GameObject child in Children)
        {
            child?.SetEnable(enable);
        }
    }

    public GameObject GetChildByTag(string tag)
    {
        return Children.FirstOrDefault((GameObject g) => g.Tag == tag);

    }


    public T AddComponent<T>() where T : Component, new()
    {
        T component = new T();
        component.gameObject = this;
        component.Awake();
        _components = _components.Add(component);
        return component;
    }

    public T AddComponent<T>(T component) where T : Component
    {
        component.gameObject = this;
        component.Awake();
        _components = _components.Add(component);
        return component;
    }



    public T AddComponentByInterface<T>() where T : class
    {
        Type type = Assembly.GetExecutingAssembly().GetTypes().FirstOrDefault((Type t) => typeof(T).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract && t.GetConstructor(Type.EmptyTypes) != null);
        if (type != null)
        {
            T component = Activator.CreateInstance(type) as T;
            if (component is Component comp)
            {
                comp.gameObject = this;
                //comp.gameobjectId = GameObjectId;
                //comp.Awake();
                _components.Add(comp);
            }
            return component;
        }
        throw new InvalidOperationException("No component found that implements " + typeof(T).Name);
    }

    public T GetComponent<T>() where T : Component
    {
        return _components
            .OfType<T>()
            .FirstOrDefault(w => w.gameObject.GameObjectId == GameObjectId);
    }

    public ImmutableList<Component> GetComponents()
    {
        return _components;
    }

    public T GetComponentByInterface<T>() where T : class
    {
        foreach (Component component in GetComponents<Component>())
        {
            if (component is T result)
            {
                return result;
            }
        }
        return null;
    }

    public List<T> GetComponents<T>() where T : Component
    {
        lock (_components)
        {
            return _components.OfType<T>().ToList();
        }
    }
    public T GetComponentById<T>(Guid componentId) where T : Component
    {
        return _components.OfType<T>().FirstOrDefault(c => c.ComponentId == componentId);
    }

    public void RemoveComponent<T>() where T : Component
    {
        var component = _components.OfType<T>().FirstOrDefault();
        if (component != null)
        {
            component.Destroy();
            _components = _components.Remove(component);
        }
    }

    public void RemoveComponent<T>(T Component) where T : Component
    {
        var component = _components.FirstOrDefault(w => w.ComponentId == Component.ComponentId);
        if (component != null)
        {
            component.Destroy();
            _components = _components.Remove(component);
        }
    }

    public void RemoveComponentByTag(string tag)
    {
        var componentToRemove = _components.FirstOrDefault(c => c.Tag == tag);
        if (componentToRemove != null)
        {
            componentToRemove.Destroy();
            _components = _components.Remove(componentToRemove);
        }
    }

    public void RemoveComponentsByTag(string tag)
    {
        _components = _components.RemoveAll(c => c.Tag == tag);
    }

    public void RemoveComponents()
    {
        _components.Clear();
    }


    public void Update(GameTime gameTime)
    {
        var componentsList = _components;

        for (int i = 0; i < componentsList.Count; i++)
        {
            var component = componentsList[i];
            if (component.IsEnabled)
            {
                if (component._startPending)
                {
                    component.Start();
                    component._startPending = false;
                }
                component.Update(gameTime);
            }
        }
    }


}
