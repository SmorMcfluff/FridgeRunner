using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public AudioSource musicSource;
    private void Awake()
    {
        musicSource = GetComponent<AudioSource>();
    }
}
