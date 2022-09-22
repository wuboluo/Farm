using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Yang.Dialogue
{
    [RequireComponent(typeof(NpcMovement))]
    [RequireComponent(typeof(BoxCollider2D))]
    public class DialogueController : MonoBehaviour
    {
        public UnityEvent onFinishEvent;

        public List<DialoguePiece> dialogueList;

        private bool canTalk;
        private bool isTalking;

        private Stack<DialoguePiece> dialogueStack;

        private GameObject uiSign;
        private NpcMovement npc => GetComponent<NpcMovement>();

        private void Awake()
        {
            uiSign = transform.GetChild(1).gameObject;

            FillDialogueStack();
        }


        private void Update()
        {
            uiSign.SetActive(canTalk);

            if (canTalk & Input.GetKeyDown(KeyCode.Space) && !isTalking)
            {
                StartCoroutine(DialogueRoutine());
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                // 在没有移动并且可以互动的时候，允许发生对话
                canTalk = !npc.isMoving && npc.interactable;
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.CompareTag("Player")) canTalk = false;
        }


        /// 构建对话堆栈：将该 npc配置的对话内容反向压入栈，从而由第一条逐条弹出，避免了使用 list考虑序号的问题，更方便
        private void FillDialogueStack()
        {
            dialogueStack = new Stack<DialoguePiece>();

            for (var i = dialogueList.Count - 1; i >= 0; i--)
            {
                dialogueList[i].isDone = false;
                dialogueStack.Push(dialogueList[i]);
            }
        }

        private IEnumerator DialogueRoutine()
        {
            isTalking = true;

            if (dialogueStack.TryPop(out var result))
            {
                //传到 UI显示对话
                EventHandler.CallShowDialogueEvent(result);
                EventHandler.CallUpdateGameStateEvent(GameState.Pause);
                yield return new WaitUntil(() => result.isDone);
                isTalking = false;
            }
            else
            {
                EventHandler.CallUpdateGameStateEvent(GameState.GamePlay);
                EventHandler.CallShowDialogueEvent(null);
                FillDialogueStack();
                isTalking = false;

                if (onFinishEvent != null)
                {
                    onFinishEvent.Invoke();
                    canTalk = false;
                }
            }
        }
    }
}