using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Y.Save;

public class Player : MonoBehaviour, ISavable
{
    public float speed;
    private Animator[] animators;

    private bool inputDisable;
    private float inputX, inputY;

    private bool isMoving;

    // 动画实用工具
    private float mouseX, mouseY;

    private Vector2 movementInput;
    private Rigidbody2D rb;

    private bool useTool;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animators = GetComponentsInChildren<Animator>();
        inputDisable = true;
    }

    private void Start()
    {
        ISavable savable = this;
        savable.RegisterSavable();
    }

    private void Update()
    {
        if (!inputDisable) PlayerInput();
        else isMoving = false;

        SwitchAnimation();
    }

    private void FixedUpdate()
    {
        if (!inputDisable) Movement();
    }

    private void OnEnable()
    {
        EventHandler.BeforeSceneUnloadEvent += OnBeforeSceneUnload;
        EventHandler.AfterSceneLoadedEvent += OnAfterSceneLoaded;
        EventHandler.MoveToPosition += OnMoveToPosition;
        EventHandler.MouseClickedEvent += OnMouseClicked;
        EventHandler.UpdateGameStateEvent += OnUpdateGameState;
        EventHandler.StartNewGameEvent += OnStartNewGameEvent;
        EventHandler.EndGameEvent += OnEndGameEvent;
    }

    private void OnDisable()
    {
        EventHandler.BeforeSceneUnloadEvent -= OnBeforeSceneUnload;
        EventHandler.AfterSceneLoadedEvent -= OnAfterSceneLoaded;
        EventHandler.MoveToPosition -= OnMoveToPosition;
        EventHandler.MouseClickedEvent -= OnMouseClicked;
        EventHandler.UpdateGameStateEvent -= OnUpdateGameState;
        EventHandler.StartNewGameEvent -= OnStartNewGameEvent;
        EventHandler.EndGameEvent -= OnEndGameEvent;
    }

    public string GUID => GetComponent<DataGUID>().guid;

    public GameSaveData GenerateSaveData()
    {
        GameSaveData saveData = new GameSaveData();
        saveData.characterPosDict = new Dictionary<string, SerializableVector3>();
        saveData.characterPosDict.Add(name, new SerializableVector3(transform.position));

        return saveData;
    }

    public void RestoreData(GameSaveData saveData)
    {
        Vector3 targetPosition = saveData.characterPosDict[name].ToVector3();
        transform.position = targetPosition;
    }

    private void OnEndGameEvent()
    {
        inputDisable = true;
    }

    private void OnUpdateGameState(GameState state)
    {
        inputDisable = state switch
        {
            GameState.GamePlay => false,
            GameState.Pause => true,
            _ => inputDisable
        };
    }

    private void OnStartNewGameEvent(int index)
    {
        inputDisable = false;
        transform.position = Settings.playerStartPos;
    }

    private void OnMouseClicked(Vector3 mouseWorldPos, ItemDetails details)
    {
        // 物品不是种子，商品或家具
        if (details.itemType != ItemType.Seed && details.itemType != ItemType.Commodity && details.itemType != ItemType.Furniture)
        {
            mouseX = mouseWorldPos.x - transform.position.x;
            mouseY = mouseWorldPos.y - (transform.position.y + 0.85f);

            // 根据 x或y 相对人物本身偏移量的大小，考虑左右或上下的优先级
            // 例如：人物坐标（0，0）鼠标坐标（2，1），此时 x偏移2，y偏移1。则选择为右方向
            if (Mathf.Abs(mouseX) > Mathf.Abs(mouseY)) mouseY = 0;
            else mouseX = 0;

            StartCoroutine(UseToolRoutine(mouseWorldPos, details));
        }

        // 如果物品是种子，商品或家具，直接执行，无需延迟。例如直接在地面生成
        else
        {
            EventHandler.CallExecuteActionAfterAnimation(mouseWorldPos, details);
        }
    }

    private IEnumerator UseToolRoutine(Vector3 mouseWorldPos, ItemDetails details)
    {
        // 开始实用工具，并且不允许移动
        useTool = true;
        inputDisable = true;
        yield return null;

        // 将身体各个部位的动画切换为实用工具相关
        foreach (Animator anim in animators)
        {
            anim.SetTrigger("useTool");

            // 人物的朝向
            anim.SetFloat("InputX", mouseX);
            anim.SetFloat("InputY", mouseY);
        }

        // 0.45-大概为工具使用在地面或到位的地方的时的大概时间，取决于动画的速度
        yield return new WaitForSeconds(0.45f);

        // 执行实际的操作
        EventHandler.CallExecuteActionAfterAnimation(mouseWorldPos, details);

        // 大概实际操作执行完毕后
        yield return new WaitForSeconds(0.25f);

        // 设置为没有在实用工具，并且可以移动
        useTool = false;
        inputDisable = false;
    }

    private void OnMoveToPosition(Vector3 pos)
    {
        transform.position = pos;
    }

    private void OnAfterSceneLoaded()
    {
        inputDisable = false;
    }

    private void OnBeforeSceneUnload()
    {
        inputDisable = true;
    }

    private void PlayerInput()
    {
        // 1.若需要人物禁止斜方向移动，则添加下面两个条件判断即可
        // if (_inputY == 0)
        inputX = Input.GetAxisRaw("Horizontal");
        // if (_inputX == 0)
        inputY = Input.GetAxisRaw("Vertical");

        // 2.防止斜方向移动速度过快
        if (inputX != 0 && inputY != 0)
        {
            inputX *= 0.6f;
            inputY *= 0.6f;
        }

        // 走路状态速度
        if (Input.GetKey(KeyCode.LeftShift))
        {
            inputX *= 0.5f;
            inputY *= 0.5f;
        }

        movementInput = new Vector2(inputX, inputY);

        isMoving = movementInput != Vector2.zero;
    }

    private void Movement()
    {
        // 俯视角的情况下，一般采用移动坐标的位置
        // *Time.deltaTim 为了适应不同设备下不同帧数的统一
        rb.MovePosition(rb.position + movementInput * speed * Time.deltaTime);
    }

    private void SwitchAnimation()
    {
        foreach (Animator anim in animators)
        {
            anim.SetBool("isMoving", isMoving);

            // 切换使用工具时，工具的朝向
            anim.SetFloat("mouseX", mouseX);
            anim.SetFloat("mouseY", mouseY);

            if (isMoving)
            {
                anim.SetFloat("InputX", inputX);
                anim.SetFloat("InputY", inputY);
            }
        }
    }
}