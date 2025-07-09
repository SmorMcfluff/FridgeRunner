using System.Collections;
using UnityEngine;

public class Hjalmar : MonoBehaviour, IDamageable
{
    public bool isDead = false;
    public Sprite deadSprite;

    public void TakeDamage(int dmg)
    {
        if (isDead) return;
        isDead = true;

        var billboard = GetComponentInChildren<BillboardToCamera>();
        if (billboard != null)
            StartCoroutine(Flip(billboard));
    }

    private IEnumerator Flip(BillboardToCamera billboard)
    {
        float duration = 0.5f;
        float elapsed = 0f;

        float startZ = billboard.zOffset;
        float endZ = 180f;

        GetComponentInChildren<SpriteRenderer>().sprite = deadSprite;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            // Optional: ease-in-out
            t = Mathf.SmoothStep(0f, 1f, t);

            billboard.zOffset = Mathf.Lerp(startZ, endZ, t);
            yield return null;
        }

        billboard.zOffset = endZ; // Snap to final value
    }
}
