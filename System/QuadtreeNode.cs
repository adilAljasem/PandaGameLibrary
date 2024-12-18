using System.Collections.Generic;
using Microsoft.Xna.Framework;
using PandaGameLibrary.Components;

internal class Quadtree
{
    private const int MAX_OBJECTS = 10;
    private const int MAX_LEVELS = 5;

    private int level;
    private List<GameObject> objects;
    private Rectangle bounds;
    private Quadtree[] nodes;

    public Quadtree(int level, Rectangle bounds)
    {
        this.level = level;
        this.objects = new List<GameObject>();
        this.bounds = bounds;
        this.nodes = new Quadtree[4];
    }

    public void Clear()
    {
        objects.Clear();

        for (int i = 0; i < nodes.Length; i++)
        {
            if (nodes[i] != null)
            {
                nodes[i].Clear();
                nodes[i] = null;
            }
        }
    }

    private void Split()
    {
        int subWidth = bounds.Width / 2;
        int subHeight = bounds.Height / 2;
        int x = bounds.X;
        int y = bounds.Y;

        nodes[0] = new Quadtree(level + 1, new Rectangle(x + subWidth, y, subWidth, subHeight));
        nodes[1] = new Quadtree(level + 1, new Rectangle(x, y, subWidth, subHeight));
        nodes[2] = new Quadtree(level + 1, new Rectangle(x, y + subHeight, subWidth, subHeight));
        nodes[3] = new Quadtree(level + 1, new Rectangle(x + subWidth, y + subHeight, subWidth, subHeight));
    }

    private int GetIndex(Rectangle rect)
    {
        int index = -1;
        double verticalMidpoint = bounds.X + (bounds.Width / 2);
        double horizontalMidpoint = bounds.Y + (bounds.Height / 2);

        bool topQuadrant = (rect.Y < horizontalMidpoint && rect.Y + rect.Height < horizontalMidpoint);
        bool bottomQuadrant = (rect.Y > horizontalMidpoint);

        if (rect.X < verticalMidpoint && rect.X + rect.Width < verticalMidpoint)
        {
            if (topQuadrant)
            {
                index = 1;
            }
            else if (bottomQuadrant)
            {
                index = 2;
            }
        }
        else if (rect.X > verticalMidpoint)
        {
            if (topQuadrant)
            {
                index = 0;
            }
            else if (bottomQuadrant)
            {
                index = 3;
            }
        }

        return index;
    }

    public void Insert(GameObject gameObject)
    {
        if (nodes[0] != null)
        {
            int index = GetIndex(gameObject.GetComponent<ColliderComponent>().Bounds);

            if (index != -1)
            {
                nodes[index].Insert(gameObject);

                return;
            }
        }

        objects.Add(gameObject);

        if (objects.Count > MAX_OBJECTS && level < MAX_LEVELS)
        {
            if (nodes[0] == null)
            {
                Split();
            }

            int i = 0;
            while (i < objects.Count)
            {
                int index = GetIndex(objects[i].GetComponent<ColliderComponent>().Bounds);
                if (index != -1)
                {
                    nodes[index].Insert(objects[i]);
                    objects.RemoveAt(i);
                }
                else
                {
                    i++;
                }
            }
        }
    }

    public void Retrieve(List<GameObject> returnObjects, Rectangle rect)
    {
        int index = GetIndex(rect);
        if (index != -1 && nodes[0] != null)
        {
            nodes[index].Retrieve(returnObjects, rect);
        }

        returnObjects.AddRange(objects);
    }
}
