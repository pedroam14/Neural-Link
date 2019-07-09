using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * Controls which spaces are valid for an
 * {@link generators.IMZDungeonGenerator} to create
 * {@link MZRoom}s in.
 * <p>
 * Essentially just a List<{@link Vector2Int}> with some convenience methods.
 *
 * @see Vector2Int
 * @see SpaceConstraints
 */
public class MZSpaceMap
{
    protected List<Vector2Int> spaces = new List<Vector2Int>();

    public int NumberSpaces()
    {
        return spaces.Count;
    }

    public bool Get(Vector2Int c)
    {
        return spaces.Contains(c);
    }

    public void Set(Vector2Int c, bool val)
    {
        if (val)
        {
            spaces.Add(c);
        }
        else
        {
            spaces.Remove(c);
        }
    }

    private Vector2Int GetFirst()
    {
        return spaces[0];
    }

    public List<Vector2Int> GetBottomSpaces()
    {
        List<Vector2Int> bottomRow = new List<Vector2Int> { GetFirst() };
        int bottomY = GetFirst().y;
        foreach (Vector2Int space in spaces)
        {
            if (space.y < bottomY)
            {
                bottomY = space.y;
                bottomRow = new List<Vector2Int> { space };
            }
            else if (space.y == bottomY)
            {
                bottomRow.Add(space);
            }
        }
        return bottomRow;
    }
}
