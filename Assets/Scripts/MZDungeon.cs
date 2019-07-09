using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;

/**
 * @see IMZDungeon
 * 
 * Due to the fact it uses MZIntMap to store the rooms, it makes the assumption
 * that room ids are low in value, tight in range, and all positive.
 */
public class MZDungeon : IMZDungeon
{

    protected int itemCount;
    protected MZIntMap<MZRoom> rooms;
    protected MZRect bounds;

    public MZDungeon()
    {
        rooms = new MZIntMap<MZRoom>();
        bounds = new MZRect(Int32.MaxValue, Int32.MaxValue,
                Int32.MinValue, Int32.MinValue);
    }

    public MZRect GetExtentBounds()
    {
        return bounds;
    }

    public Dictionary<int, MZRoom>.ValueCollection GetRooms()
    {
        return rooms.Values;
    }

    public int RoomCount()
    {
        return rooms.Count;
    }

    public MZRoom Get(int id)
    {
        if (rooms.ContainsKey(id))
        {
            return rooms[id];
        }
        else
        {
            return null;
        }
    }

    public void Add(MZRoom room)
    {
        rooms[room.id] = room;

        foreach (Vector2Int xy in room.GetCoords())
        {
            if (xy.x < bounds.Left)
            {
                bounds = new MZRect(xy.x, bounds.Top,
                        bounds.Right, bounds.Bottom);
            }
            if (xy.x >= bounds.Right)
            {
                bounds = new MZRect(bounds.Left, bounds.Top,
                        xy.x + 1, bounds.Bottom);
            }
            if (xy.y < bounds.Top)
            {
                bounds = new MZRect(bounds.Left, xy.y,
                        bounds.Right, bounds.Bottom);
            }
            if (xy.y >= bounds.Bottom)
            {
                bounds = new MZRect(bounds.Left, bounds.Top,
                        bounds.Right, xy.y + 1);
            }
        }
    }

    public void LinkOneWay(MZRoom room1, MZRoom room2)
    {
        LinkOneWay(room1, room2, null);
    }

    public void Link(MZRoom room1, MZRoom room2)
    {
        Link(room1, room2, null);
    }

    public void LinkOneWay(MZRoom room1, MZRoom room2, MZSymbol cond)
    {
        room1.SetEdge(room2.id, cond);
    }

    public void Link(MZRoom room1, MZRoom room2, MZSymbol cond)
    {
        LinkOneWay(room1, room2, cond);
        LinkOneWay(room2, room1, cond);
    }

    public bool RoomsAreLinked(MZRoom room1, MZRoom room2)
    {
        return room1.GetEdge(room2.id) != null || room2.GetEdge(room1.id) != null;
    }

    public MZRoom FindStart()
    {
        foreach (MZRoom room in GetRooms())
        {
            if (room.IsStart()) return room;
        }
        return null;
    }

    public MZRoom FindBoss()
    {
        foreach (MZRoom room in GetRooms())
        {
            if (room.IsBoss()) return room;
        }
        return null;
    }

    public MZRoom FindGoal()
    {
        foreach (MZRoom room in GetRooms())
        {
            if (room.IsGoal()) return room;
        }
        return null;
    }

    public MZRoom FindSwitch()
    {
        foreach (MZRoom room in GetRooms())
        {
            if (room.IsSwitch()) return room;
        }
        return null;
    }

}
