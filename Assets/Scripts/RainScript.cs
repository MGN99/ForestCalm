using UnityEngine;

public class RainScript : MonoBehaviour
{
    [Header("Rain Components")]
    [SerializeField] private ParticleSystem rainParticles;

    private AudioSource rainAudio;

    [Header("Rain Timeline")]
    [Tooltip("Tiempo en segundos cuando debe iniciar la lluvia (180 por defecto).")]
    [SerializeField] private float rainStartTime = 180f;

    [Tooltip("Tiempo en segundos cuando debe detenerse la lluvia (235 por defecto).")]
    [SerializeField] private float rainStopTime = 235f;

    private bool rainStarted = false;
    private bool rainStopped = false;

    private float globalTimer = 0f;

    private void Awake()
    {
        if (rainParticles != null)
        {
            rainAudio = rainParticles.GetComponent<AudioSource>();

            if (rainAudio == null)
            {
                Debug.LogWarning("RainScript: El GameObject del ParticleSystem no tiene un AudioSource.");
            }
        }
        else
        {
            Debug.LogError("RainScript: No se asignó un ParticleSystem en el inspector.");
        }
    }

    private void Start()
    {
        StopRainImmediate();  // aseguramos que empieza sin lluvia
    }

    private void Update()
    {
        globalTimer += Time.deltaTime;

        // --- Iniciar lluvia ---
        if (!rainStarted && globalTimer >= rainStartTime)
        {
            StartRain();
        }

        // --- Detener lluvia ---
        if (!rainStopped && globalTimer >= rainStopTime)
        {
            StopRain();
        }
    }

    // ---------------- INTERNAL METHODS ----------------

    private void StartRain()
    {
        rainStarted = true;

        if (rainParticles != null)
            rainParticles.Play();

        if (rainAudio != null)
            rainAudio.Play();
    }

    private void StopRain()
    {
        rainStopped = true;

        if (rainParticles != null)
            rainParticles.Stop();

        if (rainAudio != null)
            rainAudio.Stop();
    }

    private void StopRainImmediate()
    {
        if (rainParticles != null)
            rainParticles.Stop();

        if (rainAudio != null)
        {
            rainAudio.Stop();
            rainAudio.time = 0f;
        }

        rainStarted = false;
        rainStopped = false;
        globalTimer = 0f;
    }
}
