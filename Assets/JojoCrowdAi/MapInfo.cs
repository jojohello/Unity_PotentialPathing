using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

namespace JojoCrowdAi
{
    public enum Direction
    {
        left = 0,
        right,
        top,
        buttom,
    }

    public class GridInfo
    {
	    public int id;

        public int x;
        public int y;

        public bool isObstruction;

        //public float pho = 0f;
        public float potential;

        public float pho = 0f;  // 密度.
		public Vector3 flowVelocity = Vector3.zero ; // 速度势能平均数.
	    public float discomfort = 0f;
        public GridInfo[] adjoinGrids = new GridInfo[4];
        //public float[] adjoinCostDelta = new float[4];

        public GridInfo(int id, int posX, int posY)
        {
	        this.id = id;
            x = posX;
            y = posY;

            //for (int i = 0; i < 4; i++)
            //    adjoinCostDelta[i] = float.NaN;
        }
    }

    public class MapInfo
    {
        public int width;
        public int height;

        public Dictionary<int, GridInfo> grids = new Dictionary<int, GridInfo>(); //每个组的不一样，所以这个以后要移到groupInfo里面.
        public HashSet<int> obstructionList = new HashSet<int>();

        public MapInfo(int w, int h)
        {
            width = w;
            height = h;

            grids.Clear();

            for (int i = 0; i < h; i++)
                for (int j = 0; j < w; j++)
                {
                    int id = PosToId(j, i);
                    grids.Add(id, new GridInfo(id, j, i));
                }

            for(int i=0; i<h; i++)
                for (int j = 0; j < w; j++)
                {
                    int selfId = PosToId(j, i);
                    int adjionId = PosToId(j - 1, i);
                    if (adjionId >= 0)
                        grids[selfId].adjoinGrids[(int) Direction.left] = grids[adjionId];

                    adjionId = PosToId(j + 1, i);
                    if (adjionId >= 0)
                        grids[selfId].adjoinGrids[(int)Direction.right] = grids[adjionId];

                    adjionId = PosToId(j, i-1);
                    if (adjionId >= 0)
                        grids[selfId].adjoinGrids[(int)Direction.top] = grids[adjionId];

                    adjionId = PosToId(j, i + 1);
                    if (adjionId >= 0)
                        grids[selfId].adjoinGrids[(int)Direction.buttom] = grids[adjionId];
                }

        }

        public int PosToId(int x, int y)
        {
            if (x < 0 || x >= width
                || y < 0 || y >= height)
                return -1;

            return y*width + x;
        }
    }
}
