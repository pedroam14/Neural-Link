#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
namespace DE.DungeonEditor
{
    public class EntranceRoomNode : BaseNode
    {
        string dungeonName = "";
        public override void DrawWindow()
        {
            EditorGUILayout.LabelField("Dungeon Name:");
            dungeonName = GUILayout.TextArea(dungeonName,20);
            base.DrawWindow();
            
        }
        public override void DrawCurve()
        {
            
            base.DrawCurve();
        }
    }
}
#endif