using System.Collections;
using TMPro;
using UnityEngine;

public class WaveManager : MonoBehaviour
{
    public static WaveManager Instance;

    public TextMeshProUGUI waveText;
    public TextMeshProUGUI enemiesLeftText;

    public GameObject door;
    public GameObject fridge;

    public GameObject[] waves;
    private int enemyCount;

    public int waveNumber = -1;

    private void Awake()
    {
        Instance = this;
    }

    public void StartWaves()
    {
        Destroy(door);
        StartCoroutine(NextWave());
    }

    private IEnumerator NextWave()
    {
        waveNumber++;
        if (waveNumber == 5)
        {
            LowerFridge();
            yield break;
        }

        float duration = 0.5f;
        float elapsed = 0;

        waveText.text = "Wave " + (waveNumber + 1);
        waveText.color = Color.clear;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            waveText.color = Color.Lerp(Color.clear, Color.white, t);
            yield return null;
        }
        waveText.color = Color.white;

        yield return new WaitForSeconds(2);

        elapsed = 0;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            waveText.color = Color.Lerp(Color.white, Color.clear, t);
            yield return null;
        }

        Instantiate(waves[waveNumber]);
        enemyCount = GetEnemyCount();
        UpdateEnemyCountText();
    }

    private int GetEnemyCount()
    {
        return waveNumber switch
        {
            0 => 7,
            1 => 10,
            2 => 12,
            3 => 20,
            _ => 33,
        };
    }


    private void LowerFridge()
    {
        StartCoroutine(FridgeMessageAndLowerRoutine());
    }

    private IEnumerator FridgeMessageAndLowerRoutine()
    {
        float fadeDuration = 0.5f;
        float elapsed = 0f;

        waveText.gameObject.SetActive(true);
        waveText.text = "Fridge is in the middle of the house";
        waveText.color = Color.clear;

        // Fade in
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeDuration;
            waveText.color = Color.Lerp(Color.clear, Color.white, t);
            yield return null;
        }
        waveText.color = Color.white;

        // Wait while text is fully visible
        yield return new WaitForSeconds(2f);

        // Fade out
        elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeDuration;
            waveText.color = Color.Lerp(Color.white, Color.clear, t);
            yield return null;
        }
        waveText.color = Color.clear;

        // Lower the fridge
        Vector3 startPos = fridge.transform.position;
        Vector3 endPos = startPos + Vector3.down * 15f;
        float lowerDuration = 5f;
        elapsed = 0f;

        while (elapsed < lowerDuration)
        {
            elapsed += Time.deltaTime;
            fridge.transform.position = Vector3.Lerp(startPos, endPos, elapsed / lowerDuration);
            yield return null;
        }
        fridge.transform.position = endPos;
    }


    public void EnemyDied()
    {
        enemyCount--;
        UpdateEnemyCountText();
        if (enemyCount <= 0)
        {
            StartCoroutine(NextWave());
        }
    }

    private void UpdateEnemyCountText()
    {
        enemiesLeftText.color = Color.white;
        enemiesLeftText.text = enemyCount + " enemies left";
    }
}
