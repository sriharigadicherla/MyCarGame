using UnityEngine;

/// <summary>
/// Synthesizes procedural UI audio clips dynamically at runtime.
/// Requires NO external audio files or dependencies.
/// Provides crisp tactile click, hover tick, modal chime, and race start sounds.
/// </summary>
public class ProceduralUISound : MonoBehaviour
{
    private static ProceduralUISound _instance;
    public static ProceduralUISound Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject go = new GameObject("ProceduralUISound");
                _instance = go.AddComponent<ProceduralUISound>();
                if (Application.isPlaying)
                {
                    DontDestroyOnLoad(go);
                }
            }
            return _instance;
        }
    }

    private AudioSource audioSource;
    private AudioClip clickClip;
    private AudioClip hoverClip;
    private AudioClip modalClip;
    private AudioClip startRaceClip;

    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f; // 2D UI sound

        GenerateClips();
    }

    private void GenerateClips()
    {
        clickClip = CreateSnappyClickClip();
        hoverClip = CreateSoftTickClip();
        modalClip = CreateModalChimeClip();
        startRaceClip = CreateStartRaceClip();
    }

    public void PlayClick()
    {
        if (audioSource != null && clickClip != null)
        {
            audioSource.pitch = Random.Range(0.96f, 1.04f);
            audioSource.PlayOneShot(clickClip, 0.75f);
        }
    }

    public void PlayHover()
    {
        if (audioSource != null && hoverClip != null)
        {
            audioSource.pitch = Random.Range(0.98f, 1.02f);
            audioSource.PlayOneShot(hoverClip, 0.25f);
        }
    }

    public void PlayModalOpen()
    {
        if (audioSource != null && modalClip != null)
        {
            audioSource.pitch = 1.0f;
            audioSource.PlayOneShot(modalClip, 0.65f);
        }
    }

    public void PlayStartRace()
    {
        if (audioSource != null && startRaceClip != null)
        {
            audioSource.pitch = 1.0f;
            audioSource.PlayOneShot(startRaceClip, 0.90f);
        }
    }

    // 1. Crisp high-tech mechanical click (0.045s)
    private AudioClip CreateSnappyClickClip()
    {
        int sampleRate = 44100;
        float duration = 0.045f;
        int numSamples = (int)(sampleRate * duration);
        float[] samples = new float[numSamples];

        for (int i = 0; i < numSamples; i++)
        {
            float t = (float)i / sampleRate;
            float progress = t / duration;
            // Frequency sweeps down from 1400Hz to 400Hz
            float freq = Mathf.Lerp(1400f, 400f, progress);
            float env = Mathf.Exp(-progress * 9f);
            float wave = Mathf.Sin(2f * Mathf.PI * freq * t);
            // Add subtle noise click at the start
            float noise = (Random.value * 2f - 1f) * Mathf.Max(0f, 1f - progress * 5f) * 0.35f;
            samples[i] = Mathf.Clamp(wave * env + noise, -1f, 1f);
        }

        AudioClip clip = AudioClip.Create("UI_Click", numSamples, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    // 2. Subtle soft hover tick (0.018s)
    private AudioClip CreateSoftTickClip()
    {
        int sampleRate = 44100;
        float duration = 0.018f;
        int numSamples = (int)(sampleRate * duration);
        float[] samples = new float[numSamples];

        for (int i = 0; i < numSamples; i++)
        {
            float t = (float)i / sampleRate;
            float progress = t / duration;
            float env = Mathf.Exp(-progress * 14f);
            float wave = Mathf.Sin(2f * Mathf.PI * 880f * t);
            samples[i] = wave * env * 0.5f;
        }

        AudioClip clip = AudioClip.Create("UI_Hover", numSamples, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    // 3. Futuristic modal chime (0.12s)
    private AudioClip CreateModalChimeClip()
    {
        int sampleRate = 44100;
        float duration = 0.12f;
        int numSamples = (int)(sampleRate * duration);
        float[] samples = new float[numSamples];

        for (int i = 0; i < numSamples; i++)
        {
            float t = (float)i / sampleRate;
            float progress = t / duration;
            float env = Mathf.Exp(-progress * 5f);
            float wave1 = Mathf.Sin(2f * Mathf.PI * 523.25f * t); // C5
            float wave2 = Mathf.Sin(2f * Mathf.PI * 783.99f * t); // G5
            samples[i] = (wave1 * 0.5f + wave2 * 0.5f) * env;
        }

        AudioClip clip = AudioClip.Create("UI_Modal", numSamples, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    // 4. Start race energy whoosh (0.35s)
    private AudioClip CreateStartRaceClip()
    {
        int sampleRate = 44100;
        float duration = 0.35f;
        int numSamples = (int)(sampleRate * duration);
        float[] samples = new float[numSamples];

        for (int i = 0; i < numSamples; i++)
        {
            float t = (float)i / sampleRate;
            float progress = t / duration;
            // Rising pitch from 220Hz up to 880Hz
            float freq = Mathf.Lerp(220f, 880f, Mathf.Pow(progress, 1.5f));
            float env = Mathf.Sin(progress * Mathf.PI);
            float wave = Mathf.Sin(2f * Mathf.PI * freq * t);
            float engineGrit = (Mathf.Sin(2f * Mathf.PI * (freq * 0.5f) * t) > 0f ? 0.2f : -0.2f) * env;
            samples[i] = Mathf.Clamp(wave * env * 0.7f + engineGrit, -1f, 1f);
        }

        AudioClip clip = AudioClip.Create("UI_StartRace", numSamples, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }
}
