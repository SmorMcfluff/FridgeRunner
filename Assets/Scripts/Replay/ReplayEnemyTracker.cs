using System.Collections.Generic;
using UnityEngine;

public class ReplayEnemyTracker : MonoBehaviour
{
    public static ReplayEnemyTracker Instance;

    public List<EnemyReplayData> enemyDataList = new();

    private void Awake()
    {
        Instance = this;
        if (ReplayRecordingManager.Instance.isReplay)
        {
            enemyDataList = ReplayRecordingManager.Instance.loadedReplay.enemyData;
        }
    }

    public int LoadEnemy(string enemyID)
    {
        var entry = enemyDataList.Find(e => e.enemyID == enemyID);

        if (entry != null)
        {
            return entry.seed;
        }

        Debug.LogError($"Seed not found for enemyID: {enemyID}");
        return 0;
    }

    public void RegisterEnemy(string enemyID, int seed)
    {
        if (!enemyDataList.Exists(e => e.enemyID == enemyID))
        {
            enemyDataList.Add(new EnemyReplayData(enemyID, seed));
        }
        else
        {
            Debug.LogWarning($"Enemy '{enemyID}' is already registered with a seed.");
        }
    }

    public void RegisterSink(string sinkID, int seed)
    {
        if (!enemyDataList.Exists(e => e.enemyID == sinkID))
        {
            enemyDataList.Add(new EnemyReplayData(sinkID, seed));
        }
        else
        {
            Debug.LogWarning($"Sink '{sinkID}' is already registered with a seed.");
        }
    }

    public int LoadSink(string sinkID)
    {
        var entry = enemyDataList.Find(e => e.enemyID == sinkID);
        if (entry != null) return entry.seed;

        Debug.LogError($"Seed not found for sinkID: {sinkID}");
        return 0;
    }
}

[System.Serializable]
public class EnemyReplayData
{
    public string enemyID;
    public int seed;

    public EnemyReplayData(string enemyID, int seed)
    {
        this.enemyID = enemyID;
        this.seed = seed;
    }
}

[SerializeField]
public class EnemyReplayDataList
{
    public List<EnemyReplayData> data = new();
}