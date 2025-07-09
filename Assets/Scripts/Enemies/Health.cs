using UnityEngine;
using UnityEngine.Events;

public class Health : MonoBehaviour, IDamageable
{
    public int maxHealth = 10;
    private int currentHealth;
    private EnemySprite sprite;

    public bool IsDead => currentHealth <= 0;

    public UnityEvent OnDeath;

    public AudioSource audioSource;
    public AudioClip[] damageSounds;
    public AudioClip[] deathSounds;

    [Range(0.8f, 1.2f)]
    public float pitchVariationMin = 0.9f;
    [Range(0.8f, 1.2f)]
    public float pitchVariationMax = 1.1f;

    private void Start()
    {
        currentHealth = maxHealth;
        sprite = GetComponentInChildren<EnemySprite>();
    }

    public void TakeDamage(int amount)
    {
        if (IsDead) return;

        currentHealth -= amount;
        currentHealth = Mathf.Max(0, currentHealth);

        if (currentHealth <= 0)
        {
            Die(); // only play death sound
        }
        else
        {
            if (sprite != null)
            {
                sprite.DamageRedFlash();
            }
            PlayRandomSound(damageSounds);
        }
    }


    private void Die()
    {
        PlayRandomSound(deathSounds);
        if (CompareTag("Enemy"))
        {
            sprite.DeathGrey();
            Invoke(nameof(RemoveObject), 2);
        }
        else
        {
            RemoveObject();
        }
        OnDeath?.Invoke();

    }

    private void PlayRandomSound(AudioClip[] clips)
    {
        if (clips == null || clips.Length == 0 || audioSource == null) return;
        audioSource.Stop();
        AudioClip clip = clips[Random.Range(0, clips.Length)];
        audioSource.pitch = Random.Range(pitchVariationMin, pitchVariationMax);
        audioSource.clip = clip;
        audioSource.Play();
    }

    private void RemoveObject()
    {
        Destroy(gameObject);
    }
}
