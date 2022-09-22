using UnityEngine;
using UnityEngine.Playables;

public class TimelineManager : Singleton<TimelineManager>
{
    public PlayableDirector startDirector;
    private PlayableDirector currentDirector;

    private bool isDone;
    private bool isPause;

    public bool IsDone
    {
        set => isDone = value;
    }

    protected override void Awake()
    {
        base.Awake();
        currentDirector = startDirector;
    }


    private void Update()
    {
        if (isPause && Input.GetKeyDown(KeyCode.Space) && isDone)
        {
            isPause = false;
            currentDirector.playableGraph.GetRootPlayable(0).SetSpeed(1d);
        }
    }

    private void OnEnable()
    {
        EventHandler.AfterSceneLoadedEvent += OnAfterSceneLoadedEvent;
    }


    private void OnDisable()
    {
        EventHandler.AfterSceneLoadedEvent -= OnAfterSceneLoadedEvent;
    }


    public void PauseTimeline(PlayableDirector director)
    {
        currentDirector = director;
        currentDirector.playableGraph.GetRootPlayable(0).SetSpeed(0d);

        isPause = true;
    }

    private void OnAfterSceneLoadedEvent()
    {
        currentDirector = FindObjectOfType<PlayableDirector>();
        if (currentDirector != null)
        {
            currentDirector.Play();
        }
    }
}