using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

namespace Y.AStar
{
    public class AStarTest : MonoBehaviour
    {
        private AStar aStar;

        [Header("用于测试")] public Vector2Int startPos;
        public Vector2Int finishPos;

        public Tilemap displayMap;
        public TileBase displayTile;
        public bool displayStartAndFinish;
        public bool displayPath;

        private Stack<MovementStep> npcMovementStepStack;

        [Header("测试移动Npc")] 
        public NpcMovement npcMovement;
        public bool moveNpc;
        [SceneName] public string targetScene;
        public Vector2Int targetPos;
        public AnimationClip stopClip;
        
        
        private void Awake()
        {
            aStar = GetComponent<AStar>();
            npcMovementStepStack = new Stack<MovementStep>();
        }

        private void Update()
        {
            ShowPathOnGridMap();

            if (moveNpc)
            {
                moveNpc = false;
                ScheduleDetails schedule = new ScheduleDetails(0, 0, 0, 0, Season.春天, targetScene, targetPos, stopClip, true);
                npcMovement.BuildPath(schedule);
            }
        }

        private void ShowPathOnGridMap()
        {
            if (displayMap != null && displayTile != null)
            {
                if (displayStartAndFinish)
                {
                    displayMap.SetTile((Vector3Int) startPos, displayTile);
                    displayMap.SetTile((Vector3Int) finishPos, displayTile);
                }
                else
                {
                    displayMap.SetTile((Vector3Int) startPos, null);
                    displayMap.SetTile((Vector3Int) finishPos, null);
                }

                if (displayPath)
                {
                    string sceneName = SceneManager.GetActiveScene().name;

                    aStar.BuildPath(sceneName, startPos, finishPos, npcMovementStepStack);

                    foreach (MovementStep step in npcMovementStepStack)
                    {
                        displayMap.SetTile((Vector3Int) step.gridCoordinate, displayTile);
                    }
                }
                else
                {
                    if (npcMovementStepStack.Count > 0)
                    {
                        foreach (MovementStep step in npcMovementStepStack)
                        {
                            displayMap.SetTile((Vector3Int) step.gridCoordinate, null);
                        }

                        npcMovementStepStack.Clear();
                    }
                }
            }
        }
    }
}