using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Y.AStar;
using Y.Save;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
public class NpcMovement : MonoBehaviour, ISaveable
{
    public ScheduleDetailsListSO scheduleData;

    [Header("移动属性")] public float normalSpeed = 2f;
    public bool isMoving;

    // 临时存储信息
    [SerializeField] private string currentScene;
    public bool interactable; // 是否可以互动
    public bool isFirstLoad;
    public AnimationClip blankAnimationClip;

    // 动画计时器
    private float animationBreakTime;
    private Animator animator;
    private AnimatorOverrideController animatorOverride;
    private bool canPlayStopAnimation;
    private BoxCollider2D coll;
    private Vector3Int currentGridPosition;
    private ScheduleDetails currentSchedule;
    private Season currentSeason;
    private Vector2 dir;
    private Grid grid;

    private bool isInitialised;
    private readonly float maxSpeed = 3;
    private readonly float minSpeed = 1;
    private Stack<MovementStep> movementSteps;
    private Vector3Int nextGridPosition;
    private Vector3 nextWorldPosition;
    private bool npcMove;

    private Coroutine npcMoveRoutine;

    // components
    private Rigidbody2D rb;
    private bool sceneLoaded;
    private SortedSet<ScheduleDetails> scheduleSet;
    private SpriteRenderer spriteRenderer; // 因为 npc不存在与地图场景，所以在切换场景时，关闭 npc图片即可实现隐藏人物
    private AnimationClip stopAnimationClip;
    private Vector3Int targetGridPosition;
    private string targetScene;


    public string StartScene
    {
        set => currentScene = value;
    }

    private TimeSpan GameTime => TimeManager.Instance.GameTime;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        coll = GetComponent<BoxCollider2D>();
        animator = GetComponent<Animator>();

        movementSteps = new Stack<MovementStep>();

        animatorOverride = new AnimatorOverrideController(animator.runtimeAnimatorController);
        animator.runtimeAnimatorController = animatorOverride;

        scheduleSet = new SortedSet<ScheduleDetails>();
        foreach (var schedule in scheduleData.scheduleDetails) scheduleSet.Add(schedule);
    }

    private void Start()
    {
        ISaveable saveable = this;
        saveable.RegisterSaveable();
    }

    private void Update()
    {
        if (sceneLoaded) SwitchAnimation();

        // 计时器
        animationBreakTime -= Time.deltaTime;
        canPlayStopAnimation = animationBreakTime <= 0;
    }

    private void FixedUpdate()
    {
        if (sceneLoaded) Movement();
    }

    private void OnEnable()
    {
        EventHandler.AfterSceneLoadedEvent += OnAfterSceneLoadedEvent;
        EventHandler.BeforeSceneUnloadEvent += OnBeforeSceneLoadedEvent;

        EventHandler.GameMinuteEvent += OnGameMinuteEvent;

        EventHandler.StartNewGameEvent += OnStartNewGameEvent;
        EventHandler.EndGameEvent += OnEndGameEvent;
    }

    private void OnDisable()
    {
        EventHandler.AfterSceneLoadedEvent -= OnAfterSceneLoadedEvent;
        EventHandler.BeforeSceneUnloadEvent -= OnBeforeSceneLoadedEvent;

        EventHandler.GameMinuteEvent -= OnGameMinuteEvent;

        EventHandler.StartNewGameEvent -= OnStartNewGameEvent;
        EventHandler.EndGameEvent -= OnEndGameEvent;
    }

    public string GUID => GetComponent<DataGUID>().guid;

    public GameSaveData GenerateSaveData()
    {
        var saveData = new GameSaveData();
        saveData.characterPosDict = new Dictionary<string, SerializableVector3>();

        saveData.characterPosDict.Add("targetGridPosition", new SerializableVector3(targetGridPosition));
        saveData.characterPosDict.Add("currentGridPosition", new SerializableVector3(transform.position));

        saveData.dataSceneName = currentScene;
        saveData.targetScene = targetScene;

        if (stopAnimationClip != null) saveData.animationInstanceID = stopAnimationClip.GetInstanceID();

        saveData.interactable = interactable;
        saveData.timeDict = new Dictionary<string, int>();
        saveData.timeDict.Add("currentSeason", (int) currentSeason);

        return saveData;
    }

    public void RestoreData(GameSaveData saveData)
    {
        // 已初始化，因为已被保存过
        isInitialised = true;
        isFirstLoad = false;

        currentScene = saveData.dataSceneName;
        targetScene = saveData.targetScene;

        var pos = saveData.characterPosDict["currentGridPosition"].ToVector3();
        var targetGridPos = (Vector3Int) saveData.characterPosDict["targetGridPosition"].ToVector2Int();

        transform.position = pos;
        targetGridPosition = targetGridPos;

        if (saveData.animationInstanceID != 0) stopAnimationClip = Resources.InstanceIDToObject(saveData.animationInstanceID) as AnimationClip;

        interactable = saveData.interactable;
        currentSeason = (Season) saveData.timeDict["currentSeason"];
    }

    private void OnEndGameEvent()
    {
        sceneLoaded = false;
        npcMove = false;

        if (npcMoveRoutine != null)
            StopCoroutine(npcMoveRoutine);
    }

    private void CheckVisible()
    {
        if (currentScene == SceneManager.GetActiveScene().name) SetActiveInScene();
        else SetInactiveInScene();
    }

    private void OnBeforeSceneLoadedEvent()
    {
        sceneLoaded = false;
    }

    private void OnAfterSceneLoadedEvent()
    {
        grid = FindObjectOfType<Grid>();
        CheckVisible();

        if (!isInitialised)
        {
            InitNpc();
            isInitialised = true;
        }

        sceneLoaded = true;

        if (!isFirstLoad)
        {
            currentGridPosition = grid.WorldToCell(transform.position);
            var schedule = new ScheduleDetails(0, 0, 0, 0, currentSeason, targetScene, (Vector2Int) targetGridPosition, stopAnimationClip, interactable);
            BuildPath(schedule);
            isFirstLoad = true;
        }
    }

    private void OnGameMinuteEvent(int minute, int hour, int day, Season season)
    {
        var time = hour * 100 + minute;
        currentSeason = season;

        ScheduleDetails matchSchedule = null;
        foreach (var schedule in scheduleSet)
            if (schedule.Time == time)
            {
                if (schedule.day != day && schedule.day != 0) continue;
                if (schedule.season != season) continue;

                matchSchedule = schedule;
            }
            else if (schedule.Time > time)
            {
                break;
            }

        if (matchSchedule != null) BuildPath(matchSchedule);
    }


    private void OnStartNewGameEvent(int index)
    {
        isInitialised = false;
        isFirstLoad = true;
    }


    private void InitNpc()
    {
        targetScene = currentScene;

        // 保持在当前坐标的网格中心点，为了保证人物移动于单元格的速度保持一致，按照格子的中心点走
        currentGridPosition = grid.WorldToCell(transform.position);
        transform.position = new Vector3(currentGridPosition.x + Settings.gridCellSize / 2, currentGridPosition.y + Settings.gridCellSize / 2, 0);

        targetGridPosition = currentGridPosition;
    }


    /// npc移动，由于在 fixedUpdate中调用了 Movement()，所以只要构建路径，就可以达成 Movement中有路可走的条件，所以让 npc移动就只需要构建路径
    private void Movement()
    {
        // 当 npc移动到下一个点时，npcMove=false，再继续找下一个点
        if (!npcMove)
        {
            // 有路要走
            if (movementSteps.Count > 0)
            {
                // 拿到第一步
                var step = movementSteps.Pop();

                // 检查第一步是否应该出现在这个场景里
                currentScene = step.sceneName;

                // 由于 npc和玩家可能频繁的出入房屋，所以需要频繁的检测 npc的显示状态
                CheckVisible();

                // 下一步的网格坐标
                nextGridPosition = (Vector3Int) step.gridCoordinate;

                // 拿到当前步（非下一步）的时间戳，对比当前的游戏时间，如果还没到下一步的时间戳，则可以缓慢移动过去，否则需要瞬移过去
                var stepTime = new TimeSpan(step.hour, step.minute, step.second);

                MoveToGridPosition(nextGridPosition, stepTime);
            }
            else if (!isMoving && canPlayStopAnimation)
            {
                StartCoroutine(SetStopAnimation());
            }
        }
    }

    private void MoveToGridPosition(Vector3Int gridPos, TimeSpan stepTime)
    {
        npcMoveRoutine = StartCoroutine(MoveRoutine(gridPos, stepTime));
    }

    private IEnumerator MoveRoutine(Vector3Int gridPos, TimeSpan stepTime)
    {
        npcMove = true;
        nextWorldPosition = GetWorldPosition(gridPos);

        // 还有时间来移动
        if (stepTime > GameTime)
        {
            // 用来移动的时间差，以秒为单位
            var timeToMove = (float) (stepTime.TotalSeconds - GameTime.TotalSeconds);
            // 实际移动距离
            var distance = Vector3.Distance(transform.position, nextWorldPosition);
            // 实际移动速度
            var speed = Mathf.Max(minSpeed, distance / timeToMove / Settings.secondThreshold);

            if (speed <= maxSpeed)
                // 如果和目标点距离小于像素距离，认为到达
                while (Vector3.Distance(transform.position, nextWorldPosition) > Settings.pixelSize)
                {
                    dir = (nextWorldPosition - transform.position).normalized;

                    var posOffset = new Vector2(dir.x * speed * Time.fixedDeltaTime, dir.y * speed * Time.fixedDeltaTime);
                    rb.MovePosition(rb.position + posOffset);

                    // 等待下一次 FixedUpdate的更新，再继续执行
                    // 顺序：fixedUpdate > update > yield return > lateUpdate
                    yield return new WaitForFixedUpdate();
                }
        }

        // 如果时间到了就瞬移过去
        rb.position = nextWorldPosition;
        currentGridPosition = gridPos;
        nextGridPosition = currentGridPosition;

        npcMove = false;
    }

    /// 根据 schedule构建路径
    public void BuildPath(ScheduleDetails schedule)
    {
        movementSteps.Clear();
        currentSchedule = schedule;
        targetScene = schedule.targetScene;
        targetGridPosition = (Vector3Int) schedule.targetGridPosition;
        stopAnimationClip = schedule.clipAtStop;
        interactable = schedule.interactable;
        
        // 同场景移动
        if (currentScene == schedule.targetScene)
        {
            AStar.Instance.BuildPath(schedule.targetScene, (Vector2Int) currentGridPosition, schedule.targetGridPosition, movementSteps);
        }
        // 跨场景移动
        else if (schedule.targetScene != currentScene)
        {
            // 获取起始场景和目标场景，和衔接点位信息
            var route = NpcManager.Instance.GetSceneRoute(currentScene, schedule.targetScene);

            if (route != null)
                foreach (var path in route.scenePaths)
                {
                    Vector2Int fromPos, gotoPos;

                    // 如果大于最大网格坐标，认为没有具体的起始点和目标点
                    if (path.fromGridCell.x >= Settings.maxGridSize)
                        fromPos = (Vector2Int) currentGridPosition; // 从当前位置出发
                    else
                        fromPos = path.fromGridCell; // 从指定位置（例如门口，进出门）出发

                    if (path.gotoGridCell.x >= Settings.maxGridSize)
                        gotoPos = (Vector2Int) targetGridPosition; // 到达具体的目标点
                    else
                        gotoPos = path.gotoGridCell; // 到达指定位置（例如进屋或出屋的门口）

                    // 构建路径
                    AStar.Instance.BuildPath(path.sceneName, fromPos, gotoPos, movementSteps);
                }
        }

        if (movementSteps.Count > 1)
            // 更新每一步对应的时间戳
            UpdateTimeOnPath();
    }


    private void UpdateTimeOnPath()
    {
        MovementStep previousStep = null;
        var currentGameTime = GameTime;

        foreach (var step in movementSteps)
        {
            // 第一步
            previousStep ??= step;

            step.hour = currentGameTime.Hours;
            step.minute = currentGameTime.Minutes;
            step.second = currentGameTime.Seconds;

            // 计算每一步的时间戳，计算走到下一步的时候应该处于游戏时间的几分几秒
            // 判断是否走斜方向，从而使用不同的距离
            // 走过下一格需要的时间（距离/速度=时间）
            var gridMovementStepTime = MoveInDiagonal(step, previousStep)
                ? new TimeSpan(0, 0, (int) (Settings.gridCellDiagonalSize / normalSpeed / Settings.secondThreshold))
                : new TimeSpan(0, 0, (int) (Settings.gridCellSize / normalSpeed / Settings.secondThreshold));

            // 累加获得下一步的时间戳：在当前的时间里加上走过一格的时间，就得到了循环第 n步需要的时间
            currentGameTime = currentGameTime.Add(gridMovementStepTime);

            // 循环下一步
            previousStep = step;
        }
    }

    /// 判断是否走斜方向
    private bool MoveInDiagonal(MovementStep currentStep, MovementStep previousStep)
    {
        return currentStep.gridCoordinate.x != previousStep.gridCoordinate.x && currentStep.gridCoordinate.y != previousStep.gridCoordinate.y;
    }


    /// 网格坐标返回世界坐标中心点
    private Vector3 GetWorldPosition(Vector3Int gridPos)
    {
        var worldPos = grid.CellToWorld(gridPos);
        return new Vector3(worldPos.x + Settings.gridCellSize / 2, worldPos.y + Settings.gridCellSize / 2);
    }


    private void SwitchAnimation()
    {
        isMoving = transform.position != GetWorldPosition(targetGridPosition);

        animator.SetBool("IsMoving", isMoving);
        if (isMoving)
        {
            animator.SetBool("Exit", true);
            animator.SetFloat("DirX", dir.x);
            animator.SetFloat("DirY", dir.y);
        }
        else
        {
            animator.SetBool("Exit", false);
        }
    }

    private IEnumerator SetStopAnimation()
    {
        // 强制面向镜头
        animator.SetFloat("DirX", 0);
        animator.SetFloat("DirY", -1);

        // animationBreakTime 在 update中持续减小，每当达到间隔时间，进入播放动画协程，并重置时间且重新持续减小
        animationBreakTime = Settings.animationBreakTime;

        if (stopAnimationClip != null)
        {
            animatorOverride[blankAnimationClip] = stopAnimationClip;
            animator.SetBool("EventAnimation", true);
            yield return null;
            animator.SetBool("EventAnimation", false);
        }
        else
        {
            animatorOverride[stopAnimationClip] = blankAnimationClip;
            animator.SetBool("EventAnimation", false);
        }
    }

    # region 设置 npc的显示

    private void SetActiveInScene()
    {
        spriteRenderer.enabled = true;
        coll.enabled = true;

        transform.GetChild(0).gameObject.SetActive(true);
    }

    private void SetInactiveInScene()
    {
        spriteRenderer.enabled = false;
        coll.enabled = false;

        transform.GetChild(0).gameObject.SetActive(false);
    }

    #endregion
}