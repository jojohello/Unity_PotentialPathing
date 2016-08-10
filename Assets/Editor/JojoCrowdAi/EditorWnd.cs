using UnityEngine;
using System.Collections;
using JetBrains.Annotations;
using UnityEditor;

namespace JojoCrowdAi
{
    public class EditorWnd : Editor
    {
        public enum OperatorState
        {
            None = -1,
            AddObstructor,
            DelObstructor,
            SetSource,
            SetTarget,
        }

        public string[] toolIcons = new string[]
        {
            "AddObstructor",
            "DelObstructor",
            "SetSource",
            "SetTarget",
        };

        //public GUIStyle commandStyle = "Command";

        public OperatorState curOperator = OperatorState.None;
        public bool isOn = false;

        private int inputWidth = 20;
        private int inputHeight = 20;

        private Rect widthRect = new Rect(10, 20, 40, 24);
        private Rect heightRect = new Rect(10, 20, 40, 24);

        private GUILayoutOption[] titlesOption = new GUILayoutOption[]
        {
            GUILayout.MaxWidth(80f),
        };
        private GUILayoutOption[] inputOption = new GUILayoutOption[]
        {
             GUILayout.MaxWidth(120f),
        };

        public void DrawEditorDetail()
        {
            GUILayout.Label("Edit Area");

            GUILayout.Space(10);
            
            GUILayout.BeginHorizontal();
            GUILayout.Label("MapWidth", titlesOption);
            inputWidth = int.Parse(GUILayout.TextField(inputWidth.ToString(), inputOption));

            GUILayout.Space(10f);
            GUILayout.Label("MapHeight", titlesOption);
            GUILayout.Space(4f);
            inputHeight = int.Parse(GUILayout.TextField(inputHeight.ToString(), inputOption));
            //GUILayout.Button()
            GUILayout.EndHorizontal();

            GUILayout.Space(10);
            
            if (GUILayout.Button("Create Map"))
            {
                MainEditorWnd.instance.CreateMap(inputWidth, inputHeight);
            }
            
            if (CrowdAiManager.curMapInfo == null)
                return;

            GUILayout.Space(10);

            int selectedTool = (int)this.curOperator;
            this.curOperator = (OperatorState)GUILayout.Toolbar(selectedTool, toolIcons);

            GUILayout.BeginHorizontal();
            GUILayout.Space(10);
            if (GUILayout.Button("Add Group", new GUILayoutOption[] {GUILayout.Width(80)}))
            {
                
            }

            GUILayout.Space(10);

            if (GUILayout.Button("Del Group", new GUILayoutOption[] { GUILayout.Width(80) }))
            {

            }
            GUILayout.EndHorizontal();

            GUILayout.Space(10);

            //GUILayout.BeginScrollView();

            //GUILayout.EndScrollView();

            GUILayout.Space(10);
            if (GUILayout.Button("Reset Position"))
            {
                ResetGroupPos();
            }

            GUILayout.Space(10);
            if (isOn == false)
            {
                if (GUILayout.Button("start process"))
                {
                    CrowdAiManager.OnStartProcess();
                    isOn = true;
                }
            }
            else
            {
                if (GUILayout.Button("stop process"))
                {
                    isOn = false;
                    ResetGroupPos();
                }
            }
        }

        private void SetGroupPos(GroupInfo group, Vector3 v)
        {
            int memberCount = group.members.Count;
            // jojohello temp
            //for (int i = 0; i < memberCount; i++)
            //{
            //    group.members[i].position = v;
            //}
        }

        private void ResetGroupPos()
        {
            int count = CrowdAiManager.Groups.Count;
            for (int i = 0; i < count; i++)
            {
                SetGroupPos(CrowdAiManager.Groups[i], CrowdAiManager.Groups[i].startPoint);
            }
        }
    }
}

