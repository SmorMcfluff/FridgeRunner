using System.Collections;
using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public TimerDisplay timer;
    public TextMeshProUGUI countdownText;
    public TextMeshProUGUI interactionText;

    public static GameManager Instance;
    public bool isCutscene;

    private void Awake()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        Instance = this;
        Application.targetFrameRate = 60;
        isCutscene = true;
        StartCoroutine(Countdown());
    }

    public void EndLevel(bool status)
    {
        isCutscene = status;
        if (status)
        {
            interactionText.enabled = false;
            timer.SaveTime();
        }
    }

    private IEnumerator Countdown()
    {
        yield return new WaitForSeconds(0.5f);
        countdownText.enabled = true;
        countdownText.color = Color.red;
        countdownText.text = "3";
        Player.Instance.sound.Countdown(0);

        yield return new WaitForSeconds(1);
        Player.Instance.sound.Countdown(0);
        countdownText.color = new(1, 0.647f, 0);
        countdownText.text = "2";

        yield return new WaitForSeconds(1);
        Player.Instance.sound.Countdown(0);
        countdownText.color = Color.yellow;
        countdownText.text = "1";

        yield return new WaitForSeconds(0.8f);
        isCutscene = false;

        yield return new WaitForSeconds(0.2f);
        Player.Instance.sound.Countdown(1);
        countdownText.enabled = false;
        yield return new WaitForSeconds(0.2f);
        FindAnyObjectByType<MusicManager>().musicSource.Play();
    }
}
