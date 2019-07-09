#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEditor;
using System;

public class OverworldAutomata : MonoBehaviour
{

    #region "Perlin Generation"
    public Vector2Int mapSize;
    [Range(0, 100)]
    public int scale;

    /*
    each octave adds a layer of detail to the perlin noise surface, e.g. ectave 1 could be mountains, 2 could be bouldes, 3 could be rocks and so on
     */
    [Range(1, 12)]
    public int octaves;
    [Range(0, 1)]
    public float persistence;

    System.Random rdm;

    /*
    lacunarity of more than 1 means that each octave will increase its level of fine grained detail (increased frqeuency), lacunarity of 1 means that each octave will have the sam level of detail and lacunarity of less than one means that each octave will get smoother 
    the last two are *usually* undesirable so a lacunarity of 2 works most of the time
    */
    [Range(1, 10)]
    public int lacunarity;

    float[,] floatMap;
    int[,] intMap;
    string mapToString;

    private void PerlinGeneration(Vector2Int mapSize, int scale, int octaves, float persistence, int lacunarity)
    {
        rdm = new System.Random();
        float random = (float)rdm.NextDouble();
        floatMap = new float[mapSize.x, mapSize.y];
        intMap = new int[mapSize.x,mapSize.y];
        for (int x = 0; x < mapSize.x; x++)
        {
            for (int y = 0; y < mapSize.y; y++)
            {
                //Debug.Log((float)x / mapSize.x + "," + (float)y / mapSize.y);
                floatMap[x, y] = PerlinNoise.Fbm((float)x / mapSize.x, (float)y / mapSize.y, octaves, lacunarity, persistence * random) + (Mathf.PerlinNoise((float)x / mapSize.x, (float)y / mapSize.y)-0.5f)*random;
                //Debug.Log((float)x/map.x+","+(float)y/map.y);

                // mapToString += floatMap[x, y] + " ";


                if (floatMap[x, y] < -0.05f)
                {
                    intMap[x, y] = 0;
                }
                else if (floatMap[x, y] >= -0.05f && floatMap[x, y] < 0f)
                {
                    intMap[x, y] = 2;
                }
                else
                {
                    intMap[x, y] = 1;
                }

            }
            //mapToString += "\n";
        }
        for (int x = 0; x < mapSize.x; x++)
        {
            for (int y = 0; y < mapSize.y; y++)
            {
                if (intMap[x, y] == 0)
                {
                    topMap.SetTile(new Vector3Int(-x + width / 2, -y + height / 2, 0), topTile);
                }
                else
                {
                    botMap.SetTile(new Vector3Int(-x + width / 2, -y + height / 2, 0), botTile);
                }

            }
        }
        //Debug.Log(mapToString);
        mapToString = "";
    }

    #endregion

    #region "Cellular Automata Generation"
    [Range(0, 100)]
    public int iniChance;
    [Range(1, 8)]
    public int birthLimit;
    [Range(1, 8)]
    public int deathLimit;

    [Range(1, 10)]
    public int numR;
    private int count = 0;

    private int[,] terrainMap;
    public Vector3Int tmpSize;
    public Tilemap topMap;
    public Tilemap botMap;
    public RuleTile topTile;
    public RuleTile botTile;

    int width;
    int height;

    public void doSim(int nu)
    {
        clearMap(false);
        width = tmpSize.x;
        height = tmpSize.y;

        if (terrainMap == null)
        {
            terrainMap = new int[width, height];
            initPos();
        }


        for (int i = 0; i < nu; i++)
        {
            terrainMap = genTilePos(terrainMap);
        }

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (terrainMap[x, y] == 1)
                {
                    topMap.SetTile(new Vector3Int(-x + width / 2, -y + height / 2, 0), topTile);
                }
                else
                {
                    botMap.SetTile(new Vector3Int(-x + width / 2, -y + height / 2, 0), botTile);
                }
            }
        }


    }

    public void initPos()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                terrainMap[x, y] = UnityEngine.Random.Range(1, 101) < iniChance ? 1 : 0;
            }

        }

    }

    public int[,] genTilePos(int[,] oldMap)
    {
        int[,] newMap = new int[width, height];
        int neighb;
        BoundsInt myB = new BoundsInt(-1, -1, 0, 3, 3, 1);


        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                neighb = 0;
                foreach (var b in myB.allPositionsWithin)
                {
                    if (b.x == 0 && b.y == 0) continue;
                    if (x + b.x >= 0 && x + b.x < width && y + b.y >= 0 && y + b.y < height)
                    {
                        neighb += oldMap[x + b.x, y + b.y];
                    }
                    else
                    {
                        neighb++;
                    }
                }

                if (oldMap[x, y] == 1)
                {
                    if (neighb < deathLimit) newMap[x, y] = 0;

                    else
                    {
                        newMap[x, y] = 1;

                    }
                }

                if (oldMap[x, y] == 0)
                {
                    if (neighb > birthLimit) newMap[x, y] = 1;

                    else
                    {
                        newMap[x, y] = 0;
                    }
                }

            }

        }



        return newMap;
    }
    #endregion

    void Update()
    {

        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log("Constructing map.");
            //cellular automata generation
            doSim(numR);
            //perlin noise generation
            //PerlinGeneration(mapSize, scale, octaves, persistence, lacunarity);
        }


        if (Input.GetMouseButtonDown(1))
        {
            clearMap(true);
        }



        if (Input.GetMouseButton(2))
        {
            SaveAssetMap();
            count++;
        }
    }


    public void SaveAssetMap()
    {
        string saveName = "tmapXY_" + count;
        var mf = GameObject.Find("Grid");

        if (mf)
        {
            var savePath = "Assets/" + saveName + ".prefab";
            if (PrefabUtility.CreatePrefab(savePath, mf))
            {
                EditorUtility.DisplayDialog("Tilemap saved", "Your Tilemap was saved under" + savePath, "Continue");
            }
            else
            {
                EditorUtility.DisplayDialog("Tilemap NOT saved", "An ERROR occured while trying to saveTilemap under" + savePath, "Continue");
            }


        }


    }

    public void clearMap(bool complete)
    {
        topMap.ClearAllTiles();
        botMap.ClearAllTiles();
        if (complete)
        {
            terrainMap = null;
        }
    }
}
#endif