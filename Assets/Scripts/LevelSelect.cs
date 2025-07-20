using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelSelect : MonoBehaviour
{
    JsonSerializerSettings settings = new JsonSerializerSettings
    {
        TypeNameHandling = TypeNameHandling.Auto,
        TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple,
        Formatting = Formatting.None,
        ReferenceLoopHandling = ReferenceLoopHandling.Ignore
    };

    [Header("Replay UI")]
    public GameObject replayEntryPrefab;
    public Transform replayListParent;

    private string selectedReplay = null;
    private TMP_Text selectedText = null;

    public LevelUI[] levels;
    public int selectedLevel = 0;
    public GameObject levelSelectScreen;
    public GameObject replayScreen;

    private bool isLevelSelect = true;

    private void Awake()
    {
        LoadReplayList();
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        SelectLevel();
        for (int i = 0; i < levels.Length; i++)
        {
            string levelName = "Level" + (i + 1);
            float savedTime = PlayerPrefs.GetFloat(levelName + "Time", -1f);
            levels[i].levelTime.text = savedTime >= 0 ? FormatTime(savedTime) : "--:--.--";
        }
    }

    private void LoadReplayList()
    {
        string folder = Path.Combine(Application.persistentDataPath, "_replays");
        if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

        string[] files = Directory.GetFiles(folder, "*.COLDONE");
        foreach (string filePath in files)
        {
            string fileName = Path.GetFileNameWithoutExtension(filePath);
            GameObject entry = Instantiate(replayEntryPrefab, replayListParent);
            TMP_Text text = entry.GetComponentInChildren<TMP_Text>();
            text.text = fileName;

            Button button = entry.GetComponent<Button>();
            button.onClick.AddListener(() => OnReplayEntryClicked(fileName, text));
        }
    }

    private void OnReplayEntryClicked(string fileName, TMP_Text text)
    {
        if (selectedReplay == fileName)
        {
            string folder = Path.Combine(Application.persistentDataPath, "_replays");
            string path = Path.Combine(folder, fileName + ".COLDONE");

            if (File.Exists(path))
            {
                try
                {
                    string json = File.ReadAllText(path);
                    ReplayData data = JsonConvert.DeserializeObject<ReplayData>(json, settings);

                    UnityEngine.Debug.Log($"Replay confirmed: {fileName}");
                    UnityEngine.Debug.Log($"Scene: {data.scene}, Enemies: {data.enemyData?.Count ?? 0}");

                    ReplayRecordingManager.Instance.isReplay = true;
                    ReplayRecordingManager.Instance.loadedReplay = data;

                    PlayReplay();
                }
                catch (Exception ex)
                {
                    UnityEngine.Debug.LogError($"Failed to parse replay JSON: {ex.Message}");
                }
            }
            else
            {
                UnityEngine.Debug.LogError($"Replay file not found at path: {path}");
            }
        }
        else
        {
            if (selectedText != null)
                selectedText.color = Color.white;

            selectedReplay = fileName;
            selectedText = text;
            text.color = Color.cyan;
        }
    }



    private void PlayReplay()
    {
        var data = ReplayRecordingManager.Instance.loadedReplay;
        if (data == null)
        {
            UnityEngine.Debug.LogError("Replay data not loaded.");
            return;
        }

        // Scene name is stored as "Level1", "Level2", etc.
        SceneManager.LoadScene(data.scene);
    }


    private void Update()
    {
        if (!isLevelSelect) return;

        if (UnityEngine.Input.GetKeyDown(KeyCode.A) || UnityEngine.Input.GetKeyDown(KeyCode.LeftArrow))
        {
            selectedLevel = (selectedLevel - 1 + levels.Length) % levels.Length;
            SelectLevel();
        }

        if (UnityEngine.Input.GetKeyDown(KeyCode.D) || UnityEngine.Input.GetKeyDown(KeyCode.RightArrow))
        {
            selectedLevel = (selectedLevel + 1) % levels.Length;
            SelectLevel();
        }

        if (UnityEngine.Input.GetKeyDown(KeyCode.Space))
        {
            string levelName = "level" + (selectedLevel + 1);
            ReplayRecordingManager.Instance.isReplay = false;
            SceneManager.LoadScene(levelName);
        }

        if (levels[selectedLevel].levelSymbol != null)
        {
            levels[selectedLevel].levelSymbol.transform.Rotate(0f, 180f * Time.deltaTime, 0f);
        }
    }

    private void SelectLevel()
    {
        for (int i = 0; i < levels.Length; i++)
        {
            levels[i].SetSelected(i == selectedLevel);
        }
    }

    private string FormatTime(float time)
    {
        int minutes = Mathf.FloorToInt(time / 60f);
        int seconds = Mathf.FloorToInt(time % 60f);
        int centiseconds = Mathf.FloorToInt((time * 100) % 100);

        return string.Format("{0:00}:{1:00}.{2:00}", minutes, seconds, centiseconds);
    }

    public void SwitchScreen()
    {
        levelSelectScreen.SetActive(!isLevelSelect);
        replayScreen.SetActive(isLevelSelect);

        isLevelSelect = !isLevelSelect;
    }

    public void OpenReplayFolder()
    {
        string folder = Path.Combine(Application.persistentDataPath, "_replays");

        if (!Directory.Exists(folder))
        {
            Directory.CreateDirectory(folder);
            UnityEngine.Debug.Log("Created _replays folder: " + folder);
        }

        UnityEngine.Debug.Log("Attempting to open folder: " + folder);

#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
        try
        {
            // Use folder as FileName directly to avoid ! issues
            ProcessStartInfo startInfo = new ProcessStartInfo()
            {
                FileName = folder,
                UseShellExecute = true,
                Verb = "open"
            };
            Process.Start(startInfo);
        }
        catch (Exception e)
        {
            UnityEngine.Debug.LogError("Failed to open folder in Explorer: " + e.Message);
        }

#elif UNITY_EDITOR_LINUX || UNITY_STANDALONE_LINUX
    try
    {
        Process.Start("xdg-open", folder);
    }
    catch (Exception e)
    {
        UnityEngine.Debug.LogError("Failed to open folder in Linux: " + e.Message);
    }
#else
    Debug.LogWarning("Opening folders is not supported on this platform.");
#endif
    }

}

[Serializable]
public class LevelUI
{
    public Image levelSymbol;

    public TextMeshProUGUI levelName;
    public TextMeshProUGUI levelTime;
    public TextMeshProUGUI spaceToPlay;

    public void SetSelected(bool status)
    {
        levelSymbol.transform.localRotation = Quaternion.Euler(0, 0, 0);
        levelName.enabled = status;
        levelTime.enabled = status;
        spaceToPlay.enabled = status;
    }
}