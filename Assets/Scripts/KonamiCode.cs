using UnityEngine;
using UnityEngine.Events;

public class KonamiCode : MonoBehaviour
{
    public UnityEvent cheatResult;

    private KeyCode[] konamiCode = new KeyCode[]
    {
        KeyCode.UpArrow,
        KeyCode.UpArrow,
        KeyCode.DownArrow,
        KeyCode.DownArrow,
        KeyCode.LeftArrow,
        KeyCode.RightArrow,
        KeyCode.LeftArrow,
        KeyCode.RightArrow,
        KeyCode.B,
        KeyCode.A
    };

    private int currentIndex = 0;

    void Update()
    {
        if (Input.anyKeyDown)
        {
            if (Input.GetKeyDown(konamiCode[currentIndex]))
            {
                currentIndex++;
                if (currentIndex >= konamiCode.Length)
                {
                    cheatResult?.Invoke();
                    ReplayRecordingManager.Instance.inputList.Add(new InputEvent(
                        Time.timeSinceLevelLoad,
                        Vector2.zero,
                        0f,
                        false,
                        false, false, false,
                        true,
                        Player.Instance.transform.position,
                        ""
                    ));

                    currentIndex = 0;
                }
            }
            else if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.DownArrow) ||
                     Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.RightArrow) ||
                     Input.GetKeyDown(KeyCode.B) || Input.GetKeyDown(KeyCode.A))
            {
                currentIndex = 0;
            }
        }
    }
}
