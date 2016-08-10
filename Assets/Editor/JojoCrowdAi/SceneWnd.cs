using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

namespace JojoCrowdAi
{
    public class SceneWnd
    {
        private int _horizontalCount = 10;
        private int _verticalCount = 10;

        List<Vector2> pointLeft = new List<Vector2>();
        List<Vector2> pointRight = new List<Vector2>();
        List<Vector2> pointTop = new List<Vector2>();
        List<Vector2> pointBottom = new List<Vector2>();
        private int maxWidth = 0;
        private int maxHeight = 0;

        public void Initialize()
        {
            pointLeft.Clear();
            pointRight.Clear();
            pointTop.Clear();
            pointBottom.Clear();

            maxWidth = CrowdAiManager.GetWidth() * MainEditorWnd.delta;
            maxHeight = CrowdAiManager.GetHeight()* MainEditorWnd.delta;

            _horizontalCount = CrowdAiManager.GetHeight() + 1;
            _verticalCount = CrowdAiManager.GetWidth() + 1;

            for (int i = 0; i < _horizontalCount; i++)
            {
                pointLeft.Add(
                    new Vector2(MainEditorWnd.sceneOffect.x, MainEditorWnd.sceneOffect.y + i * MainEditorWnd.delta));

                pointRight.Add(
                    new Vector2(MainEditorWnd.sceneOffect.x + maxWidth, MainEditorWnd.sceneOffect.y + i * MainEditorWnd.delta));
            }

            for (int i = 0; i < _verticalCount; i++)
            {
                pointTop.Add(
                    new Vector2(MainEditorWnd.sceneOffect.x + i * MainEditorWnd.delta, MainEditorWnd.sceneOffect.y));

                pointBottom.Add(
                    new Vector2(MainEditorWnd.sceneOffect.x + i * MainEditorWnd.delta, MainEditorWnd.sceneOffect.y + maxHeight));
            }
        }

        public void DrawSceneDetail()
        {
            if (null == CrowdAiManager.curMapInfo)
                return;

            DrawCheckerboard();
            DrawObstacle();
            DrawMembers();
        }

        public bool IsInMap()
        {
            Vector2 mousePos = Event.current.mousePosition;
            if (mousePos.x < MainEditorWnd.sceneOffect.x)
                return false;

            if (mousePos.x > maxWidth + MainEditorWnd.sceneOffect.x)
                return false;

            if (mousePos.y < MainEditorWnd.sceneOffect.y)
                return false;

            if (mousePos.y > maxHeight + MainEditorWnd.sceneOffect.y)
                return false;

            return true;
        }

        // 画格子.
        private void DrawCheckerboard()
        {
            for (int i = 0; i < _horizontalCount; i++)
            {
                Utils.DrawLine(pointLeft[i], pointRight[i], Color.white);
            }

            for (int i = 0; i < _verticalCount; i++)
            {
                Utils.DrawLine(pointBottom[i], pointTop[i], Color.white);
            }

            // jojohello temp.
            float posX;
            float posY;
            Rect rect = new Rect();
            rect.width = MainEditorWnd.delta;
            rect.height = MainEditorWnd.delta;
            foreach (int key in CrowdAiManager.curMapInfo.grids.Keys)
            {
                posX = CrowdAiManager.GetXFromId(key);
                posY = CrowdAiManager.GetYFromId(key);
                rect.x = (posX) * MainEditorWnd.delta + MainEditorWnd.sceneOffect.x;
                rect.y = (posY) * MainEditorWnd.delta + MainEditorWnd.sceneOffect.y;
                GUI.Label(rect, CrowdAiManager.curMapInfo.grids[key].pho.ToString("#0.0"));
            }
        }

        // 画阻挡物的格子.
        private Rect boxRect = new Rect();
        private void DrawObstacle()
        {
            HashSet<int> obstructions = CrowdAiManager.GetObstructionList();

            foreach (int data in obstructions)
            {
                boxRect.x = CrowdAiManager.GetXFromId(data) * MainEditorWnd.delta + MainEditorWnd.sceneOffect.x;
                boxRect.y = CrowdAiManager.GetYFromId(data) * MainEditorWnd.delta + MainEditorWnd.sceneOffect.y;
                boxRect.width = MainEditorWnd.delta;
                boxRect.height = MainEditorWnd.delta;

                GUI.Label(boxRect, MainEditorWnd.GetObstructionIcon());
            }
        }

        // 画每个成员位置.
        private void DrawMembers()
        {
            if (CrowdAiManager.curMapInfo == null)
                return;

            List<GroupInfo> groups = CrowdAiManager.Groups;
            int count = groups.Count;
            Rect drawRect = new Rect();
            for (int i = 0; i<count; i++)
            {
                GUI.color = groups[i].color;

                int memberCount = groups[i].members.Count;
                for (int j = 0; j < memberCount; j++)
                {
                    drawRect.x = groups[i].members[j].position.x * MainEditorWnd.delta + MainEditorWnd.sceneOffect.x - 8;
                    drawRect.y = groups[i].members[j].position.y * MainEditorWnd.delta + MainEditorWnd.sceneOffect.y - 8;
                    drawRect.width = 16;
                    drawRect.height = 16;
                    GUI.Label(drawRect, MainEditorWnd.GetMemberIcon());
                }
            }

            GUI.color = Color.white;
            groups = null;
        }

        // 
        
    }
}

