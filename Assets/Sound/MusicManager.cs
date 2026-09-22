using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance { get; private set; }

    [Header("Audio Clips")]
    [SerializeField] private AudioClip musicClip;
    [SerializeField] private AudioClip gameOverClip;
    [SerializeField] private AudioClip buttonClickClip;
    [SerializeField] private AudioClip transitionClip;
    [SerializeField] private AudioClip negativeScoreClip;
    [SerializeField] private AudioClip shieldSound;
    [SerializeField] private AudioClip slowMotionSound;

    [Header("Mixer")]
    [SerializeField] private AudioMixerGroup mixerGroup;

    private AudioSource musicSource;
    private AudioSource sfxSource;
    private AudioSource transitionSource;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.clip = musicClip;
        musicSource.loop = true;
        musicSource.outputAudioMixerGroup = mixerGroup;

        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.loop = false;
        sfxSource.outputAudioMixerGroup = mixerGroup;

        transitionSource = gameObject.AddComponent<AudioSource>();
        transitionSource.clip = transitionClip;
        transitionSource.loop = true;
        transitionSource.outputAudioMixerGroup = mixerGroup;
    }

    private void Start()
    {
        musicSource.Play();

        Button[] allButtons = FindObjectsOfType<Button>(true);
        foreach (Button btn in allButtons)
        {
            btn.onClick.AddListener(PlayButtonClick);
        }
    }

    public void StopMusic() => musicSource.Stop();

    public void PlayGameOverMusic() => sfxSource.PlayOneShot(gameOverClip);

    public void PlayButtonClick() => sfxSource.PlayOneShot(buttonClickClip);

    public void PlayTransitionSound() => transitionSource.Play();

    public void StopTransitionSound() => transitionSource.Stop();

    public void PlayNegativeScoreSound() => sfxSource.PlayOneShot(negativeScoreClip);

    public void PlayShieldSound() => sfxSource.PlayOneShot(shieldSound);

    public void PlaySlowMotionSound() => sfxSource.PlayOneShot(slowMotionSound);

    public void FadeOutAndStop(float duration = 1f) => StartCoroutine(FadeOut(duration));

    private System.Collections.IEnumerator FadeOut(float duration)  //per possibile implementazione futura
    {
        float startVolume = musicSource.volume;
        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            musicSource.volume = Mathf.Lerp(startVolume, 0f, t / duration);
            yield return null;
        }
        musicSource.Stop();
        musicSource.volume = startVolume;
    }
}