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
            CameraLook.Instance.SetReplayRotation(first.rotationY);
            CameraLook.Instance.ForceSetCurrentRotation(first.rotationY);
        }
    }

    void Update()
    {
        double elapsedReplayTime = Time.timeSinceLevelLoad;

        while (nextEventIndex < inputEvents.Count &&
               inputEvents[nextEventIndex].timestamp <= elapsedReplayTime)
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

        Player.Instance.SetReplayPosition(evt.position);
        Player.Instance.SetReplayMovement(evt.inputDir);
        Player.Instance.SetReplayRotation(evt.rotationY);
        Player.Instance.SetReplaySprinting(evt.isSprinting);
        if (evt.jump)
            Player.Instance.TriggerReplayJump();
        if (evt.leftClick)
            Player.Instance.TriggerReplayClick();
        if (evt.rightClick)
            Player.Instance.TriggerReplayRightClick();
        if (evt.konamiCode)
            FindAnyObjectByType<KonamiCode>()?.cheatResult?.Invoke();
        if (!string.IsNullOrEmpty(evt.interactId))
        {
            GameObject target = GameObject.Find(evt.interactId);
            target?.GetComponent<IInteractable>()?.OnInteract();
        }
    }

    private void EndReplay()
    {
        SceneManager.LoadScene("LevelSelect");
    }
}
