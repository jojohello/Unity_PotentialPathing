using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace JojoCrowdAi
{
    public partial class CrowdAiManager : MonoBehaviour
    {
        public static MapInfo curMapInfo = null;

        private static List<GroupInfo> groups = new List<GroupInfo>();

        public static List<GroupInfo> Groups {
            get { return groups; }
        }

	    const float minPho = 0.2f;
	    const float MaxPho = 0.6f;

        void Update()
        {
            DoUpdate();
        }

        public static void CreateMap(int width, int height)
        {
            groups.Clear();
            curMapInfo = new MapInfo(width, height);

            // jojohello temp ----------------------------------------------
            groups.Add(new GroupInfo());
            groups[0].endPoint = new Vector3(width - 1, height - 1, 0f);

            for(int i=0;i< 1;i++)
                for (int j = 0; j < 1; j++)
                {
                    Member member = new Member();
                    member.position.x = i + 0.5f;
                    member.position.y = j + 0.5f;

                    groups[0].members.Add(member);
                }

            groups.Add(new GroupInfo());
            groups[1].color = Color.blue;
            groups[1].endPoint = new Vector3(0, 0, 0);
            for (int i = 0; i < 1; i++)
                for (int j = 0; j < 1; j++)
                {
                    Member member = new Member();
                    member.position.x = width - 1 - i + 0.5f;
                    member.position.y = height - 1 - j + 0.5f;

                    groups[1].members.Add(member);
                }

            DoUpdate();
            // -------------------------------------------------------------
        }

        public static int GetWidth()
        {
            if (null == curMapInfo)
                return 0;

            return curMapInfo.width;
        }

        public static int GetHeight()
        {
            if (null == curMapInfo)
                return 0;

            return curMapInfo.height;
        }

        public static HashSet<int> GetObstructionList()
        {
            if (null == curMapInfo)
                return null;

            return curMapInfo.obstructionList;
        }
        
        public static void AddObstruction(int x, int y)
        {
            if (null == curMapInfo)
                return;

            if (x < 0 || x >= GetWidth() || y < 0 || y >= GetHeight())
                return;

            int id = GetIdFromXY(x, y);
            if (curMapInfo.obstructionList.Contains(id))
                return;

            curMapInfo.obstructionList.Add(id);
            GridInfo grid = null;
            if (curMapInfo.grids.TryGetValue(id, out grid))
            {
                grid.isObstruction = true;
            }
        }

        public static void DelObstruction(int x, int y)
        {
            int id = GetIdFromXY(x, y);
            if (curMapInfo.obstructionList.Contains(id) == false)
                return;

            curMapInfo.obstructionList.Remove(id);
            GridInfo grid = null;
            if (curMapInfo.grids.TryGetValue(id, out grid))
            {
                grid.isObstruction = false;
            }
        }

        public static bool IsObstruction(int id)
        {
            if (curMapInfo == null)
                return false;

            return curMapInfo.obstructionList.Contains(id);
        }

        public static int GetXFromId(int id)
        {
            if (curMapInfo == null)
                return -1;

            return id % curMapInfo.width;
        }

        public static int GetYFromId(int id)
        {
            if (curMapInfo == null)
                return -1;

            return id/curMapInfo.width;
        }

        public static int GetIdFromXY(int x, int y)
        {
            if (null == curMapInfo)
                return -1;

            return curMapInfo.PosToId(x, y);
        }

        public static void AddGroup()
        {
            groups.Add(new GroupInfo());
        }

        public static void DelGroup(int index)
        {
            if (index >= groups.Count)
                return;

            groups.RemoveAt(index);
        }
        
        public static void DoUpdate()
        {
            int nCount = groups.Count;

            CaculateDensity();

            for (int i = 0; i < nCount; i++)
            {
                CaculateCost(groups[i]);
                int memberCount = groups[i].members.Count;
                for(int j=0; j<memberCount; j++)
                    UpdateMembersParams(groups[i], groups[i].members[j]);
            }
        }

        public static void OnStartProcess()
        {
            
        }

        // jojohello log
        public static void CaculateDensity()
        {
            if (null == curMapInfo)
                return;

	        foreach (int key in curMapInfo.grids.Keys)
	        {
				curMapInfo.grids[key].pho = 0f;
				curMapInfo.grids[key].velocityAver = Vector3.zero;
			}
                

            int groupCount = groups.Count;
            Member member;
            float exponent = 1f;
            int indexX = 0;
            int indexY = 0;
            int gridId = -1;
            float deltaX = 0f;
            float deltaY = 0f;
            GridInfo grid = null;
	        float tempPho = 0f;
            for (int i = 0; i < groupCount; i++)
            {
                int memberCount = groups[i].members.Count;
                for (int j = 0; j < memberCount; j++)
                {
                    member = groups[i].members[j];
                    indexX = (int)member.position.x;
                    indexY = (int) member.position.y;

                    // 找右下角.
                    if (indexX + 0.5f > member.position.x)
                        indexX -= 1;
                    if (indexY + 0.5f > member.position.y)
                        indexY -= 1;

                    deltaX = member.position.x - (indexX + 0.5f);
                    deltaY = member.position.y - (indexY + 0.5f);

                    gridId = curMapInfo.PosToId(indexX, indexY);
                    if (gridId >= 0)
                    {
                        grid = curMapInfo.grids[gridId];
	                    tempPho = Mathf.Pow(Mathf.Min(1 - deltaX, 1 - deltaY), member.radius);

						grid.pho += tempPho;
						grid.velocityAver += tempPho * member.curVelocity;
					}

                    gridId = curMapInfo.PosToId(indexX + 1, indexY);
                    if (gridId >= 0)
                    {
                        grid = curMapInfo.grids[gridId];
						tempPho = Mathf.Pow(Mathf.Min(deltaX, 1 - deltaY), member.radius);

						grid.pho += tempPho;
						grid.velocityAver += tempPho * member.curVelocity;
					}

                    gridId = curMapInfo.PosToId(indexX, indexY + 1);
                    if (gridId >= 0)
                    {
                        grid = curMapInfo.grids[gridId];
						tempPho = Mathf.Pow(Mathf.Min(1 - deltaX, deltaY), member.radius);

						grid.pho += tempPho;
						grid.velocityAver += tempPho * member.curVelocity;
					}

                    gridId = curMapInfo.PosToId(indexX + 1, indexY + 1);
                    if (gridId >= 0)
                    {
                        grid = curMapInfo.grids[gridId];
						tempPho = Mathf.Pow(Mathf.Min(deltaX, deltaY), member.radius);

						grid.pho += tempPho;
						grid.velocityAver += tempPho * member.curVelocity;
					}
                }
            }
        }

        // 这里计算的是每个组里面的路径消耗.
        private static Dictionary<int, float> endDict = new Dictionary<int, float>();
        private static Dictionary<int, float> prepareDict = new Dictionary<int, float>();
        public static void CaculateCost(GroupInfo group)
        {
            endDict.Clear();
            prepareDict.Clear();

            int id = curMapInfo.PosToId((int)group.endPoint.x, (int)group.endPoint.y);

            if (id < 0)
                return;

            prepareDict.Add(id, 0f);

            int minID = 0;
            float minValue = 0f;
            int curX;
            int curY;
            int nextX;
            int nextY;
            do
            {
                minID = -1;
                minValue = -1;

                foreach (int key in prepareDict.Keys)
                {
                    if (minID == -1)
                    {
                        minID = key;
                        minValue = prepareDict[key];
                        continue;
                    }

                    if (minValue > prepareDict[key])
                    {
                        minID = key;
                        minValue = prepareDict[key];
                    }
                }

                prepareDict.Remove(minID);
                endDict.Add(minID, minValue);

                curX = GetXFromId(minID);
                curY = GetYFromId(minID);
                for (int i = -1; i <= 1; i++)
                    for (int j = -1; j <= 1; j++)
                    {
                        if (Mathf.Abs(Mathf.Abs(i) - Mathf.Abs(j)) < float.Epsilon)
                            continue;

                        if (curX + i < 0 || curX + i >= GetWidth())
                            continue;

                        if (curY + j < 0 || curY + j >= GetHeight())
                            continue;

                        int adjoinId = GetIdFromXY(curX + i, curY + j);

                        if (prepareDict.ContainsKey(adjoinId))
                            continue;

                        if (endDict.ContainsKey(adjoinId))
                            continue;

                        if (IsObstruction(adjoinId))
                        {
                            endDict.Add(adjoinId, float.NaN);
                            continue;
                        }

                        prepareDict.Add(adjoinId, minValue + 1);
                    }
            } while (prepareDict.Count > 0);

            //group.costDict = endDict;

            foreach (int key in curMapInfo.grids.Keys)
                curMapInfo.grids[key].cost = 0;
            
            foreach (int key in endDict.Keys)
            {
                curMapInfo.grids[key].cost = endDict[key]; // 这里可能还包括了
                curMapInfo.grids[key].pathLength = endDict[key];
            }
        }

        public static void UpdateMembersParams(GroupInfo group, Member member)
        {
            if (curMapInfo == null)
                return;

            int indexX = (int)(member.position.x);
            int indexY = (int) (member.position.y);

            int id = curMapInfo.PosToId(indexX, indexY);
            if (id < 0)
                return;
            
            GridInfo grid = curMapInfo.grids[id];
            Vector3 midPos = new Vector3(indexX + 0.5f, indexY + 0.5f, 0f);
            float FaiMX = 0f;
            float FaiLeft = 0f;
            float FaiRight = 0f;
            float FaiTop = 0f;
            float FaiButton = 0f;
            float CostLeft = 0f;
            float CostRight = 0f;
            float CostTop = 0f;
            float CostButton = 0f;

            float AdjoinCost = 0f;
            GridInfo adjoinGrid = null;
            
            // 去顶相邻格子的总消耗，当相邻的格子为不可走的时候，他的消耗为自身所在方格的消耗值，
            // 等于跟当前格子等势，于是角色在过了中心之后，就无论如何不会再往为阻挡物的格子方向走.
            adjoinGrid = grid.adjoinGrids[(int)Direction.left];
            if (adjoinGrid == null || adjoinGrid.isObstruction)
                CostLeft = grid.cost + 1;
            else
                CostLeft = adjoinGrid.cost;

            adjoinGrid = grid.adjoinGrids[(int)Direction.right];
            if (adjoinGrid == null || adjoinGrid.isObstruction)
                CostRight = grid.cost + 1;
            else
                CostRight = adjoinGrid.cost;

            adjoinGrid = grid.adjoinGrids[(int)Direction.top];
            if (adjoinGrid == null || adjoinGrid.isObstruction)
                CostTop = grid.cost + 1;
            else
                CostTop = adjoinGrid.cost;

            adjoinGrid = grid.adjoinGrids[(int)Direction.buttom];
            if (adjoinGrid == null || adjoinGrid.isObstruction)
                CostButton = grid.cost + 1;
            else
                CostButton = adjoinGrid.cost;
            
            // 这里的运算有大量的类似代码，以后需要提取出来.
            if (midPos.x - member.position.x > float.Epsilon) // 点靠左.
            {
                FaiLeft = (grid.cost - CostLeft)*(1 - (midPos.x - member.position.x));
                FaiRight = (CostLeft - grid.cost)*(midPos.x - member.position.x);// + grid.cost - CostRight;
            }
            else if (member.position.x - midPos.x > float.Epsilon) // 点靠右.
            {
                FaiLeft = (CostRight - grid.cost)*(member.position.x - midPos.x);// + grid.cost - CostLeft;
                FaiRight = (grid.cost - CostRight)*(1 - (member.position.x - midPos.x));
            }
            else // 靠中间.
            {
                FaiLeft = grid.cost - CostLeft;
                FaiRight = grid.cost - CostRight;
            }

            if (midPos.y - member.position.y > float.Epsilon) // 点靠上.
            {
                FaiTop = (grid.cost - CostTop) * (1 - (midPos.y - member.position.y));
                FaiButton = (CostTop - grid.cost)*(midPos.y - member.position.y);// + grid.cost - CostButton;
            }
            else if (member.position.y - midPos.y > float.Epsilon) // 点靠下.
            {
                FaiTop = (CostButton - grid.cost)*(member.position.y - midPos.y);// + grid.cost - CostTop;
                FaiButton = (grid.cost - CostButton) * (1 - (member.position.y - midPos.y));
            }
            else // 靠中间.
            {
                FaiTop = grid.cost - CostTop;
                FaiButton = grid.cost - CostButton;
            }

            // Fei代表cost的落差，C越大，势越大，角色是从大势往下势去走，所以落差越大，就越往那个方向倾斜.
            // 当Fai小于0的时候，说明当前所在位置是最低势，不能往对应方向走，跟等势是一样的了.
            if (FaiLeft < float.Epsilon)
                FaiLeft = 0f;
            if (FaiRight < float.Epsilon)
                FaiRight = 0f;
            if (FaiTop < float.Epsilon)
                FaiTop = 0f;
            if (FaiButton < float.Epsilon)
                FaiButton = 0f;
            
            Vector3 v = Vector3.zero;
            if (FaiLeft > FaiRight)
            {
                v.x = -FaiLeft* 1000f;
            }else
            {
                v.x = FaiRight * 1000f;
            }

            if (FaiTop > FaiButton)
            {
                v.y = -FaiTop * 1000f;
            }
            else
            {
                v.y = FaiButton * 1000f;
            }

            member.curVelocity = v.normalized * member.maxSpeed;
        }

        public static void SetGroupTarget(GroupInfo group, Vector2 targetPos)
        {
            group.endPoint = targetPos;
            CaculateCost(group);
        }
    }
}

