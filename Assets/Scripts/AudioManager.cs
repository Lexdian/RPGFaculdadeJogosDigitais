using System.Collections;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource audioSource1;
    [SerializeField] private AudioSource audioSource2;

    private AudioSource activeSource;
    private Coroutine fadeCoroutine;
    private float maxVolume = 0.1f; // Volume máximo padrão da música

    void Awake()
    {
        // Garante que exista apenas uma instância do AudioManager (Singleton)
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Define a fonte inicial ativa
        activeSource = audioSource1;
    }

    /// <summary>
    /// Para a música atual imediatamente.
    /// </summary>
    public void StopMusic()
    {
        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        activeSource.Stop();
    }

    /// <summary>
    /// Troca a música instantaneamente (ou toca se não houver nenhuma).
    /// </summary>
    public void PlayMusicInstant(AudioClip newClip, float volume = 0.1f)
    {
        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);

        maxVolume = volume;
        activeSource.clip = newClip;
        activeSource.volume = maxVolume;

        if (newClip != null)
        {
            activeSource.Play();
        }
        else
        {
            activeSource.Stop();
        }
    }

    /// <summary>
    /// Troca a música suavemente com efeito de Fade Out e Fade In.
    /// </summary>
    public void PlayMusicWithFade(AudioClip newClip, float fadeDuration = 1.5f, float targetVolume = 0.1f)
    {
        // Interrompe transições anteriores se ainda estiverem acontecendo
        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);

        // Se a música nova for igual à que já está tocando e está ativa, não faz nada
        if (activeSource.clip == newClip && activeSource.isPlaying) return;

        // Alterna entre as duas fontes de áudio para fazer o crossfade
        AudioSource newSource = (activeSource == audioSource1) ? audioSource2 : audioSource1;

        fadeCoroutine = StartCoroutine(FadeTransition(newClip, newSource, fadeDuration, targetVolume));
    }

    private IEnumerator FadeTransition(AudioClip newClip, AudioSource newSource, float duration, float targetVolume)
    {
        float time = 0;
        float startVolume = activeSource.volume;

        // 1. Fade Out da música atual
        if (activeSource.isPlaying)
        {
            while (time < duration / 2)
            {
                time += Time.deltaTime;
                activeSource.volume = Mathf.Lerp(startVolume, 0, time / (duration / 2));
                yield return null;
            }
            activeSource.Stop();
        }

        // Prepara a nova música
        newSource.clip = newClip;
        maxVolume = targetVolume;

        // 2. Fade In da nova música
        if (newClip != null)
        {
            newSource.Play();
            time = 0;
            while (time < duration / 2)
            {
                time += Time.deltaTime;
                newSource.volume = Mathf.Lerp(0, maxVolume, time / (duration / 2));
                yield return null;
            }
            newSource.volume = maxVolume;
        }

        // Define a nova fonte como a ativa
        activeSource = newSource;
    }
}