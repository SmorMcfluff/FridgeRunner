using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class PlayerSounds : MonoBehaviour
{
    private AudioSource source;

    public AudioClip sinkSound;
    public AudioClip drinkSound;
    public AudioClip wallSound;
    public AudioClip winSound;

    public AudioClip[] shootSounds;
    public AudioClip[] countdownBeeps;


    private void Awake()
    {
        source = GetComponent<AudioSource>();    
    }

    public void SinkSound()
    {
        source.PlayOneShot(sinkSound);
    }

    public void DrinkSound()
    {
        source.PlayOneShot(drinkSound);
    }

    public void WallSound()
    {
        source.PlayOneShot(wallSound);
    }

    public void WinSound()
    {
        source.PlayOneShot(winSound);
    }

    public void ShootSound()
    {
        Debug.Log("Pew");
        source.pitch = Random.Range(0.95f, 1.05f);
        int r = Random.Range(0, shootSounds.Length);
        source.PlayOneShot(shootSounds[r]);
        source.pitch = 1;
    }

    public void Countdown(int i)
    {
        source.PlayOneShot(countdownBeeps[i]);
    }
}
