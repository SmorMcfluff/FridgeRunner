using System.Collections;
using TMPro;
using UnityEngine;

public class Door : MonoBehaviour, IInteractable
{
    public Transform doorHinge;
    public float openSpeed = 1.5f;

    public bool isOpen = false;
    public bool needsCard = false;

    public TextMeshProUGUI interactionMessage;

    private Quaternion openRotation;
    private Quaternion closedRotation;

    private Coroutine currentCoroutine;
    private Coroutine messageCoroutine;

    private void Start()
    {
        closedRotation = doorHinge.rotation;
        if (interactionMessage != null)
            interactionMessage.gameObject.SetActive(false);
        gameObject.name += transform.parent.name;
    }

    public void OnInteract()
    {
        Debug.Log("Invoked");
        if (needsCard && !Player.Instance.hasKeycard)
        {
            ShowMessage("Need keycard");
            return;
        }

        if (currentCoroutine != null)
        {
            StopCoroutine(currentCoroutine);
        }
        currentCoroutine = StartCoroutine(ToggleDoor());
    }

    private IEnumerator ToggleDoor()
    {
        Vector3 toPlayer = Player.Instance.transform.position - doorHinge.position;
        Vector3 awayDir = -toPlayer.normalized;

        Vector3 localForward = doorHinge.forward;
        Vector3 cross = Vector3.Cross(localForward, awayDir);
        float direction = cross.y >= 0 ? 1f : -1f;

        openRotation = closedRotation * Quaternion.Euler(0, 90f * direction, 0);

        Quaternion startRotation = doorHinge.rotation;
        Quaternion targetRotation = isOpen ? closedRotation : openRotation;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * openSpeed;
            doorHinge.rotation = Quaternion.Slerp(startRotation, targetRotation, t);
            yield return null;
        }
        doorHinge.rotation = targetRotation;

        isOpen = !isOpen;
        currentCoroutine = null;
    }

    private void ShowMessage(string msg)
    {
        if (interactionMessage == null) return;

        if (messageCoroutine != null)
            StopCoroutine(messageCoroutine);

        messageCoroutine = StartCoroutine(ShowMessageRoutine(msg));
    }

    private IEnumerator ShowMessageRoutine(string msg)
    {
        interactionMessage.text = msg;
        interactionMessage.gameObject.SetActive(true);

        yield return new WaitForSeconds(1f);

        interactionMessage.gameObject.SetActive(false);
        messageCoroutine = null;
    }
}
