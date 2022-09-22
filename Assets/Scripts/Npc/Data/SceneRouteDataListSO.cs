using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SceneRouteDataListSO", menuName = "Map/SceneRouteDataList")]
public class SceneRouteDataListSO : ScriptableObject
{
    public List<SceneRoute> sceneRoutesList;
}