using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;

public class ColorMap {
    
    protected int xsum, ysum, xmin, xmax, ymin, ymax;
    protected Dictionary<Vector2Int, int> map;

    public ColorMap() {
        map = new Dictionary<Vector2Int, int>();
        ymin = xmin = Int32.MaxValue;
        ymax = xmax = Int32.MinValue;
    }
    
    public void Set(int x, int y, int color) {
        Vector2Int xy = new Vector2Int(x,y);

        if (map.ContainsKey(xy) == false) {
            xsum += x;
            ysum += y;
        }
        map[xy] = color;
        
        if (x < xmin) xmin = x;
        if (x > xmax) xmax = x;
        if (y < ymin) ymin = y;
        if (y > ymax) ymax = y;
    }
    
    public int Get(int x, int y) {
        return map[new Vector2Int(x,y)];
    }
    
    public Vector2Int GetCenter() {
        return new Vector2Int(xsum/map.Count, ysum/map.Count);
    }
    
    public int GetWidth() {
        return xmax-xmin+1;
    }
    
    public int GetHeight() {
        return ymax-ymin+1;
    }
    
    public int GetLeft() {
        return xmin;
    }
    
    public int GetTop() {
        return ymin;
    }
    
    public int GetRight() {
        return xmax;
    }
    
    public int GetBottom() {
        return ymax;
    }
    
    protected bool IsConnected() {
        if (map.Count == 0) return false;

        // Do a breadth first search starting at the top left to check if
        // every position is reachable.
        List<Vector2Int> world = new List<Vector2Int>(map.Keys);
        Queue<Vector2Int> queue = new Queue<Vector2Int>();

        Vector2Int first = world[0];
        world.RemoveAt(0);
        queue.Enqueue(first);
        Vector2Int[] directions = new Vector2Int[] { Vector2Int.up, Vector2Int.right, Vector2Int.down, Vector2Int.left };
        
        while (queue.Count > 0) {
            Vector2Int pos = queue.Dequeue();
            
            foreach (Vector2Int d in directions) {
                Vector2Int neighbor = pos + d;
                
                if (world.Contains(neighbor)) {
                    world.Remove(neighbor);
                    queue.Enqueue(neighbor);
                }
            }
        }
        
        return world.Count == 0;
    }
    
    public void CheckConnected() {
        if (!IsConnected()) {
            // Parts of the map are unreachable!
            throw new MZGenerationFailureException("ColorMap is not fully connected");
        }
    }
    
}
