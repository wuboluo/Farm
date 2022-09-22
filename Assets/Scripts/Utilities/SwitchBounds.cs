using System;
using Cinemachine;
using UnityEngine;

public class SwitchBounds : MonoBehaviour
{
    private void OnEnable()
    {
        EventHandler.AfterSceneLoadedEvent += SwitchConfinerShape;
    }

    private void OnDisable()
    {
        EventHandler.AfterSceneLoadedEvent -= SwitchConfinerShape;
    }

    private void SwitchConfinerShape()
    {
        var confingerShape = GameObject.FindGameObjectWithTag("BoundsConfiner").GetComponent<PolygonCollider2D>();
        var confiner = GetComponent<CinemachineConfiner>();
        
        confiner.m_BoundingShape2D = confingerShape;
        
        // https://docs.unity3d.com/Packages/com.unity.cinemachine@2.8/api/Cinemachine.CinemachineConfiner.html
        // Call this if the bounding shape's points change at runtime
        // 如果边界形状的点在运行时发生变化，则调用它
        confiner.InvalidatePathCache();
    }
}
