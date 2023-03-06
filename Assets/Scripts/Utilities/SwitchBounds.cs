using Cinemachine;
using UnityEngine;

public class SwitchBounds : MonoBehaviour
{
    private void OnEnable()
    {
        EventHandler.AfterSceneLoadedEvent += SwitchConfineShape;
    }

    private void OnDisable()
    {
        EventHandler.AfterSceneLoadedEvent -= SwitchConfineShape;
    }

    private void SwitchConfineShape()
    {
        PolygonCollider2D confineShape = GameObject.FindGameObjectWithTag("BoundsConfiner").GetComponent<PolygonCollider2D>();
        CinemachineConfiner confine = GetComponent<CinemachineConfiner>();
        
        confine.m_BoundingShape2D = confineShape;
        
        // https://docs.unity3d.com/Packages/com.unity.cinemachine@2.8/api/Cinemachine.CinemachineConfiner.html
        // Call this if the bounding shape's points change at runtime
        // 如果边界形状的点在运行时发生变化，则调用它
        confine.InvalidatePathCache();
    }
}
