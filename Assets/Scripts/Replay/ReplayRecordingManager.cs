using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ReplayRecordingManager : MonoBehaviour
{
    public static ReplayRecordingManager Instance;
    public ReplayData loadedReplay;
    public List<InputEvent> inputList;
    public bool isReplay;

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!isReplay)
        {
            loadedReplay = null;
            inputList.Clear();
        }
    }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (inputList == null)
            inputList = new List<InputEvent>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.J))
        {
            SaveData();
        }
    }

    public void SaveData()
    {
        string scene = SceneManager.GetActiveScene().name;
        float time = GameManager.Instance.timer.elapsedTime;
        List<EnemyReplayData> enemyData = ReplayEnemyTracker.Instance.enemyDataList;

        var replayData = new ReplayData(scene, enemyData, inputList);

        ReplayFileManager.SaveToFile(replayData, FormatTime(time));
    }

    public static string FormatTime(float time)
    {
        int minutes = Mathf.FloorToInt(time / 60f);
        int seconds = Mathf.FloorToInt(time % 60f);
        int centiseconds = Mathf.FloorToInt((time * 100) % 100);

        return string.Format("{0:00}:{1:00}.{2:00}", minutes, seconds, centiseconds);
    }

    public void SetReplayData(ReplayData data)
    {
        loadedReplay = data;
    }
}

[System.Serializable]
public class ReplayData
{
    public string scene;
    public string version;
    public List<EnemyReplayData> enemyData;
    public List<InputEvent> inputEvents;

    public ReplayData(string scene, List<EnemyReplayData> enemyData, List<InputEvent> inputEvents)
    {
        this.scene = scene;
        version = Application.version;
        this.enemyData = enemyData;
        this.inputEvents = inputEvents;
    }
}