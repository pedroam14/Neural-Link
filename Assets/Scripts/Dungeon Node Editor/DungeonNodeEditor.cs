#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


namespace DE.DungeonEditor
{
    public class DungeonNodeEditor : EditorWindow
    {
        #region Variables
        static List<BaseNode> windows = new List<BaseNode>();
        Vector3 mousePosition;
        bool makeTransition;
        bool clickedOnAWindow;
        BaseNode selectedNode;
        int keys, locks;
        bool dungeonDataCreated = false; //initially false until the user creates the GeneralDungeonStats node
        public enum UserActions
        {
            addLockRoom, addKeyRoom, addEntrance, addBossRoom, addTransitionNode, deleteNode, commentNode, createDungeonStats
        }
        #endregion
        #region Init
        [MenuItem("Dungeon Editor/Editor")]
        static void ShowEditor()
        {
            DungeonNodeEditor editor = EditorWindow.GetWindow<DungeonNodeEditor>();
            editor.minSize = new Vector2(800, 600);
        }
        #endregion
        #region GUIMethods
        private void OnGUI()
        {
            Event e = Event.current;
            mousePosition = e.mousePosition;
            UserInput(e);
            DrawWindows();
        }

        private void OnEnable()
        {
            locks = 0;
            keys = 0;
            dungeonDataCreated = false;
            windows.Clear();
        }

        void DrawWindows()
        {
            BeginWindows();
            foreach (BaseNode n in windows)
            {
                n.DrawCurve();
            }
            for (int i = 0; i < windows.Count; i++)
            {
                windows[i].windowRect = GUI.Window(i, windows[i].windowRect, DrawNodeWindows, windows[i].windowTitle);
            }
            EndWindows();
        }
        void DrawNodeWindows(int id)
        {
            windows[id].DrawWindow();
            GUI.DragWindow();
        }
        void UserInput(Event e)
        {
            if (e.button == 1 && !makeTransition)
            {
                if (e.type == EventType.MouseDown)
                {
                    RightClick(e);
                }
            }
            if (e.button == 0 && !makeTransition)
            {
                if (e.type == EventType.MouseDown)
                {
                    // RightClick(e);
                }
            }
        }
        void RightClick(Event e)
        {

            selectedNode = null;
            for (int i = 0; i < windows.Count; i++)
            {
                if (windows[i].windowRect.Contains(e.mousePosition))
                {
                    selectedNode = windows[i];
                    clickedOnAWindow = true; //means that wherever the player clicked is in a region contained by one of the windows
                    break;
                }
            }
            if (!clickedOnAWindow)
            {
                AddNewNode(e); //means he right clicked on nowhere (mouse button 1) so we should start the add node routine
            }
            else
            {
                ModifyNodes(e); //means he clicked on one of the windows above with mouse button 1 (right click) and therefore wants to change the selected node
            }
            clickedOnAWindow = false;
        }
        void AddNewNode(Event e)
        {
            GenericMenu menu = new GenericMenu();

            menu.AddSeparator("");

            menu.AddItem(new GUIContent("Add Entrance"), false, ContextCallBack, UserActions.addEntrance);
            menu.AddItem(new GUIContent("Add Lock Room"), false, ContextCallBack, UserActions.addLockRoom);
            menu.AddItem(new GUIContent("Add Key Room"), false, ContextCallBack, UserActions.addKeyRoom);
            menu.AddItem(new GUIContent("Add Boss Room"), false, ContextCallBack, UserActions.addBossRoom);
            menu.AddItem(new GUIContent("Add Comment"), false, ContextCallBack, UserActions.commentNode);
            if (!dungeonDataCreated)
            {
                menu.AddItem(new GUIContent("Add Dungeon Data"), false, ContextCallBack, UserActions.createDungeonStats);
            }

            menu.ShowAsContext();
            e.Use();
        }
        void ModifyNodes(Event e)
        {
            GenericMenu menu = new GenericMenu();
            if (selectedNode is KeyRoomNode || selectedNode is LockRoomNode || selectedNode is EntranceRoomNode || selectedNode is BossRoomNode)
            {
                menu.AddSeparator("");
                menu.AddItem(new GUIContent("Add Transition"), false, ContextCallBack, UserActions.addTransitionNode);
                menu.AddSeparator("");
                menu.AddItem(new GUIContent("Delete"), false, ContextCallBack, UserActions.deleteNode);

            }
            if (selectedNode is CommentNode || selectedNode is GeneralDungeonStats)
            {
                menu.AddSeparator("");
                menu.AddItem(new GUIContent("Delete"), false, ContextCallBack, UserActions.deleteNode);
            }
            menu.ShowAsContext();
            e.Use();
        }
        void ContextCallBack(object o)
        {
            UserActions a = (UserActions)o;
            //a lot of the meat of the code goes in here
            switch (a)
            {
                case UserActions.addEntrance:
                    EntranceRoomNode entranceRoom = new EntranceRoomNode()
                    {
                        windowRect = new Rect(mousePosition.x, mousePosition.y, 200, 100),
                        windowTitle = "Dungeon Entrance"
                    };
                    windows.Add(entranceRoom);
                    //now this means a dungeon entrance, think long and hard about the implications this will have on your fitness evaluation down the line
                    break;
                case UserActions.addKeyRoom:
                    KeyRoomNode keyRoom = new KeyRoomNode
                    {
                        windowRect = new Rect(mousePosition.x, mousePosition.y, 200, 100),
                        windowTitle = "Dungeon Key Room " + keys
                    };
                    keys++;
                    windows.Add(keyRoom);
                    //now this means a dungeon key room has been made, think long and hard about the implications this will have on your fitness evaluation down the line 
                    break;
                case UserActions.addLockRoom:
                    LockRoomNode lockRoom = new LockRoomNode
                    {
                        windowRect = new Rect(mousePosition.x, mousePosition.y, 200, 150),
                        windowTitle = "Dungeon Lock Room " + locks
                    };
                    locks++;
                    windows.Add(lockRoom);
                    //now this means a dungeon lock room has been made, think long and hard about the implications this will have on your fitness evaluation down the line
                    break;
                case UserActions.addBossRoom:
                    BossRoomNode bossRoom = new BossRoomNode
                    {
                        windowRect = new Rect(mousePosition.x, mousePosition.y, 200, 100),
                        windowTitle = "Boss Room"
                    };
                    windows.Add(bossRoom);
                    //now this means a dungeon boss room has been made, think long and hard about the implications this will have on your fitness evaluation down the line
                    break;
                case UserActions.createDungeonStats:
                    GeneralDungeonStats stats = new GeneralDungeonStats
                    {
                        windowRect = new Rect(mousePosition.x, mousePosition.y, 300, 150),
                        windowTitle = "Dungeon Data"
                    };                    
                    //now this means a dungeon general data such as difficulty and number of filler rooms has been decided, think long and hard about the implications this will have on your fitness evaluation down the line
                    dungeonDataCreated = true;
                    windows.Add(stats);
                    break;
                case UserActions.addTransitionNode:
                    break;
                case UserActions.deleteNode:
                    if (selectedNode != null)
                    {
                        if (selectedNode is KeyRoomNode)
                        {
                            keys--;
                        }
                        if (selectedNode is LockRoomNode)
                        {
                            locks--;
                        }
                        if (selectedNode is GeneralDungeonStats)
                        {
                            dungeonDataCreated = false;
                        }
                        windows.Remove(selectedNode);
                    }
                    break;
                case UserActions.commentNode:
                    CommentNode comment = new CommentNode
                    {
                        windowRect = new Rect(mousePosition.x, mousePosition.y, 300, 300),
                        windowTitle = "Comment"
                    };
                    windows.Add(comment);
                    break;

            }
        }

        #endregion
        #region HelperMethods
        #endregion

    }
}
#endif