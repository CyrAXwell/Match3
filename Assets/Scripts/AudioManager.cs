using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    [Header("AudioMixer")]
    [SerializeField] private AudioMixer audioMixer;

    [Header("AudioSources")]
    [SerializeField] private AudioSource sFXSource;

    [Header("Sounds")]
    [SerializeField] public AudioClip MatchSound;
    [SerializeField] public AudioClip SwapSound;
    [SerializeField] public AudioClip TrySwapSound;
    [SerializeField] public AudioClip GlassSound;
    [SerializeField] public AudioClip ButtonSound;
    [SerializeField] public AudioClip WinSound;
    [SerializeField] public AudioClip LoseSound;

    private void Awake()
    {
        audioMixer.SetFloat("sfx", Mathf.Log10(1)*20);
    }

    public void PlaySFX(AudioClip clip)
    {
        sFXSource.PlayOneShot(clip, 1f);
    }

    public void MuteSFX()
    {
        sFXSource.mute = true;
    }

    public void UnMuteSFX()
    {
        sFXSource.mute = false;
    }
}
