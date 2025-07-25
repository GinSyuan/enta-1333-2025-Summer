using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    public AudioSource sfxSource;
    public AudioClip placeBuildingSFX;
    public AudioClip boredSFX;
    public AudioClip combatSFX;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); 
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void PlaySFX(AudioClip clip)
    {
        sfxSource.PlayOneShot(clip);
    }

    public void PlayPlaceBuilding() => PlaySFX(placeBuildingSFX);
    public void PlayBored() => PlaySFX(boredSFX);
    public void PlayCombat() => PlaySFX(combatSFX);
}
