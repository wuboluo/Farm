using UnityEngine;

namespace Y.AStar
{
    public class GridNodes
    {
        public int width;
        public int height;

        private Node[,] gridNode;

        
        /// 构造函数初始化节点范维数组 
        public GridNodes(int width, int height)
        {
            this.width = width;
            this.height = height;

            gridNode = new Node[width, height];
            
            for (var x = 0; x < width; x++)
            {
                for (var y = 0; y < height; y++)
                {
                    gridNode[x, y] = new Node(new Vector2Int(x, y));
                }
            }
        }


        public Node GetGridNode(int xPos, int yPos)
        {
            if (xPos < width && yPos < height)
            {
                return gridNode[xPos, yPos];
            }
            
            Debug.Log("超出网格范围");
            return null;
        }
    }
}