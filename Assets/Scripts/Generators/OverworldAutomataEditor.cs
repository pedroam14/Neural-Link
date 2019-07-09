#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections;
[CustomEditor(typeof(ImprovedOverworldAutomata))]
public class OverworldAutomataEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        ImprovedOverworldAutomata overAutomata = (ImprovedOverworldAutomata)target;
        if (GUILayout.Button("Build World"))
        {
            if (!overAutomata.mapDrawn)
            {
                overAutomata.GenerateMap();
                overAutomata.DrawMap();
                overAutomata.mapDrawn = true;
            }
        }
        if (GUILayout.Button("Delete World"))
        {
            if (overAutomata.mapDrawn)
            {
                overAutomata.ClearMap();
                overAutomata.mapDrawn = false;
            }
        }
    }
}
#endif