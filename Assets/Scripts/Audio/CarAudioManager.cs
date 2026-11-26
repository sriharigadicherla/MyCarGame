using UnityEngine;

/// <summary>
/// Dynamic Procedural Audio System for the Driving Game.
/// Generates real-time engine sound synthesis (frequency & harmonics scaled by RPM and throttle load),
/// tire skid friction noise, collision impacts, checkpoint chimes, and city ambience.
/// Fully self-contained with ZERO external audio asset dependencies!
/// </summary>
public class CarAudioManager : MonoBehaviour
{
    [Header("Car Controller Reference")]
    public CarController carController;

    [Header("Engine Sound Parameters")]
    public float baseFrequency = 52f;
    public float maxFrequency = 340f;
    public float engineVolume = 0.45f;

    [Header("Audio Sources")]
    public AudioSource engineAudioSource;
    public AudioSource tireSkidAudioSource;
    public AudioSource effectsAudioSource;
    public AudioSource ambientAudioSource;

    // Procedural synthesis runtime variables
    private double samplingFrequency = 48000.0;
    private double phase = 0.0;
    private double phaseHarmonic = 0.0;
    private float currentPitch = 1.0f;
    private float targetEngineVol = 0.2f;

    void Awake()
    {
        samplingFrequency = AudioSettings.outputSampleRate;
        SetupAudioSources();
    }

    void Start()
    {
        FindCarIfNull();
        CreateTireSkidClip();
        CreateAmbientLoop();
    }

    private void FindCarIfNull()
    {
        if (carController == null)
        {
            var car = GameObject.Find("SportsSedan");
            if (car != null) carController = car.GetComponent<CarController>();
        }
    }

    private void SetupAudioSources()
    {
        if (engineAudioSource == null)
        {
            engineAudioSource = gameObject.GetComponent<AudioSource>();
            if (engineAudioSource == null) engineAudioSource = gameObject.AddComponent<AudioSource>();
            engineAudioSource.playOnAwake = false;
            engineAudioSource.loop = true;
            engineAudioSource.spatialBlend = 0.0f; // 2D clean audio
            engineAudioSource.volume = engineVolume;
        }

        // Child object for Tire Skid audio (prevents conflict with OnAudioFilterRead)
        if (tireSkidAudioSource == null)
        {
            Transform t = transform.Find("TireSkidAudio");
            GameObject go = (t != null) ? t.gameObject : new GameObject("TireSkidAudio");
            go.transform.SetParent(transform, false);
            tireSkidAudioSource = go.GetComponent<AudioSource>();
            if (tireSkidAudioSource == null) tireSkidAudioSource = go.AddComponent<AudioSource>();
            tireSkidAudioSource.playOnAwake = false;
            tireSkidAudioSource.loop = true;
            tireSkidAudioSource.spatialBlend = 0.0f;
            tireSkidAudioSource.volume = 0f;
        }

        // Child object for Collision/Chime Effects audio
        if (effectsAudioSource == null)
        {
            Transform t = transform.Find("EffectsAudio");
            GameObject go = (t != null) ? t.gameObject : new GameObject("EffectsAudio");
            go.transform.SetParent(transform, false);
            effectsAudioSource = go.GetComponent<AudioSource>();
            if (effectsAudioSource == null) effectsAudioSource = go.AddComponent<AudioSource>();
            effectsAudioSource.playOnAwake = false;
            effectsAudioSource.loop = false;
            effectsAudioSource.spatialBlend = 0.0f;
            effectsAudioSource.volume = 0.7f;
        }

        // Child object for City Ambience audio
        if (ambientAudioSource == null)
        {
            Transform t = transform.Find("AmbientAudio");
            GameObject go = (t != null) ? t.gameObject : new GameObject("AmbientAudio");
            go.transform.SetParent(transform, false);
            ambientAudioSource = go.GetComponent<AudioSource>();
            if (ambientAudioSource == null) ambientAudioSource = go.AddComponent<AudioSource>();
            ambientAudioSource.playOnAwake = false;
            ambientAudioSource.loop = true;
            ambientAudioSource.spatialBlend = 0.0f;
            ambientAudioSource.volume = 0.18f;
        }
    }

    private void CreateTireSkidClip()
    {
        // Generate 1-second white noise loop for tire skidding
        int samples = (int)samplingFrequency;
        AudioClip skidClip = AudioClip.Create("Procedural_TireSkid", samples, 1, (int)samplingFrequency, false);
        float[] data = new float[samples];
        for (int i = 0; i < samples; i++)
        {
            data[i] = (Random.value * 2f - 1f) * 0.4f;
        }
        skidClip.SetData(data, 0);
        tireSkidAudioSource.clip = skidClip;
        tireSkidAudioSource.Play();
    }

    private void CreateAmbientLoop()
    {
        // Generate soothing city drive ambient drone
        int samples = (int)samplingFrequency * 3;
        AudioClip ambClip = AudioClip.Create("Procedural_CityAmbience", samples, 1, (int)samplingFrequency, false);
        float[] data = new float[samples];
        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / (float)samplingFrequency;
            float drone1 = Mathf.Sin(2f * Mathf.PI * 110f * t) * 0.08f;
            float drone2 = Mathf.Sin(2f * Mathf.PI * 220f * t) * 0.04f;
            float windNoise = (Random.value * 2f - 1f) * 0.02f;
            data[i] = drone1 + drone2 + windNoise;
        }
        ambClip.SetData(data, 0);
        ambientAudioSource.clip = ambClip;
        ambientAudioSource.Play();
    }

    void Update()
    {
        if (carController == null)
        {
            FindCarIfNull();
            return;
        }

        // 1. Calculate Engine Pitch & Volume from RPM
        float rpmRatio = carController.NormalizedRpm;
        currentPitch = Mathf.Lerp(baseFrequency, maxFrequency, rpmRatio);

        // Throttle load boost
        float load = carController.IsAccelerating ? 1.0f : 0.65f;
        targetEngineVol = Mathf.Lerp(0.25f, engineVolume * load, rpmRatio);

        // 2. Tire Skid Volume
        bool isSkidding = carController.IsHandbraking && (carController.CurrentSpeedKmh > 12f);
        float targetSkidVol = isSkidding ? 0.65f : 0.0f;
        tireSkidAudioSource.volume = Mathf.Lerp(tireSkidAudioSource.volume, targetSkidVol, Time.deltaTime * 12f);
    }

    void OnAudioFilterRead(float[] data, int channels)
    {
        // Real-time procedural multi-harmonic combustion engine wave generation
        double increment = currentPitch / samplingFrequency;
        double incrementHarmonic = (currentPitch * 1.85) / samplingFrequency;

        for (int i = 0; i < data.Length; i += channels)
        {
            phase += increment;
            phaseHarmonic += incrementHarmonic;

            if (phase > 1.0) phase -= 1.0;
            if (phaseHarmonic > 1.0) phaseHarmonic -= 1.0;

            // Combustion cylinder pop curve (asymmetric sawtooth + sine harmonics)
            float wave1 = Mathf.Sin((float)(phase * 2.0 * Mathf.PI));
            float wave2 = Mathf.Sin((float)(phaseHarmonic * 2.0 * Mathf.PI)) * 0.45f;
            float rumble = (float)((phase * 2.0) - 1.0) * 0.35f;

            float sample = (wave1 + wave2 + rumble) * targetEngineVol;

            for (int c = 0; c < channels; c++)
            {
                data[i + c] += sample;
            }
        }
    }

    public void PlayCollisionSound(float impactForce)
    {
        if (effectsAudioSource == null) return;

        // Generate dynamic impact pop
        int samples = 4800; // 0.1s
        AudioClip hitClip = AudioClip.Create("Hit", samples, 1, 48000, false);
        float[] data = new float[samples];
        for (int i = 0; i < samples; i++)
        {
            float decay = Mathf.Exp(-i / 800f);
            data[i] = (Random.value * 2f - 1f) * decay * 0.9f;
        }
        hitClip.SetData(data, 0);

        float vol = Mathf.Clamp(impactForce / 20f, 0.3f, 1.0f);
        effectsAudioSource.PlayOneShot(hitClip, vol);
    }

    public void PlayCheckpointChime()
    {
        if (effectsAudioSource == null) return;

        // Two-tone cheerful harmonic bell chime (523Hz C5 -> 659Hz E5 -> 784Hz G5)
        int sampleRate = 48000;
        int samples = sampleRate / 2; // 0.5s
        AudioClip chimeClip = AudioClip.Create("Chime", samples, 1, sampleRate, false);
        float[] data = new float[samples];

        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / sampleRate;
            float freq = (t < 0.22f) ? 587.33f : 880f; // D5 -> A5
            float decay = Mathf.Exp(-t * 6f);
            data[i] = Mathf.Sin(2f * Mathf.PI * freq * t) * decay * 0.6f;
        }
        chimeClip.SetData(data, 0);
        effectsAudioSource.PlayOneShot(chimeClip, 0.85f);
    }
}
