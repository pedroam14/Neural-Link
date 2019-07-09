using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * Represents the spacial layout of a lock-and-key puzzle and contains all
 * {@link MZSymbol}s, {@link MZRoom}s and {@link MZEdge}s within the puzzle.
 */
public interface IMZDungeon {
    /**
     * @return  the rooms within the dungeon
     */
    Dictionary<int, MZRoom>.ValueCollection GetRooms();

    /**
     * @return the number of rooms in the dungeon
     */
    int RoomCount();

    /**
     * @param id        the id of the room
     * @return  the room with the given id
     */
    MZRoom Get(int id);

    /**
     * Adds a new room to the dungeon, overwriting any rooms already in it that
     * have the same coordinates.
     * 
     * @param room  the room to Add
     */
    void Add(MZRoom room);

    /**
     * Adds a one-way unconditional edge between the given rooms.
     * A one-way edge may be used to travel from room1 to room2, but not room2
     * to room1.
     * 
     * @param room1 the first room to link
     * @param room2 the second room to link
     */
    void LinkOneWay(MZRoom room1, MZRoom room2);

    /**
     * Adds a two-way unconditional edge between the given rooms.
     * A two-way edge may be used to travel from each room to the other.
     * 
     * @param room1 the first room to link
     * @param room2 the second room to link
     */
    void Link(MZRoom room1, MZRoom room2);

    /**
     * Adds a one-way conditional edge between the given rooms.
     * A one-way edge may be used to travel from room1 to room2, but not room2
     * to room1.
     * 
     * @param room1 the first room to link
     * @param room2 the second room to link
     * @param cond  the condition on the edge
     */
    void LinkOneWay(MZRoom room1, MZRoom room2, MZSymbol cond);
    /**
     * Adds a two-way conditional edge between the given rooms.
     * A two-way edge may be used to travel from each room to the other.
     * 
     * @param room1 the first room to link
     * @param room2 the second room to link
     * @param cond  the condition on the edge
     */
    void Link(MZRoom room1, MZRoom room2, MZSymbol cond);
    /**
     * Tests whether two rooms are linked.
     * Two rooms are linked if there are any edges (in any direction) between
     * them.
     * 
     * @return  true if the rooms are linked, false otherwise
     */
    bool RoomsAreLinked(MZRoom room1, MZRoom room2);
    
    /**
     * @return  the room containing the START symbol
     */
    MZRoom FindStart();
    /**
     * @return  the room containing the BOSS symbol
     */
    MZRoom FindBoss();
    /**
     * @return  the room containing the GOAL symbol
     */
    MZRoom FindGoal();
    /**
     * @return  the room containing the Switch symbol
     */
    MZRoom FindSwitch();

    /**
     * Gets the {@link MZRect} that encloses every room within the dungeon.
     * <p>
     * The Bounds object has the X coordinate of the West-most room, the Y
     * coordinate of the North-most room, the 'right' coordinate of the
     * East-most room, and the 'bottom' coordinate of the South-most room.
     * 
     * @return  the rectangle enclosing every room within the dungeon
     */
    MZRect GetExtentBounds();
}
