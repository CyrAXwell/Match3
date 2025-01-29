using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    [Header("AudioMixer")]
    [SerializeField] private AudioMixer _audioMixer;

    [Header("AudioSources")]
    [SerializeField] private AudioSource _sFXSource;

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
        _audioMixer.SetFloat("sfx", Mathf.Log10(1)*20);
    }

    public void PlaySFX(AudioClip clip)
    {
        _sFXSource.PlayOneShot(clip, 1f);
    }

    public void MuteSFX()
    {
        _sFXSource.mute = true;
    }

    public void UnMuteSFX()
    {
        _sFXSource.mute = false;
    }
}
