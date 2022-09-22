using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Yang.Dialogue
{
    public class DialogueUI : MonoBehaviour
    {
        public GameObject dialogueBox;
        public TextMeshProUGUI dialogueText;
        public Image faceLeft, faceRight;
        public TextMeshProUGUI nameLeft, nameRight;

        public GameObject continueBox;

        private void Awake()
        {
            continueBox.SetActive(false);
        }
        
        private void OnEnable()
        {
            EventHandler.ShowDialogueEvent += OnShowDialogueEvent;
        }

        private void OnDisable()
        {
            EventHandler.ShowDialogueEvent -= OnShowDialogueEvent;
        }

        private void OnShowDialogueEvent(DialoguePiece piece)
        {
            StartCoroutine(ShowDialogue(piece));
        }

        private IEnumerator ShowDialogue(DialoguePiece piece)
        {
            if (piece != null)
            {
                piece.isDone = false;

                dialogueBox.SetActive(true);
                continueBox.SetActive(false);

                dialogueText.text = string.Empty;

                if (piece.name != string.Empty)
                {
                    if (piece.onLeft)
                    {
                        faceRight.gameObject.SetActive(false);
                        faceLeft.gameObject.SetActive(true);
                        faceLeft.sprite = piece.faceImage;
                        nameLeft.text = piece.name;
                    }
                    else
                    {
                        faceRight.gameObject.SetActive(true);
                        faceLeft.gameObject.SetActive(false);
                        faceRight.sprite = piece.faceImage;
                        nameRight.text = piece.name;
                    }
                }
                else
                {
                    faceLeft.gameObject.SetActive(false);
                    faceRight.gameObject.SetActive(false);
                    nameLeft.gameObject.SetActive(false);
                    nameRight.gameObject.SetActive(false);
                }

                yield return dialogueText.DOText(piece.dialogueText, 1f).WaitForCompletion();

                piece.isDone = true;

                if (piece.hasToPause && piece.isDone)
                    continueBox.SetActive(true);
            }
            else
            {
                dialogueBox.SetActive(false);
            }
        }
    }
}