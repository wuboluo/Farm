using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Y.Save;

namespace Y.Transition
{
    public class TransitionManager : Singleton<TransitionManager>, ISavable
    {
        [SceneName] public string startSceneName = string.Empty;

        private CanvasGroup fadeCanvasGroup;
        private bool isFade;

        protected override void Awake()
        {
            base.Awake();

            // 实际打包运行的时候，只能存在一个已激活的场景，所以要在最开始直接加载 UI场景，无需异步
            // 但是 LoadSceneMode.Additive 不代表是被激活的
            SceneManager.LoadScene("UI", LoadSceneMode.Additive);
        }

        private void Start()
        {
            ISavable savable = this;
            savable.RegisterSavable();

            fadeCanvasGroup = FindObjectOfType<CanvasGroup>();
        }

        private void OnEnable()
        {
            EventHandler.TransitionEvent += OnTransitionEvent;
            EventHandler.StartNewGameEvent += OnStartNewGameEvent;
            EventHandler.EndGameEvent += OnEndGameEvent;
        }

        private void OnDisable()
        {
            EventHandler.TransitionEvent -= OnTransitionEvent;
            EventHandler.StartNewGameEvent -= OnStartNewGameEvent;
            EventHandler.EndGameEvent -= OnEndGameEvent;
        }

        private void OnEndGameEvent()
        {
            StartCoroutine(UnloadScene());
        }

        public string GUID => GetComponent<DataGUID>().guid;

        private IEnumerator UnloadScene()
        {
            EventHandler.CallBeforeSceneUnloadEvent();
            yield return Fade(1);
            yield return SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene().buildIndex);
            yield return Fade(0);
        }

        public GameSaveData GenerateSaveData()
        {
            GameSaveData saveData = new GameSaveData();
            saveData.dataSceneName = SceneManager.GetActiveScene().name;

            return saveData;
        }

        public void RestoreData(GameSaveData saveData)
        {
            // 加载游戏进度场景
            StartCoroutine(LoadSaveDataScene(saveData.dataSceneName));
        }

        private void OnStartNewGameEvent(int index)
        {
            StartCoroutine(LoadSaveDataScene(startSceneName));
        }

        private IEnumerator LoadSaveDataScene(string sceneName)
        {
            yield return Fade(1);

            // 在游戏过程中 加载另外游戏进度
            // 可能在加载其他场景时，当前激活的场景不是 PersistentScene ，而是 home 或 stall等其他，所以需要先卸载掉
            if (SceneManager.GetActiveScene().name != "PersistentScene")
            {
                EventHandler.CallBeforeSceneUnloadEvent();
                yield return SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene().buildIndex);
            }

            // 之后再激活指定的场景
            yield return LoadSceneSetActive(sceneName);
            EventHandler.CallAfterSceneLoadedEvent();

            yield return Fade(0);
        }

        private void OnTransitionEvent(string sceneToGo, Vector3 positionToGo)
        {
            // 播放切换动画时，不能再次切换，避免人物移动过快疯狂切换
            if (!isFade)
                StartCoroutine(Transition(sceneToGo, positionToGo));
        }

        // 场景切换
        private IEnumerator Transition(string sceneName, Vector3 targetPosition)
        {
            EventHandler.CallBeforeSceneUnloadEvent();
            yield return Fade(1);
            yield return SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene());

            yield return LoadSceneSetActive(sceneName);
            EventHandler.CallMoveToPosition(targetPosition);
            EventHandler.CallAfterSceneLoadedEvent();
            yield return Fade(0);
        }

        // 加载场景并设置为激活
        public IEnumerator LoadSceneSetActive(string sceneName)
        {
            yield return SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);

            Scene newScene = SceneManager.GetSceneAt(SceneManager.sceneCount - 1);

            SceneManager.SetActiveScene(newScene);
        }

        // 淡入淡出场景（1:黑  0:透明）
        private IEnumerator Fade(float targetAlpha)
        {
            isFade = true;
            fadeCanvasGroup.blocksRaycasts = true;

            float speed = Mathf.Abs(fadeCanvasGroup.alpha - targetAlpha) / Settings.fadeDuration;

            // Mathf.Approximately
            // https://docs.unity.cn/cn/current/ScriptReference/Mathf.Approximately.html
            // 比较两个浮点值，如果它们相似，则返回 true。浮点不精确性使得使用相等运算符比较浮点数不精确。
            // 例如，(1.0 == 10.0 / 10.0) 不会每次都返回 true。 Approximately() 比较两个浮点数，如果它们相互之间的差值处于较小值 (Epsilon) 范围内，则返回 true。
            while (!Mathf.Approximately(fadeCanvasGroup.alpha, targetAlpha))
            {
                fadeCanvasGroup.alpha = Mathf.MoveTowards(fadeCanvasGroup.alpha, targetAlpha, speed * Time.deltaTime);
                yield return null;
            }

            fadeCanvasGroup.blocksRaycasts = false;
            isFade = false;
        }
    }
}