using UnityEngine;

public class BGMPlayer : MonoBehaviour
{
    public AudioSource bgmSource;
    public AudioClip bgmClip;

    void Start()
    {
        if (bgmSource != null && bgmClip != null)
        {
            bgmSource.clip = bgmClip;
            bgmSource.loop = true;
            bgmSource.Play();
        }
        else
        {
            Debug.LogWarning("BGM source or clip not set!");
        }
    }
}
