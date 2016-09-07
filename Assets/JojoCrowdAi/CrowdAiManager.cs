using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Security.Cryptography;
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

            for(int i=0;i< 2;i++)
                for (int j = 0; j < 3; j++)
                {
                    Member member = new Member();
                    member.position.x = i + 0.5f;
                    member.position.y = j + 0.5f;

                    groups[0].members.Add(member);
                }

			groups.Add(new GroupInfo());
			groups[1].color = Color.blue;
			groups[1].endPoint = new Vector3(0, 0, 0);
			for(int i = 0; i < 2; i++)
				for(int j = 0; j < 3; j++) {
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
#region about density   
		private static float exponent = 1f;
		private static float deltaX = 0f;
		private static float deltaY = 0f;
	    private static Vector3 tempPos = Vector3.zero;
		public static void CaculateDensity()
        {
            if (null == curMapInfo)
                return;

	        foreach (int key in curMapInfo.grids.Keys)
	        {
				curMapInfo.grids[key].pho = 0f;
				curMapInfo.grids[key].flowVelocity = Vector3.zero;
		        curMapInfo.grids[key].discomfort = 0f;
	        }
                

            int groupCount = groups.Count;
			Member tempMember;

			for (int i = 0; i < groupCount; i++)
			{
				int memberCount = groups[i].members.Count;
				for (int j = 0; j < memberCount; j++)
				{
					tempMember = groups[i].members[j];
					AddPersonEffectOnDensity(tempMember, tempMember.position, false);

					// add discomfort area
					tempPos = tempMember.position + tempMember.curVelocity*0.25f;
					AddPersonEffectOnDensity(tempMember, tempPos, true);
				}
			}
        }

		private static int originalX = 0;
		private static int originalY = 0;
		private static int indexX = 0;
		private static int indexY = 0;
		private static float phoSelf = 0f;
		private static float phoRight = 0f;
		private static float phoButtom = 0f;
		static private void AddPersonEffectOnDensity(Member member, Vector3 pos, bool isDiscomfort = false)
		{
			phoSelf = 0f;
			phoRight = 0f;
			phoButtom = 0f;

			originalX = indexX = (int)pos.x;
			originalY = indexY = (int)pos.y;

			// 找右下角.
			if(indexX + 0.5f > pos.x)
				indexX -= 1;
			if(indexY + 0.5f > pos.y)
				indexY -= 1;

			deltaX = pos.x - (indexX + 0.5f);
			deltaY = pos.y - (indexY + 0.5f);

			CaculatePhoForGrid(member, indexX, indexY, 0, isDiscomfort);
			CaculatePhoForGrid(member, indexX + 1, indexY, 1, isDiscomfort);
			CaculatePhoForGrid(member, indexX, indexY + 1, 2, isDiscomfort);
			CaculatePhoForGrid(member, indexX + 1, indexY + 1, 3, isDiscomfort);

			if (isDiscomfort == false)
			{
				if (originalX > indexX)
					member.phoHorizontal = phoSelf + (phoRight - phoSelf)*(originalX + 0.5f - pos.x);
				else
					member.phoHorizontal = phoSelf + (phoRight - phoSelf)*(pos.x - originalX - 0.5f);

				if (originalY > indexY)
					member.phoVertical = phoSelf + (phoButtom - phoSelf)*(originalY + 0.5f - pos.y);
				else
					member.phoVertical = phoSelf + (phoButtom - phoSelf)*(pos.y - indexY - 0.5f);
			}
		}

		private static int gridId = -1;
		private static GridInfo tempGrid = null;
		private static float tempPho = 0f;
		private static void CaculatePhoForGrid(Member member, int x, int y, int dataIndex, bool isDiscomfort)
		{
			gridId = curMapInfo.PosToId(x, y);
			if(gridId >= 0) {
				tempGrid = curMapInfo.grids[gridId];
				switch (dataIndex)
				{
					case 0:
						tempPho = Mathf.Pow(Mathf.Min(1 - deltaX, 1 - deltaY), member.radius);
						break;
					case 1:
						tempPho = Mathf.Pow(Mathf.Min(deltaX, 1 - deltaY), member.radius);
						break;
					case 2:
						tempPho = Mathf.Pow(Mathf.Min(1 - deltaX, deltaY), member.radius);
						break;
					case 3:
						tempPho = Mathf.Pow(Mathf.Min(deltaX, deltaY), member.radius);
						break;
				}
				
				if(isDiscomfort) {
					member.discomfortGridIDs[dataIndex] = gridId;
					member.discomfortPotentials[dataIndex] = tempPho;

					tempGrid.discomfort += tempPho;
				} else {
					if(x == originalX && y == originalY) {
						phoSelf = tempPho;
					} else if(x != originalX && y == originalY) {
						phoRight = tempPho;
					} else if(x == originalX && y != originalY) {
						phoButtom = tempPho;
					}

					tempGrid.pho += tempPho;
					tempGrid.flowVelocity += tempPho * member.curVelocity;
				}
				
			} else if(isDiscomfort) {
				member.discomfortGridIDs[dataIndex] = -1;
			}
		}
#endregion

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
				group.costDict[key] = 0;
            
            foreach (int key in endDict.Keys)
            {
				//curMapInfo.grids[key].cost = endDict[key]; // 这里可能还包括了
				//curMapInfo.grids[key].pathLength = endDict[key];
				group.costDict[key] = endDict[key];
			}
        }

		private static GridInfo adjoinGrid = null;
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
            float faiLeft = 0f;
            float faiRight = 0f;
            float faiTop = 0f;
            float faiButtom = 0f;
            float costLeft = 0f;
            float costRight = 0f;
            float costTop = 0f;
            float costButtom = 0f;
	        float costSelf = GetGridCost(id, group, member);
	        float fsRight = 0f;	// fs means flow speed.
	        float fsLeft = 0f;
	        float fsTop = 0f;
	        float fsButtom = 0f;
			float desRight = 0f; // ds means density
			float desLeft = 0f;
			float desTop = 0f;  
			float desButtom = 0f;
			float fsFaiHorizontal = 0f;
	        float fsFaiVertical = 0f;
			float desFaiHorizontal = 0f;
			float desFaiVertical = 0f;

			// 去顶相邻格子的总消耗，当相邻的格子为不可走的时候，他的消耗为自身所在方格的消耗值，
			// 等于跟当前格子等势，于是角色在过了中心之后，就无论如何不会再往为阻挡物的格子方向走.
			GetCostAndFSAdnDesOfDirection(Direction.left,
				grid,
				group,
				member,
				costSelf,
				out costLeft, out fsLeft, out desLeft);

			GetCostAndFSAdnDesOfDirection(Direction.right,
			   grid,
			   group,
			   member,
			   costSelf,
			   out costRight, out fsRight, out desRight);

			GetCostAndFSAdnDesOfDirection(Direction.top,
				grid,
				group,
				member,
				costSelf,
				out costTop, out fsTop, out desTop);

			GetCostAndFSAdnDesOfDirection(Direction.buttom,
				grid,
				group,
				member,
				costSelf,
				out costButtom, out fsButtom, out desTop);

			// There is a large number of similar code here.
			// It should be abstract out later.
			if(midPos.x - member.position.x > float.Epsilon) // clost to left.
            {
                faiLeft = (costSelf - costLeft)*(1 - (midPos.x - member.position.x));
                faiRight = (costLeft - costSelf) *(midPos.x - member.position.x);// + grid.cost - CostRight;
				
				fsFaiHorizontal = grid.flowVelocity.x + (fsLeft - grid.flowVelocity.x) * (midPos.x - member.position.x);// + grid.cost - CostRight;
				desFaiHorizontal = grid.pho + (desLeft - grid.pho) * (midPos.x - member.position.x);
			}
            else if (member.position.x - midPos.x > float.Epsilon) // close to right.
            {
                faiLeft = (costRight - costSelf) *(member.position.x - midPos.x);// + grid.cost - CostLeft;
                faiRight = (costSelf - costRight)*(1 - (member.position.x - midPos.x));

				fsFaiHorizontal = grid.flowVelocity.x + (fsRight - grid.flowVelocity.x) * (member.position.x - midPos.x);
				desFaiHorizontal = grid.pho + (desRight - grid.pho) * (member.position.x - midPos.x);
			}
            else // clost to mid.
            {
                faiLeft = costSelf - costLeft;
                faiRight = costSelf - costRight;
				
				fsFaiHorizontal = grid.flowVelocity.x;
	            desFaiHorizontal = grid.pho;
            }

			desFaiHorizontal -= member.phoHorizontal;

            if (midPos.y - member.position.y > float.Epsilon) // close to top.
            {
                faiTop = (costSelf - costTop) * (1 - (midPos.y - member.position.y));
                faiButtom = (costTop - costSelf) *(midPos.y - member.position.y);// + grid.cost - CostButton;

				fsFaiVertical = grid.flowVelocity.y + (fsTop - grid.flowVelocity.y) * (midPos.y - member.position.y);
				desFaiVertical = grid.pho + (desTop - grid.pho) * (midPos.y - member.position.y);
			}
            else if (member.position.y - midPos.y > float.Epsilon) // close to buttom.
            {
                faiTop = (costButtom - costSelf) *(member.position.y - midPos.y);// + grid.cost - CostTop;
                faiButtom = (costSelf - costButtom) * (1 - (member.position.y - midPos.y));

				fsFaiVertical = grid.flowVelocity.y + (fsButtom - grid.flowVelocity.y) * (member.position.y - midPos.y);
				desFaiVertical = grid.pho + (desButtom - grid.pho) * (member.position.y - midPos.y);

			} else // clost to mid.
            {
                faiTop = costSelf - costTop;
                faiButtom = costSelf - costButtom;

	            fsFaiVertical = grid.flowVelocity.y;
	            desFaiVertical = grid.pho;
            }

			desFaiVertical -= member.phoVertical;

			// Fei代表cost的落差，C越大，势越大，角色是从大势往下势去走，所以落差越大，就越往那个方向倾斜.
			// 当Fai小于0的时候，说明当前所在位置是最低势，不能往对应方向走，跟等势是一样的了.
			if (faiLeft < float.Epsilon)
                faiLeft = 0f;
            if (faiRight < float.Epsilon)
                faiRight = 0f;
            if (faiTop < float.Epsilon)
                faiTop = 0f;
            if (faiButtom < float.Epsilon)
                faiButtom = 0f;
            
            Vector3 v = Vector3.zero;
            if (faiLeft > faiRight)
            {
                v.x = -faiLeft* 1000f;
            }else
            {
                v.x = faiRight * 1000f;
            }

            if (faiTop > faiButtom)
            {
                v.y = -faiTop * 1000f;
            }
            else
            {
                v.y = faiButtom * 1000f;
            }

            member.curVelocity = v.normalized * member.maxSpeed;

			// caculate how the destiny effect on speed.
			// left or right
			if(member.curVelocity.x*fsFaiHorizontal < 0)
				fsFaiHorizontal = 0;
			
			if(Mathf.Abs(member.curVelocity.x) > float.Epsilon
				&& desFaiHorizontal > minPho
				&& Mathf.Abs(fsFaiHorizontal) < Mathf.Abs(member.curVelocity.x)) 
			{
					if(desFaiHorizontal >= MaxPho)
						member.curVelocity.x = fsFaiHorizontal;
					else
						member.curVelocity.x = fsFaiHorizontal + (member.curVelocity.x - fsFaiHorizontal)
							* (MaxPho - desFaiHorizontal ) / (MaxPho - minPho);
			}

			if(member.curVelocity.y * fsFaiVertical < 0)
				fsFaiVertical = 0;

			if(Mathf.Abs(member.curVelocity.y) > float.Epsilon
				&& desFaiVertical > minPho
				&& Mathf.Abs(fsFaiVertical) < Mathf.Abs(member.curVelocity.y)) {
				if(desFaiVertical >= MaxPho)
					member.curVelocity.y = fsFaiVertical;
				else
					member.curVelocity.y = fsFaiVertical + (member.curVelocity.y - fsFaiVertical)
						* (MaxPho - desFaiVertical) / (MaxPho - minPho);
			}

			// how to avoid the uncomfortable area.
		}

        public static void SetGroupTarget(GroupInfo group, Vector2 targetPos)
        {
            group.endPoint = targetPos;
            CaculateCost(group);
        }

	    private static void GetCostAndFSAdnDesOfDirection(
			Direction dir,
			GridInfo grid,
			GroupInfo group,
			Member member,
			float costSelf, 
			out float cost, out float fs, out float des)
	    {
			adjoinGrid = grid.adjoinGrids[(int)dir];
			if(adjoinGrid == null || adjoinGrid.isObstruction) {
				cost = costSelf + 1;
				fs = 0f;
				des = 0f;
			} else {
				cost = GetGridCost(adjoinGrid.id, group, member);
				if (dir == Direction.left
					|| dir == Direction.right)
					fs = adjoinGrid.flowVelocity.x;
				else
					fs = adjoinGrid.flowVelocity.y;
				des = adjoinGrid.pho;
			}
		}

	    static private float GetGridCost(int gridID, GroupInfo group, Member member)
	    {
		    float ret = 0f;
		    if (group.costDict.ContainsKey(gridID))
			    ret += group.costDict[gridID];

		    if (curMapInfo.grids.ContainsKey(gridID))
			    ret += curMapInfo.grids[gridID].discomfort;

			for(int i=0; i<4; i++)
		    {
			    if (member.discomfortGridIDs[i] != gridID)
				    continue;

			    ret -= member.discomfortPotentials[i];
			    break;
		    }

		    return ret;
	    }
    }
}

