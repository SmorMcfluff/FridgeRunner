using System.Collections;
using UnityEngine;

public class EnemySprite : MonoBehaviour
{
    public Color defaultColor = Color.white;
    private SpriteRenderer sr;
    private Coroutine flashRoutine;

    [Tooltip("Enable if this sprite should always face the camera.")]
    public bool billboardToCamera = true;

    IEnumerator Start()
    {
        yield return new WaitForEndOfFrame();
        sr = GetComponent<SpriteRenderer>();
        sr.color = defaultColor;
    }

    void LateUpdate()
    {
        if (billboardToCamera && Camera.main != null)
        {
            transform.forward = Camera.main.transform.forward;
        }
    }

    public void DamageRedFlash()
    {
        if (flashRoutine != null)
        {
            StopCoroutine(flashRoutine);
        }
        flashRoutine = StartCoroutine(FlashRed());
    }

    private IEnumerator FlashRed()
    {
        sr.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        sr.color = defaultColor;
        flashRoutine = null;
    }

    public void DeathGrey()
    {
        if (flashRoutine != null)
        {
            StopCoroutine(flashRoutine);
            flashRoutine = null;
        }
        sr.color = Color.grey;
    }
}
