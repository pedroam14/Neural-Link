using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * Constrains the coordinates where MZRooms may be placed to be only those within
 * the {@link MZSpaceMap}, as well as placing limitations on the number of keys
 * and switches.
 * 
 * @see CountConstraints
 * @see MZSpaceMap
 */
public class SpaceConstraints : CountConstraints
{

    public static readonly int DefaultMaxKeys = 4, DefaultMaxSwitches = 1;

    protected MZSpaceMap spaceMap;

    public SpaceConstraints(MZSpaceMap spaceMap, int maxKeys, int maxSwitches) : base(spaceMap.NumberSpaces(), maxKeys, maxSwitches)
    {
        this.spaceMap = spaceMap;
    }

    protected override bool ValidRoomCoords(Vector2Int c)
    {
        return spaceMap.Get(c);
    }

    public override List<int> InitialRooms()
    {
        List<int> ids = new List<int>();
        foreach (Vector2Int xy in spaceMap.GetBottomSpaces())
        {
            ids.Add(GetRoomId(xy));
        }
        return ids;
    }
}
