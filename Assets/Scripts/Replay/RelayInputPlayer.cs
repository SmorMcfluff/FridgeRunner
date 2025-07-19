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

        if (inputEvents.Count > 0)
        {
            var first = inputEvents[0];

            // Set initial rotation
            CameraLook.Instance.SetReplayRotation(first.ry);
            CameraLook.Instance.ForceSetCurrentRotation(first.ry);
        }
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

        Player.Instance.SetReplayPosition(evt.pos);
        Player.Instance.SetReplayMovement(evt.id);
        Player.Instance.SetReplayRotation(evt.ry);
        Player.Instance.SetReplaySprinting(evt.iS);
        if (evt.j)
            Player.Instance.TriggerReplayJump();
        if (evt.lc)
            Player.Instance.TriggerReplayClick();
        if (evt.rc)
            Player.Instance.TriggerReplayRightClick();
        if (evt.kc)
            FindAnyObjectByType<KonamiCode>()?.cheatResult?.Invoke();
        if (!string.IsNullOrEmpty(evt.i))
        {
            GameObject target = GameObject.Find(evt.i);
            target?.GetComponent<IInteractable>()?.OnInteract();
        }
    }

    private void EndReplay()
    {
        SceneManager.LoadScene("LevelSelect");
    }
}
