using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public GameObject menuPrefab;
    private GameObject menuCanvas;

    public Button settingsBtn;
    public GameObject pausePanel;
    public Slider volumeSlider;

    private void Awake()
    {
        settingsBtn.onClick.AddListener(TogglePausePanel);
        volumeSlider.onValueChanged.AddListener(AudioManager.Instance.SetMasterVolume);
    }

    private void Start()
    {
        menuCanvas = GameObject.FindWithTag("MenuCanvas");
        Instantiate(menuPrefab, menuCanvas.transform);
    }

    private void OnEnable()
    {
        EventHandler.AfterSceneLoadedEvent += OnAfterSceneLoadedEvent;
    }

    private void OnDisable()
    {
        EventHandler.AfterSceneLoadedEvent -= OnAfterSceneLoadedEvent;
    }

    private void OnAfterSceneLoadedEvent()
    {
        if (menuCanvas.transform.childCount > 0)
        {
            Destroy(menuCanvas.transform.GetChild(0).gameObject);
        }
    }

    private void TogglePausePanel(  )
    {
        bool isOpen = pausePanel.activeInHierarchy;
        if (isOpen)
        {
            pausePanel.SetActive(false);
            Time.timeScale = 1;
        }
        else
        {
            // 在游戏暂停时回收一下垃圾
            GC.Collect();
            
            pausePanel.SetActive(true);
            // 当 timeScale=0时，游戏内的循环和协程都不会执行
            Time.timeScale = 0;
        }
    }

    public void ReturnMenuCanvas()
    {
        Time.timeScale = 1;
        StartCoroutine(BackToMenu());
    }

    private IEnumerator BackToMenu()
    {
        pausePanel.SetActive(false);
        EventHandler.CallEndGameEvent();        
        
        yield return new WaitForSeconds(1);
        Instantiate(menuPrefab, menuCanvas.transform);
    }
}