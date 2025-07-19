using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using Newtonsoft.Json;

public class ReplayFileManager : MonoBehaviour
{
    private static readonly JsonSerializerSettings settings = new JsonSerializerSettings
    {
        TypeNameHandling = TypeNameHandling.Auto,
        Formatting = Formatting.None,
        ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
        ContractResolver = new Newtonsoft.Json.Serialization.DefaultContractResolver()
        {
            IgnoreSerializableInterface = true,
            IgnoreSerializableAttribute = true,
        }
    };

    /// <summary>Returns the full path for a replay file based on the provided data.</summary>
    private static string FilePath(ReplayData data, string time)
    {
        string folder = EnsureReplayFolder();
        string scene = data.scene switch
        {
            "Level1" => "House",
            "Level2" => "Maze",
            "Level3" => "esouH",
            _=> "SceneName"
        };
        string sanitizedTime = time.Replace(":", "-").Replace(".", "-");
        string baseFileName = $"{SanitizeFileName(scene)}_{sanitizedTime}";
        string extension = ".COLDONE";

        string fullPath = Path.Combine(folder, baseFileName + extension);
        int counter = 1;
        while (File.Exists(fullPath))
        {
            fullPath = Path.Combine(folder, $"{baseFileName}_{counter++}{extension}");
        }
        return fullPath;
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
    public static string SaveToFile(ReplayData data, string time)
    {
        string path = FilePath(data, time);

        string json = JsonConvert.SerializeObject(data, settings);
        File.WriteAllText(path, json);
        Debug.Log($"Replay saved to: {path}");
        return path;
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
