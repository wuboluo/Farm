using System;
using UnityEngine;
using UnityEngine.Events;

namespace Yang.Dialogue
{
    [Serializable]
    public class DialoguePiece
    {
        [Header("对话详情")]
        public Sprite faceImage;
        public bool onLeft;
        public string name;

        [TextArea] public string dialogueText;

        public bool hasToPause; // 是否需要暂停 （空格继续）
        [HideInInspector] 
        public bool isDone; // 对话是否完成

        // public UnityEvent afterTalkEvent;
    }
}