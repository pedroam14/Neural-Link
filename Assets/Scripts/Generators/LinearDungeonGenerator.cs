using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/**
 * Extends MZDungeonGenerator to choose the least nonlinear one immediately
 * available. This saves the player from having to do a lot of backtracking.
 * 
 * Ignores switches for now.
 *
 */
public class LinearDungeonGenerator : DungeonGenerator
{
    public StreamWriter dungeonPerformance;
    public int maxGenerations;
    public bool debug = true;

    public LinearDungeonGenerator(int seed, IMZDungeonConstraints constraints, int nOfRooms, int maxGenerations) : base(seed, constraints, nOfRooms)
    {
        this.maxGenerations = maxGenerations;
    }

    private class AStarHelper
    {
        MZDungeon dungeon;
        int keyLevel;

        public AStarHelper(MZDungeon _dungeon, int _keyLevel)
        {
            this.dungeon = _dungeon;
            this.keyLevel = _keyLevel;
        }
        public List<int> GetNeighbours(int roomId)
        {
            List<int> ids = new List<int>();
            foreach (MZEdge edge in dungeon.Get(roomId).GetEdges())
            {
                if (!edge.HasSymbol() || edge.GetSymbol().GetValue() < keyLevel)
                {
                    ids.Add(edge.GetTargetRoomId());
                }
            }
            return ids;
        }
    }
    private List<int> astar(int start, int goal, int keyLevel, MZDungeon dungeon)
    {
        AStar<int> astar = new AStar<int>(new AStarHelper(dungeon, keyLevel), start, goal, dungeon);
        return astar.Solve();
    }
    private class AStar<T>
    {
        List<int> path, closedSet, currentNeighbours;
        private Dictionary<int, float> openSet;
        private AStarHelper aStarHelper;
        private int start;
        private int goal;
        private int current;
        private MZDungeon dungeon;

        public AStar(AStarHelper aStarHelper, int start, int goal, MZDungeon dungeon)
        {
            this.aStarHelper = aStarHelper;
            this.start = start;
            this.goal = goal;
            this.dungeon = dungeon;
            this.current = start;
            openSet = new Dictionary<int, float>();
            closedSet = new List<int>();
        }

        public List<int> Solve()
        {

            openSet.Add(start, 0);
            int testerino = openSet.Count;
            while (openSet.Count != 0 && !closedSet.Contains(goal))
            {
                current = openSet.First().Key;
                openSet.Remove(current);
                closedSet.Add(current);
                currentNeighbours = aStarHelper.GetNeighbours(current);
                float distance;
                foreach (int neighbour in currentNeighbours)
                {
                    if (!closedSet.Contains(neighbour))
                    {
                        if (!openSet.ContainsKey(neighbour))
                        {
                            dungeon.Get(neighbour).pathfindingParentID = current;
                            distance = (float)Distance(dungeon.Get(neighbour).GetCenter(), dungeon.Get(goal).GetCenter());
                            openSet.Add(neighbour, distance);
                            openSet.OrderBy(x => x.Value);
                        }
                    }
                }
            }
            if (!closedSet.Contains(goal))
            {
                return null;
            }
            path = closedSet;
            return path;
        }
        internal double Distance(Vector2Int v1, Vector2Int v2)
        {
            double distance = Math.Sqrt(Math.Pow(v1.x - v2.x, 2) + Math.Pow(v1.y - v2.y, 2));
            return distance;
        }
    }
    /* 
    private int keyLevel;

    public AStarClient(int keyLevel)
    {
        this.keyLevel = keyLevel;
    }

    public override List<int> GetNeighbors(int roomId)
    {
        List<int> ids = new List<int>();
        foreach (MZEdge edge in dungeon.Get(roomId).GetEdges())
        {
            if (!edge.HasSymbol() || edge.GetSymbol().GetValue() < keyLevel)
            {
                ids.Add(edge.GetTargetRoomId());
            }
        }
        return ids;
    }
    public override Vector2Int GetVector2Int(int roomId)
    {
        return dungeon.Get(roomId).GetCenter();
    }
    }
    */
    /* 
    private List<int> AStar(int start, int goal, int keyLevel)
    {
        AStar<int> astar = new AStar<int>();
        return astar.Solve();
    }
    */

    /**
     * Nonlinearity is measured as the number of rooms the player would have to
     * pass through multiple times to Get to the goal room (collecting keys and
     * unlocking doors along the way).
     * 
     * Uses A* to find a path from the entry to the first key, from each key to
     * the next key and from the last key to the goal.
     * 
     * @return  The number of rooms passed through multiple times
     **/
    public int MeasureNonlinearity()
    {
        List<MZRoom> keyRooms = new List<MZRoom>(constraints.GetMaxKeys());
        for (int i = 0; i < constraints.GetMaxKeys(); ++i)
        {
            keyRooms.Add(null);
        }
        foreach (MZRoom room in dungeon.GetRooms())
        {
            if (room.GetItem() == null) continue;
            MZSymbol item = room.GetItem();
            if (item.GetValue() >= 0 && item.GetValue() < keyRooms.Count)
            {
                keyRooms.Insert(item.GetValue(), room);
            }
        }
        //for N >= 0: keyRooms[N] = location of key N

        MZRoom current = dungeon.FindStart(), goal = dungeon.FindGoal();
        //clients may disable generation of the goal room if redundant, in which case the equivalent 'ending' room becomes the boss room
        if (goal == null) goal = dungeon.FindBoss();
        Debug.Assert(current != null && goal != null);
        int nextKey = 0, nonlinearity = 0;

        List<int> visitedRooms = new List<int>();
        while (current != goal)
        {
            MZRoom intermediateGoal;

            //Debug.Log("Current room ID: " + current.id);
            //Debug.Log("Max Keys:" + constraints.GetMaxKeys());
            //Debug.Log("Next Key: " + nextKey);

            if (nextKey == constraints.GetMaxKeys())
            {
                intermediateGoal = goal;
            }

            else
            {
                intermediateGoal = keyRooms[nextKey];
            }
            //Debug.Log("Current goal ID: " + intermediateGoal.id);
            ///*
            //Debug.Log("Dungeon: " + dungeon.GetType());
            //*/
            //Debug.Log("A* running...");
            List<int> steps = astar(current.id, intermediateGoal.id, nextKey, dungeon); //ちゃんとこれを作ってよ
            //Debug.Log("Reversing steps!");
            steps.Reverse();

            foreach (int id in steps)
            {
                Debug.Log("Visited Room: " + id);
                if (visitedRooms.Contains(id)) ++nonlinearity;
            }
            visitedRooms.AddRange(steps);

            nextKey++;
            current = dungeon.Get(steps[0]);
            MZRoom test = current;
        }
        return nonlinearity;
    }
    public override void Generate()
    {
        dungeonPerformance = new StreamWriter("dungeon.csv");
        dungeonPerformance.WriteLine("generation,non-linearity");
        int generation = 0, currentNonlinearity = int.MaxValue;
        int bestAttempt = 0;
        MZDungeon currentBest = null;
        while (currentNonlinearity > 12 && generation < maxGenerations)
        {
            generation++;
            //Debug.Log("Generation: " + generation);
            base.Generate();
            //Debug.Log("Method base.Generate() has finished running, now measuring non-linearity.");
            int nonlinearity = MeasureNonlinearity();
            Debug.Log("Dungeon generation " + generation + " nonlinearity: " + nonlinearity);
            if (nonlinearity < currentNonlinearity)
            {
                currentNonlinearity = nonlinearity;
                bestAttempt = generation;
                currentBest = dungeon;
            }
            dungeonPerformance.WriteLine(generation + "," + currentNonlinearity);
        }
        dungeonPerformance.WriteLine(bestAttempt + "," + currentNonlinearity);
        dungeonPerformance.Close();
        Debug.Assert(currentBest != null);
        Debug.Log("Chose " + bestAttempt + " nonlinearity: " + currentNonlinearity);

        dungeon = currentBest;
    }
}
//*/
