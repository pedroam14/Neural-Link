#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
namespace DE.DungeonEditor
{
    public class GeneralDungeonStats : BaseNode
    {
        public int nOfRooms = 0;
        public int difficulty;

        string[] difficultyString = new string[]
{
     "Easy", "Medium", "Hard",
};

        public override void DrawWindow()
        {
            nOfRooms = EditorGUILayout.IntField("Number of Rooms:", nOfRooms);
            difficulty = EditorGUILayout.Popup("Difficulty", difficulty, difficultyString);
        }
    }
}
#endif