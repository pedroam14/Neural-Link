#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
namespace DE.DungeonEditor
{
    public class KeyRoomNode : BaseNode
    {
        public string keyName = "";
        public override void DrawWindow()
        {
            EditorGUILayout.LabelField("Key:");
            keyName = GUILayout.TextArea(keyName.ToString(), 1);
        }
        public override void DrawCurve()
        {
            base.DrawCurve();
        }
    }
}
#endif
