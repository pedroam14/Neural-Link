using UnityEngine;
using System.Collections;
using System;
using UnityEngine.Tilemaps;
using System.Threading.Tasks;
public class ImprovedOverworldAutomata : MonoBehaviour
{

    public Vector2Int mapSize;

    public string seed;
    public bool useRandomSeed;

    [Range(0, 100)]
    public int randomFillPercent;
    [Range(0, 1)]
    public float radialFactor;
    public bool quadraticGradient = false;
    public bool perlin = false;

    int[,] map;
    [Range(0, 10)]
    public int smoothingIteractions;
    public Tilemap topMap;

    public Tilemap botMap;
    public RuleTile topTile;
    public RuleTile botTile;
    public RuleTile edgeTile;
    public bool mapDrawn = false;

    void Start()
    {
        //GenerateMap();
    }

    void Update()
    {
        /* 
        if (Input.GetMouseButtonDown(0))
        {
            if (!mapDrawn)
            {
                GenerateMap();
                DrawMap();
                mapDrawn = true;
            }
        }
        if (Input.GetMouseButtonDown(1))
        {
            ClearMap();
            mapDrawn = false;
        }
        */
    }

    public void GenerateMap()
    {
        map = new int[mapSize.x, mapSize.y];
        RandomFillMap();

        for (int i = 0; i < smoothingIteractions; i++)
        {
            SmoothMap();
        }
    }


    void RandomFillMap()
    {
        if (useRandomSeed)
        {
            seed = Time.time.ToString();
        }

        System.Random pseudoRandom = new System.Random(seed.GetHashCode());
        if (!perlin)
            for (int x = 0; x < mapSize.x; x++)
            {
                for (int y = 0; y < mapSize.y; y++)
                {
                    if (x == 0 || x == mapSize.x - 1 || y == 0 || y == mapSize.y - 1)
                    {
                        map[x, y] = 2;
                    }
                    else
                    {
                        map[x, y] = ((float)pseudoRandom.Next(0, 100) * RadialGradient((float)x, (float)y) < randomFillPercent) ? 1 : 0;
                    }

                }
            }
        else
        {
            for (int x = 0; x < mapSize.x; x++)
            {
                for (int y = 0; y < mapSize.y; y++)
                {
                    if (x == 0 || x == mapSize.x - 1 || y == 0 || y == mapSize.y - 1)
                    {
                        map[x, y] = 2;
                    }
                    else
                    {
                        map[x, y] = pseudoRandom.NextDouble() * RadialGradient((float)x, (float)y) < Mathf.PerlinNoise((float)x / (float)mapSize.x, (float)y / mapSize.y) ? 1 : 0;
                    }
                }
            }
        }
    }

    private float RadialGradient(float x, float y)
    {
        float dist;
        dist = (float)Math.Sqrt(Math.Pow(x - (float)mapSize.x / 2, 2) + Math.Pow(y - (float)mapSize.y / 2, 2));
        if (quadraticGradient)
        {
            return ((float)Math.Pow(mapSize.magnitude * radialFactor / dist, 2));
        }
        else
        {
            return mapSize.magnitude * radialFactor / dist;
        }

    }

    void SmoothMap()
    {
        Parallel.For(0, mapSize.x, x =>
             {
                 Parallel.For(0, mapSize.y, y =>
                  {
                      int neighbourWallTiles = GetSurroundingWallCount(x, y);

                      if (neighbourWallTiles > 4)
                      {
                          map[x, y] = 1;
                      }
                      else if (neighbourWallTiles < 4)
                      {
                          map[x, y] = 0;
                      }

                  });
             });
    }

    int GetSurroundingWallCount(int gridX, int gridY)
    {
        int wallCount = 0;
        for (int neighbourX = gridX - 1; neighbourX <= gridX + 1; neighbourX++)
        {
            for (int neighbourY = gridY - 1; neighbourY <= gridY + 1; neighbourY++)
            {
                if (neighbourX >= 0 && neighbourX < mapSize.x && neighbourY >= 0 && neighbourY < mapSize.y)
                {
                    if (neighbourX != gridX || neighbourY != gridY)
                    {
                        wallCount += map[neighbourX, neighbourY];
                    }
                }
                else
                {
                    wallCount++;
                }
            }
        }

        return wallCount;
    }

    public void ClearMap()
    {
        topMap.ClearAllTiles();
        botMap.ClearAllTiles();
        map = new int[mapSize.x, mapSize.y];
    }

    public void DrawMap()
    {
        for (int x = 0; x < mapSize.x; x++)
        {
            for (int y = 0; y < mapSize.y; y++)
            {
                if (map[x, y] == 1)
                {
                    topMap.SetTile(new Vector3Int(-x + mapSize.x / 2, -y + mapSize.y / 2, 0), topTile);

                }
                else if (map[x, y] == 2)
                {
                    topMap.SetTile(new Vector3Int(-x + mapSize.x / 2, -y + mapSize.y / 2, 0), edgeTile);
                }
                else
                {
                    botMap.SetTile(new Vector3Int(-x + mapSize.x / 2, -y + mapSize.y / 2, 0), botTile);
                }
            }
        }
    }
}