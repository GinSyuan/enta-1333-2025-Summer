/// <summary>
/// Central manager for playing sound effects and music.
/// Key Usage: Call Play/Stop functions with audio clip names to control audio.
/// </summary>
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

/// <summary>
    /// PlaySFX - Perform this action
    /// </summary>
    public void PlaySFX(AudioClip clip)
    {
        sfxSource.PlayOneShot(clip);
    }

/// <summary>
    /// PlayPlaceBuilding - Perform this action
    /// </summary>
    public void PlayPlaceBuilding() => PlaySFX(placeBuildingSFX);
/// <summary>
    /// PlayBored - Perform this action
    /// </summary>
    public void PlayBored() => PlaySFX(boredSFX);
/// <summary>
    /// PlayCombat - Perform this action
    /// </summary>
    public void PlayCombat() => PlaySFX(combatSFX);
}
