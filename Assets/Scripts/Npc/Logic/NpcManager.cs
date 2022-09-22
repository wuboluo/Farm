using System.Collections.Generic;

public class NpcManager : Singleton<NpcManager>
{
    public SceneRouteDataListSO sceneRouteData;

    public List<NpcPosition> npcPositionList;

    private Dictionary<string, SceneRoute> sceneRouteDict = new Dictionary<string, SceneRoute>();

    protected override void Awake()
    {
        base.Awake();
        InitSceneRouteDict();
    }

    private void OnEnable()
    {
        EventHandler.StartNewGameEvent += OnStartNewGameEvent;

    }

    private void OnDisable()
    {
        EventHandler.StartNewGameEvent -= OnStartNewGameEvent;
    }

    private void OnStartNewGameEvent(int index)
    {
        foreach (var character in npcPositionList)
        {
            character.npc.position = character.position;
            character.npc.GetComponent<NpcMovement>().StartScene = character.startScene;
        }
    }

    /// 初始化路径字典 
    private void InitSceneRouteDict()
    {
        if (sceneRouteData.sceneRoutesList.Count > 0)
        {
            foreach (var route in sceneRouteData.sceneRoutesList)
            {
                var key = route.fromSceneName + route.gotoSceneName;

                if (sceneRouteDict.ContainsKey(key)) continue;
                sceneRouteDict.Add(key, route);
            }
        }
    }

    /// 获得两个场景间的路径
    public SceneRoute GetSceneRoute(string fromSceneName, string gotoSceneName)
    {
        return sceneRouteDict[fromSceneName + gotoSceneName];
    }
}