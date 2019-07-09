#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DE;
using UnityEditor;

namespace DE.DungeonEditor
{
    public class LockRoomNode : BaseNode
    {
        public State currentState;
        public string lockName = "";
        public bool isLocked = true;
        public override void DrawWindow()
        {
            if (currentState == null)
            {
                EditorGUILayout.LabelField("Add Room State to Modify:");
                
            }
            else
            {
                EditorGUILayout.LabelField("Current State:");
            }
            currentState = (State) EditorGUILayout.ObjectField(currentState,typeof(State),false);
            EditorGUILayout.LabelField("Key Item Needed:");
            lockName = GUILayout.TextArea(lockName.ToString(), 1);
            isLocked  = GUILayout.Toggle(isLocked,"Locked:");
        }
        public override void DrawCurve()
        {
            base.DrawCurve();
        }
    }
}
#endif