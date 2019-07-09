#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


namespace DE.DungeonEditor
{
    public class BossRoomNode : BaseNode
    {
        public string bossName = "";
        public override void DrawWindow()
        {
            base.DrawWindow();
            EditorGUILayout.LabelField("Boss Name:");
            bossName = GUILayout.TextArea(bossName, 200);
        }
        public override void DrawCurve()
        {
            base.DrawCurve();
        }
    }
}
#endif
