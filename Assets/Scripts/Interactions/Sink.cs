using System.Collections;
using UnityEngine;

public class Sink : MonoBehaviour, IInteractable
{
    public float minTempDecrease = 0.5f;
    public float maxTempDecrease = 1.5f;

    public GameObject water;

    [Header("Replay stuff")]
    [SerializeField]
    private int randomSeed;
    public string sinkId;

    private System.Random random;
    private bool isActive;

    private void Awake()
    {
        water.SetActive(false);

        // Unique ID generation
        if (WaveManager.Instance != null)
        {
            sinkId += WaveManager.Instance.waveNumber.ToString() + "_";
        }
        sinkId += transform.parent.gameObject.name;
    }

    private void Start()
    {
        if (ReplayRecordingManager.Instance.isReplay)
        {
            randomSeed = ReplayEnemyTracker.Instance.LoadSink(sinkId);
            random = new System.Random(randomSeed);
        }
        else
        {
            randomSeed = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
            random = new System.Random(randomSeed);
            ReplayEnemyTracker.Instance.RegisterSink(sinkId, randomSeed);
        }
        gameObject.name += transform.parent.name;
    }

    public void OnInteract()
    {
        UnityEngine.Debug.Log("Invoked");
        if (isActive) return;
        isActive = true;

        float tempDecrease = (float)(random.NextDouble() * (maxTempDecrease - minTempDecrease) + minTempDecrease);
        Player.Instance.heat.ChangeTemp(-tempDecrease);

        StartCoroutine(ToggleWater());
    }

    private IEnumerator ToggleWater()
    {
        water.SetActive(true);
        Player.Instance.sound.SinkSound();
        yield return new WaitForSeconds(0.75f);
        water.SetActive(false);
        isActive = false;
    }
}
