using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ReplayFileManager : MonoBehaviour
{
    /// <summary>Returns the full path for a replay file based on the provided data.</summary>
    private static string FilePath(ReplayData data)
    {
        string folder = EnsureReplayFolder();
        string scene = data.scene switch
        {
            "Level1" => "House",
            "Level2" => "Maze",
            "Level3" => "esouH",
            _=> "SceneName"
        };
        string sanitizedTime = data.time.Replace(":", "-").Replace(".", "-");
        string fileName = $"{SanitizeFileName(scene)}_{sanitizedTime}.COLDONE";
        return Path.Combine(folder, fileName);
    }

    /// <summary>Creates the replay folder if it doesn't exist and returns its path.</summary>
    private static string EnsureReplayFolder()
    {
        string folder = Path.Combine(Application.persistentDataPath, "_replays");
        if (!Directory.Exists(folder))
        {
            Directory.CreateDirectory(folder);
        }
        return folder;
    }

    /// <summary>Saves a ReplayData object to disk and returns the full file path.</summary>
    public static string SaveToFile(ReplayData data)
    {
        string path = FilePath(data);
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(path, json);
        Debug.Log($"Replay saved to: {path}");
        return path;
    }

    /// <summary>Loads a replay from a specified file path.</summary>
    public static List<EnemyReplayData> LoadFromFile(string filePath)
    {
        if (!File.Exists(filePath))
        {
            Debug.LogWarning($"Replay file not found at {filePath}");
            return new List<EnemyReplayData>();
        }

        string json = File.ReadAllText(filePath);
        EnemyReplayDataList wrapper = JsonUtility.FromJson<EnemyReplayDataList>(json);
        return wrapper?.data ?? new List<EnemyReplayData>();
    }

    /// <summary>Generates a filename based on the current scene and timestamp.</summary>
    private static string GetFileName()
    {
        string scene = SceneManager.GetActiveScene().name switch
        {
            "Level1" => "House",
            "Level2" => "Maze",
            "Level3" => "esouH",
            _ => throw new NotImplementedException($"Scene name '{SceneManager.GetActiveScene().name}' not handled."),
        };

        string time = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
        return $"{scene}_{time}";
    }

    /// <summary>Replaces invalid filename characters with underscores.</summary>
    private static string SanitizeFileName(string name)
    {
        foreach (char c in Path.GetInvalidFileNameChars())
        {
            name = name.Replace(c, '_');
        }
        return name;
    }
}
