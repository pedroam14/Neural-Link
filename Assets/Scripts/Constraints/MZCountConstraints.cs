using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;

/**
 * Limits the {@link generators.IMZDungeonGenerator} in
 * the <i>number</i> of keys, switches and rooms it is allowed to place.
 *
 * Also restrict to a grid of 1x1 rooms.
 *
 * @see IMZDungeonConstraints
 */
public class CountConstraints : IMZDungeonConstraints {

    protected int maxSpaces, maxKeys, maxSwitches;
    
    protected MZIntMap<Vector2Int> gridCoords;
    protected Dictionary<Vector2Int, int> roomIds;
    protected int firstRoomId;
    
    public CountConstraints(int maxSpaces, int maxKeys, int maxSwitches) {
        this.maxSpaces = maxSpaces;
        this.maxKeys = maxKeys;
        this.maxSwitches = maxSwitches;

        gridCoords = new MZIntMap<Vector2Int>();
        roomIds = new Dictionary<Vector2Int, int>();
        Vector2Int first = new Vector2Int(0,0);
        firstRoomId = GetRoomId(first);
    }
    
    public int GetRoomId(Vector2Int xy) {
        if (roomIds.ContainsKey(xy)) {
            return roomIds[xy];
        } else {
            int id = gridCoords.NewInt();
            gridCoords[id] = xy;
            roomIds[xy] = id;
            return id;
        }
    }
    
    public Vector2Int GetRoomCoords(int id) {
        return gridCoords[id];
    }
    
    public int GetMaxRooms() {
        return maxSpaces;
    }
    
    public void SetMaxSpaces(int maxSpaces) {
        this.maxSpaces = maxSpaces;
    }
    
    public virtual List<int> InitialRooms() {
        return new List<int> { firstRoomId };
    }

    public int GetMaxKeys() {
        return maxKeys;
    }
    
    public void SetMaxKeys(int maxKeys) {
        this.maxKeys = maxKeys;
    }
    
    public bool IsAcceptable(IMZDungeon dungeon) {
        return true;
    }

    public int GetMaxSwitches() {
        return maxSwitches;
    }

    public void SetMaxSwitches(int maxSwitches) {
        this.maxSwitches = maxSwitches;
    }

    protected virtual bool ValidRoomCoords(Vector2Int c) {
        return c.y >= 0;
    }
    
    public List<KeyValuePair<Double, int>> GetAdjacentRooms(int id, int keyLevel) {
        Vector2Int xy = gridCoords[id];
        List<KeyValuePair<Double, int>> ids = new List<KeyValuePair<Double, int>>();
        Vector2Int[] directions = new Vector2Int[] { Vector2Int.up, Vector2Int.right, Vector2Int.down, Vector2Int.left };

        foreach (Vector2Int d in directions) {
            Vector2Int neighbor = xy + d;
            if (ValidRoomCoords(neighbor))
                ids.Add(new KeyValuePair<Double,int>(1.0,GetRoomId(neighbor)));
        }
        return ids;
    }

    public List<Vector2Int> GetCoords(int id) {
        return new List<Vector2Int> { GetRoomCoords(id) };
    }

    public double EdgeGraphifyProbability(int id, int nextId) {
        return 0.2;
    }

    public bool RoomCanFitItem(int id, MZSymbol key) {
        return true;
    }

}
