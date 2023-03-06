using System;
using UnityEngine;

namespace Y.AStar
{
    public class Node : IComparable<Node>
    {
        // 网格坐标
        public Vector2Int gridPosition;

        // 距离 start格子的距离
        public int gCost;

        // 距离 target格子的距离
        public int hCost;

        // 当前格子的值
        public int FCost => gCost + hCost;

        // 当前格子是否为障碍
        public bool isObstacle;

        public Node parentNode;

        public Node(Vector2Int pos)
        {
            gridPosition = pos;
            parentNode = null;
        }

        public int CompareTo(Node other)
        {
            // 比较选出最低的 F值，返回 -1 0 1
            int result = FCost.CompareTo(other.FCost);

            if (result == 0)
            {
                result = hCost.CompareTo(other.hCost);
            }

            return result;
        }
    }
}