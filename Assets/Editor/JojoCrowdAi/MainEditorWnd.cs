using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.ProjectWindowCallback;
using System.Collections;
using System.Collections.Generic;
using System;

namespace JojoCrowdAi
{
    public class MainEditorWnd : EditorWindow
    {
        public static MainEditorWnd instance;
        
        private SceneWnd sceneWnd = null;
        private EditorWnd editorWnd = null;

        public static readonly Vector2 sceneOffect = new Vector2(5, 30);
        public static readonly int delta = 30;
        private GUILayoutOption[] sceneAreaOption = new GUILayoutOption[]
        {
            GUILayout.MinWidth(60),
            GUILayout.ExpandHeight(true),
        };
        private GUILayoutOption[] editAreaOption = new GUILayoutOption[]
        {
            GUILayout.ExpandHeight(true),
        };

        private bool isMouseDown = false;

        private float lastTime = 0f;
        private float curTime = 0f;
        private float deltaTime = 0f;
        
        [MenuItem("Jojohello/Crowd Editor")]
        static public void OpenWnd()
        {
            if (null == instance)
            {
                instance = 
                    (MainEditorWnd)EditorWindow.GetWindow(
                        typeof(MainEditorWnd), 
                        false, 
                        "Jojohello Crowd Editor",
                        true);

                instance.Initialize();

                instance.Show();
            }
        }

        public void Initialize()
        {
            sceneWnd = new SceneWnd();
            sceneWnd.Initialize();

            editorWnd = new EditorWnd();

            lastTime = curTime = Time.time;
        }

        void Update()
        {
            curTime = Time.time;
            deltaTime = curTime - lastTime;
            lastTime = curTime;

            if (deltaTime < float.Epsilon)
                deltaTime = 0.03f;

            if (editorWnd == null || editorWnd.isOn == false)
            {
                lastTime = curTime;
                return;
            }
                

            // 重新计算每个组员的位置.
            List<GroupInfo> groups = CrowdAiManager.Groups;
            int count = groups.Count;
            for (int i = 0; i < count; i++)
            {
                int memberCount = groups[i].members.Count;
                for (int j = 0; j < memberCount; j++)
                    groups[i].members[j].DoUpdate(deltaTime);
            }

            // 重新计算每组的消耗，以及每位成员的下一帧速度.
            CrowdAiManager.DoUpdate();

            Repaint();
        }

        void OnGUI()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.BeginVertical("box", sceneAreaOption);

            GUILayout.Label("Scene");

            if (sceneWnd != null)
                sceneWnd.DrawSceneDetail();

            EditorGUILayout.EndVertical();

            GUILayout.Space(4f);

            EditorGUILayout.BeginVertical("box", editAreaOption);

            if (editorWnd != null)
                editorWnd.DrawEditorDetail();

            EditorGUILayout.EndVertical();

            EditorGUILayout.EndHorizontal();

            DealEvent();
        }

        public void CreateMap(int w, int h)
        {
            CrowdAiManager.CreateMap(w, h);

            sceneAreaOption = new GUILayoutOption[]
            {
                GUILayout.ExpandHeight(true),
                GUILayout.Width(CrowdAiManager.GetWidth() * MainEditorWnd.delta + 5),
            };

            if (null != sceneWnd)
                sceneWnd.Initialize();
        }

        private void DealEvent()
        {
            Event curEvent = Event.current;

            if (curEvent.type == EventType.MouseDown && curEvent.button == 0)
            {
                isMouseDown = true;
            }

            if (curEvent.type == EventType.mouseUp && curEvent.button == 0)
            {
                isMouseDown = false;
            }

            if (editorWnd == null || editorWnd.isOn)
                return;

            UpdateMouseDrag();
        }

        public void UpdateMouseDrag()
        {
            if (sceneWnd == null)
                return;

            if (isMouseDown == false)
                return;

            if (sceneWnd.IsInMap() == false)
                return;

            Vector2 mousePos = Event.current.mousePosition - sceneOffect;
            switch (editorWnd.curOperator)
            {
            case EditorWnd.OperatorState.AddObstructor:
                    CrowdAiManager.AddObstruction((int)(mousePos.x / delta), (int)(mousePos.y / delta));
                    Repaint();
                    break;
                case EditorWnd.OperatorState.DelObstructor:
                    CrowdAiManager.DelObstruction((int)(mousePos.x / delta), (int)(mousePos.y / delta));
                    Repaint();
                    break;
            }
        }

        #region command fun
        public void FlexHorizontal(Action func)
        {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            func();
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        public void FlexHorizontal(Action func, float width)
        {
            GUILayout.BeginHorizontal(new GUILayoutOption[]
            {
                GUILayout.Width(width)
            });
            GUILayout.Space(width);
            func();
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        private static Texture2D obstructionIcon = null;
        public static Texture2D GetObstructionIcon()
        {
            if (obstructionIcon == null)
                obstructionIcon = LoadNodeIcon("obstruction");
            return obstructionIcon;
        }

        private static Texture2D memberIcon = null;
        public static Texture2D GetMemberIcon()
        {
            if (memberIcon == null)
                memberIcon = LoadNodeIcon("member");
            return memberIcon;
        }

        public static string InternalResourcesPath = null;
        public static Texture2D LoadNodeIcon(string name)
        {
            if (string.IsNullOrEmpty(InternalResourcesPath))
                SearchForInternalResourcesPath(out InternalResourcesPath);

            if (string.IsNullOrEmpty(InternalResourcesPath))
                return null;

            return (Texture2D)AssetDatabase.LoadAssetAtPath(InternalResourcesPath + name + ".png", typeof(Texture2D));
        }

        private static bool SearchForInternalResourcesPath(out string path)
        {
            path = "";
            string text = "/Editor/JojoCrowdAi/Textures/";
            string text2 = null;
            string[] allAssetPaths = AssetDatabase.GetAllAssetPaths();
            for (int i = 0; i < allAssetPaths.Length; i++)
            {
                string text3 = allAssetPaths[i];
                if (text3.Contains(text))
                {
                    text2 = text3;
                    break;
                }
            }
            bool result;
            if (text2 == null)
            {
                result = false;
            }
            else
            {
                string[] array = text2.Replace(text, "#").Split(new char[]
                {
            '#'
                });
                path = array[0] + text;
                result = true;
            }
            return result;
        }
        #endregion
    }
}
