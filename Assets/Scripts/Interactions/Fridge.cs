using System.Collections;
using UnityEngine;

public class Fridge : MonoBehaviour, IInteractable
{
    public Transform playerStand;
    public Transform playerLook;

    Player player;

    public Light[] lightSrc;
    private float[] intensity;
    private float[] range;

    private bool isActivated = false;

    private void Start()
    {
        player = Player.Instance;

        intensity = new float[lightSrc.Length];
        range = new float[lightSrc.Length];

        for (int i = 0; i < lightSrc.Length; i++)
        {
            intensity[i] = lightSrc[i].intensity;
            range[i] = lightSrc[i].range;

            lightSrc[i].intensity = 0;
            lightSrc[i].range = 0;
        }
        gameObject.name += transform.parent.name;
    }

    private void Update()
    {
        if (isActivated)
        {
            Vector3 lookDir = playerLook.position - player.transform.position;
            lookDir.y = 0;
            lookDir.Normalize();
            Quaternion targetRot = Quaternion.LookRotation(lookDir);
            player.transform.rotation = Quaternion.Slerp(player.transform.rotation, targetRot, Time.deltaTime * 5f);
        }
    }

    public void OnInteract()
    {
        UnityEngine.Debug.Log("Invoked");
        if (isActivated) return;
        isActivated = true;
        GameManager.Instance.EndLevel(true);

        player.GetComponent<Rigidbody>().isKinematic = true;

        StartCoroutine(MovePlayer());
        StartCoroutine(OpenDoor());
        StartCoroutine(LampFadeIn());
        player.handAnim.PlayFinishAnimation();
    }

    IEnumerator MovePlayer()
    {
        FindAnyObjectByType<MusicManager>().musicSource.Stop();
        float duration = 0.2f;
        float elapsed = 0;

        Vector3 initPlayerPos = player.transform.position;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            Vector3 playerPos = Vector3.Lerp(initPlayerPos, playerStand.position, t);
            player.transform.position = playerPos;
            yield return null;
        }
        player.transform.position = playerStand.position;
    }

    IEnumerator OpenDoor()
    {
        float duration = 5;
        float elapsed = 0;

        while (elapsed < duration)
        {
            float t = elapsed / duration;

            float height = Mathf.Lerp(1.4f, 4.2f, t);

            Vector3 pos = new(0, height, 0);
            transform.localPosition = pos;
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.localPosition = new(0, 4.2f, 0);

    }

    IEnumerator LampFadeIn()
    {
        yield return new WaitForSeconds(2.5f);
        float duration = 3;
        float elapsed = 0;

        while (elapsed < duration)
        {
            float t = elapsed / duration;

            for (int i = 0; i < lightSrc.Length; i++)
            {
                lightSrc[i].intensity = Mathf.Lerp(0, intensity[i], t);
                lightSrc[i].range = Mathf.Lerp(0, range[i], t);
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        for (int i = 0; i < lightSrc.Length; i++)
        {
            lightSrc[i].intensity = intensity[i];
            lightSrc[i].range = range[i];
        }
    }
}
