using UnityEngine;

namespace Y.Transition
{
    public class Teleport : MonoBehaviour
    {
        // 要去到的场景
        [SceneName] public string sceneToGo;

        // 要去到的坐标
        public Vector3 positionToGo;

        private void OnTriggerEnter2D(Collider2D other)
        {
            // 避免 Npc也被传送走，Npc不会通过这种方式传送
            if (other.CompareTag("Player"))
            {
                EventHandler.CallTransitionEvent(sceneToGo, positionToGo);
            }
        }
    }
}