using UnityEngine;

/// <summary>
/// Sonidos generados por código (no necesitas archivos de audio).
/// Crea una onda senoidal corta y la reproduce con AudioSource.PlayClipAtPoint.
/// </summary>
public static class SoundFx
{
    private static AudioClip pickupClip;
    private static AudioClip beepClip;
    private static AudioClip cashClip;

    public static void Pickup() => Play(ref pickupClip, 420f, 0.09f, 0.20f);
    public static void Scan() => Play(ref beepClip, 950f, 0.07f, 0.25f);
    public static void Cash() => Play(ref cashClip, 660f, 0.22f, 0.28f);

    static void Play(ref AudioClip clip, float frequency, float duration, float volume)
    {
        if (clip == null) clip = MakeTone(frequency, duration);

        Vector3 position = Camera.main != null
            ? Camera.main.transform.position
            : Vector3.zero;

        AudioSource.PlayClipAtPoint(clip, position, volume);
    }

    static AudioClip MakeTone(float frequency, float duration)
    {
        const int sampleRate = 44100;
        int samples = Mathf.Max(1, (int)(sampleRate * duration));
        float[] data = new float[samples];

        for (int i = 0; i < samples; i++)
        {
            float t = i / (float)sampleRate;
            // Envolvente: empieza fuerte y se apaga para que no suene a "clic".
            float envelope = 1f - i / (float)samples;
            data[i] = Mathf.Sin(2f * Mathf.PI * frequency * t) * envelope * 0.6f;
        }

        AudioClip clip = AudioClip.Create("Tono", samples, 1, sampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }
}
