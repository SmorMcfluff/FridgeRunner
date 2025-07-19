using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ReplayInputPlayer : MonoBehaviour
{
    private List<InputEvent> inputEvents;
    private int nextEventIndex = 0;

    void Start()
    {
        if (ReplayRecordingManager.Instance == null || !ReplayRecordingManager.Instance.isReplay)
        {
            enabled = false;
            return;
        }

        inputEvents = ReplayRecordingManager.Instance.loadedReplay.inputEvents;

        //if (inputEvents.Count > 0)
        //{
        //    var first = inputEvents[0];

        //    // Set initial rotation
        //    CameraLook.Instance.SetReplayRotation(first.ry);
        //    CameraLook.Instance.ForceSetCurrentRotation(first.ry);
        //}
    }

    void Update()
    {
        double elapsedReplayTime = Time.timeSinceLevelLoad;

        while (nextEventIndex < inputEvents.Count &&
               inputEvents[nextEventIndex].ts <= elapsedReplayTime)
        {
            var evt = inputEvents[nextEventIndex];
            ApplyReplayInput(evt);
            nextEventIndex++;

            if (nextEventIndex >= inputEvents.Count)
                Invoke(nameof(EndReplay), 5f);
        }
    }

    private void ApplyReplayInput(InputEvent evt)
    {
        switch (evt)
        {
            case ME move:
                var p = new Vector3(move.x, move.y, move.z);
                Player.Instance.SetReplayPosition(p);
                break;

            case RE rotation:
                Player.Instance.SetReplayRotation(rotation.ry);
                break;

            case CE click:
                if (click.lc)
                    Player.Instance.TriggerReplayClick();
                if (click.rc)
                    Player.Instance.TriggerReplayRightClick();
                break;

            case KE:
                FindAnyObjectByType<KonamiCode>()?.cheatResult?.Invoke();
                break;

            case IE interact:
                if (!string.IsNullOrEmpty(interact.i))
                {
                    GameObject target = GameObject.Find(interact.i);
                    if (target != null)
                    {
                        target.GetComponent<IInteractable>()?.OnInteract();

                    }
                }
                break;

            default: break;
        }
    }

    private void EndReplay()
    {
        SceneManager.LoadScene("LevelSelect");
    }
}
