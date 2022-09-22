using System.Collections.Generic;
using UnityEngine;

namespace Y.AStar
{
    public class AStar : Singleton<AStar>
    {
        private GridNodes gridNodes;

        private Node startNode, targetNode;

        private int gridWidth, gridHeight;
        private int originX, originY;

        // 当前选中的 node周围的 8个点
        private List<Node> openNodeList;

        // 所有被选中的点
        private HashSet<Node> closeNodeList;

        private bool pathFound;

        /// 构建路径，更新 stack中的每一步
        public void BuildPath(string sceneName, Vector2Int startPos, Vector2Int endPos, Stack<MovementStep> npcMovementStep)
        {
            pathFound = false;

            if (GenerateGridNodes(sceneName, startPos, endPos))
            {
                // 查找最短路径
                if (FindShortestPath())
                {
                    // 构建 Npc移动路径
                    UpdatePathOnMovementStepStack(sceneName, npcMovementStep);
                }
            }
        }

        /// 构建网格节点信息，初始化两个列表
        private bool GenerateGridNodes(string sceneName, Vector2Int startPos, Vector2Int endPos)
        {
            if (GridMapManager.Instance.GetGridDimensions(sceneName, out var gridDimensions, out var gridOrigin))
            {
                // 根据瓦片地图范围构建网络移动节点范维数组
                gridNodes = new GridNodes(gridDimensions.x, gridDimensions.y);
                gridWidth = gridDimensions.x;
                gridHeight = gridDimensions.y;
                originX = gridOrigin.x;
                originY = gridOrigin.y;

                openNodeList = new List<Node>();
                closeNodeList = new HashSet<Node>();
            }
            else
                return false;

            // gridNodes的范围是从 0,0开始所以需要减去原点坐标得到的实际位置
            startNode = gridNodes.GetGridNode(startPos.x - originX, startPos.y - originY);
            targetNode = gridNodes.GetGridNode(endPos.x - originX, endPos.y - originY);


            for (var x = 0; x < gridWidth; x++)
            {
                for (var y = 0; y < gridHeight; y++)
                {
                    var tilePos = new Vector3Int(x + originX, y + originY, 0);
                    var key = tilePos.x + "x" + tilePos.y + "y" + sceneName;

                    var tile = GridMapManager.Instance.GetTileDetails(key);

                    if (tile != null)
                    {
                        var node = gridNodes.GetGridNode(x, y);
                        if (tile.isNpcObstacle)
                            node.isObstacle = true;
                    }
                }
            }

            return true;
        }

        /// 找到最短路径所有的 node添加到 closedNodeList 
        private bool FindShortestPath()
        {
            // 添加起点
            openNodeList.Add(startNode);

            // 不断循环寻找下一个路径点
            while (openNodeList.Count > 0)
            {
                // 节点排序，node内含比较函数，所以第一项永远是最近的（根据 FCost和 HCost）
                openNodeList.Sort();

                // 找到第一项（最近的）
                var closeNode = openNodeList[0];

                // 从这一轮周围 8个点中移除此点，并将这个点添加至所有被选中的点的集合
                openNodeList.RemoveAt(0);
                closeNodeList.Add(closeNode);

                // 当这个点等于目标点了，则到达终点，跳出循环，结束寻路
                if (closeNode == targetNode)
                {
                    pathFound = true;
                    break;
                }

                // 计算周围 8个 node补充到 openList
                EvaluateNeighbourNodes(closeNode);
            }

            return pathFound;
        }

        /// 评估周围 8个点，并生成对应消耗值
        /// currentNode：每当选择一个新的点时，去比较以他为中心的上下左右 8个点
        private void EvaluateNeighbourNodes(Node currentNode)
        {
            var currentNodePos = currentNode.gridPosition;
            Node validNeighbourNode;

            // 从 -1到 1是因为周围 8个点可以通过当前坐标相邻的特性去遍历
            for (var x = -1; x <= 1; x++)
            {
                for (var y = -1; y <= 1; y++)
                {
                    // 都等于 0则为原点本身，不做考虑
                    if (x == 0 && y == 0)
                        continue;

                    // 根据相对坐标找到周围 8个点中的一个点
                    validNeighbourNode = GetValidNeighbourNode(currentNodePos.x + x, currentNodePos.y + y);

                    // 如果这个点是有效点
                    if (validNeighbourNode != null)
                    {
                        if (!openNodeList.Contains(validNeighbourNode))
                        {
                            validNeighbourNode.gCost = currentNode.gCost + GetDistance(currentNode, validNeighbourNode);
                            validNeighbourNode.hCost = GetDistance(validNeighbourNode, targetNode);

                            // 链接父节点
                            validNeighbourNode.parentNode = currentNode;
                            openNodeList.Add(validNeighbourNode);
                        }
                    }
                }
            }
        }

        /// 找到有效的 Node（非障碍，非已选择）
        private Node GetValidNeighbourNode(int x, int y)
        {
            // 出界的话，认为无效点
            if (x >= gridWidth || y >= gridHeight || x < 0 || y < 0) return null;

            var neighbourNode = gridNodes.GetGridNode(x, y);

            // 不为障碍，且不为此点的父节点（已被添加至选中的点的集合中，通俗点说就是来自这个点）
            if (neighbourNode.isObstacle || closeNodeList.Contains(neighbourNode))
                return null;

            return neighbourNode;
        }

        /// 返回两点距离值（14的倍数 + 10的倍数）
        private int GetDistance(Node nodeA, Node nodeB)
        {
            var xDistance = Mathf.Abs(nodeA.gridPosition.x - nodeB.gridPosition.x);
            var yDistance = Mathf.Abs(nodeA.gridPosition.y - nodeB.gridPosition.y);

            if (xDistance > yDistance)
            {
                return 14 * yDistance + 10 * (xDistance - yDistance);
            }

            return 14 * xDistance + 10 * (yDistance - xDistance);
        }


        /// 更新路径每一步的坐标和场景名字 
        private void UpdatePathOnMovementStepStack(string sceneName, Stack<MovementStep> npcMovementStep)
        {
            var nextNode = targetNode;

            while (nextNode != null)
            {
                var newStep = new MovementStep
                {
                    sceneName = sceneName,
                    gridCoordinate = new Vector2Int(nextNode.gridPosition.x + originX, nextNode.gridPosition.y + originY)
                };

                npcMovementStep.Push(newStep);
                nextNode = nextNode.parentNode;
            }
        }
    }
}