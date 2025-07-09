using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class TimerDisplay : MonoBehaviour
{
    public TextMeshProUGUI timerText;

    public float elapsedTime = 0f;

    private void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.isCutscene)
            return;

        elapsedTime += Time.deltaTime;
        timerText.text = FormatTime(elapsedTime);
    }

    private string FormatTime(float time)
    {
        int minutes = Mathf.FloorToInt(time / 60f);
        int seconds = Mathf.FloorToInt(time % 60f);
        int centiseconds = Mathf.FloorToInt((time * 100) % 100); // two decimal places

        return string.Format("{0:00}:{1:00}.{2:00}", minutes, seconds, centiseconds);
    }

    public void ResetTimer()
    {
        elapsedTime = 0f;
        timerText.text = "00:00.00";
    }

    public void SaveTime()
    {
        string levelName = SceneManager.GetActiveScene().name;
        string key = $"{levelName}Time";

        if (!PlayerPrefs.HasKey(key) || elapsedTime < PlayerPrefs.GetFloat(key))
        {
            PlayerPrefs.SetFloat(key, elapsedTime);
            PlayerPrefs.Save();
        }
    }

}
