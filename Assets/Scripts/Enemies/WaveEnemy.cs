using System.Collections;
using UnityEngine;

public class WaveEnemy : MonoBehaviour
{
    public static WaveEnemy Instance;

    WaveManager waveManager;
    private void Awake()
    {
        Instance = this;
        waveManager = FindAnyObjectByType<WaveManager>();
    }

    private IEnumerator Start()
    {
        yield return null;
        yield return null;
        GetComponent<EnemyAI>().attackRange = 200f;
    }

    public void OnDeath()
    {
        waveManager.EnemyDied();
    }
}
