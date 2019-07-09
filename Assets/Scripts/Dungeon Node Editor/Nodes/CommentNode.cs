using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace DE.DungeonEditor
{
    public class CommentNode : BaseNode
    {
        string comment = "";
        public override void DrawWindow()
        {
            comment = GUILayout.TextArea(comment, 200);
        }
    }
}