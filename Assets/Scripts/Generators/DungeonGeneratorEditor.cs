#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections;
[CustomEditor(typeof(DungeonMaster), true)]
public class DungeonGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        DungeonMaster dungeon = (DungeonMaster)target;
        if (GUILayout.Button("Build Dungeon"))
        {
            if (!dungeon.dungeonIsBuilt)
            {
                dungeon.CreateDungeon();
                dungeon.dungeonIsBuilt = true;
            }
            else
            {
                Debug.Log("Clear the scene before making another dungeon!");
            }
        }
        if (GUILayout.Button("Delete Dungeon"))
        {
            if (dungeon.dungeonIsBuilt)
            {
                dungeon.ClearScene();
                dungeon.dungeonIsBuilt = false;
            }
            else{
                Debug.Log("Nothing to clear!");
            }
        }
    }
}
#endif