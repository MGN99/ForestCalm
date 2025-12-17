using System.Collections;
using UnityEngine;

public class TimeManagerPrincipal : MonoBehaviour
{
    [Header("Configuración del Tiempo")]
    [Tooltip("Hora de inicio (ej: 18 para atardecer)")]
    [SerializeField] private int startHour = 16; // Empezar a las 4 PM

    [Tooltip("Cuánto tarda 1 minuto del juego en segundos reales")]
    [SerializeField] private float realSecondsPerGameMinute = 0.5f; //Control de velocidad

    [Header("Skybox Textures")]
    [SerializeField] private Texture2D skyboxNight;
    [SerializeField] private Texture2D skyboxSunrise;
    [SerializeField] private Texture2D skyboxDay;
    [SerializeField] private Texture2D skyboxSunset;

    [Header("Gradients")]
    [SerializeField] private Gradient graddientNightToSunrise;
    [SerializeField] private Gradient graddientSunriseToDay;
    [SerializeField] private Gradient graddientDayToSunset;
    [SerializeField] private Gradient graddientSunsetToNight;

    [Header("Light Settings")]
    [SerializeField] private Light globalLight;

    private int minutes;
    public int Minutes
    { get { return minutes; } set { minutes = value; OnMinutesChange(value); } }

    private int hours;
    public int Hours
    { get { return hours; } set { hours = value; OnHoursChange(value); } }

    private int days;
    public int Days
    { get { return days; } set { days = value; } }

    private float tempSecond;
    private bool stopTime;

    private void Start()
    {
        Days = 0;
        Hours = startHour; // Hora configurable
        Minutes = 0;
        tempSecond = 0;
        stopTime = false;

        // Configuración inicial del Skybox según la hora de inicio
        SetupInitialSkybox();

        // Rotación inicial del sol correcta según la hora
        // 360 grados / 24 horas = 15 grados por hora.
        float initialRotation = (startHour * 15f) - 90f; // -90 offset común para que salga por el este
        globalLight.transform.rotation = Quaternion.Euler(0f, initialRotation, 0f);
    }

    public void Update()
    {
        if (stopTime) return;
        tempSecond += Time.deltaTime;

        //variable de velocidad en lugar de "1 segundo fijo"
        if (tempSecond >= realSecondsPerGameMinute)
        {
            Minutes += 1;
            tempSecond = 0;
        }
    }

    // aspecto visual inicial para no empezar con cielo de noche si son las 18:00
    private void SetupInitialSkybox()
    {
        if (Hours >= 5 && Hours < 8) // Amanecer
        {
            RenderSettings.skybox.SetTexture("_Texture1", skyboxNight);
            RenderSettings.skybox.SetTexture("_Texture2", skyboxSunrise);
            RenderSettings.skybox.SetFloat("_Blend", 0.5f); // Mezcla media
        }
        else if (Hours >= 8 && Hours < 18) // Día
        {
            RenderSettings.skybox.SetTexture("_Texture1", skyboxDay);
            RenderSettings.skybox.SetTexture("_Texture2", skyboxDay);
            RenderSettings.skybox.SetFloat("_Blend", 0f);
        }
        else if (Hours >= 18 && Hours < 22) // Atardecer
        {
            RenderSettings.skybox.SetTexture("_Texture1", skyboxSunset); // Empezamos directo con Sunset
            RenderSettings.skybox.SetTexture("_Texture2", skyboxSunset);
            RenderSettings.skybox.SetFloat("_Blend", 0f);

            // Forzar color de luz de atardecer
            if (graddientDayToSunset != null)
                globalLight.color = graddientDayToSunset.Evaluate(0.5f);
        }
        else // Noche
        {
            RenderSettings.skybox.SetTexture("_Texture1", skyboxNight);
            RenderSettings.skybox.SetTexture("_Texture2", skyboxNight);
            RenderSettings.skybox.SetFloat("_Blend", 0f);
        }
    }

    private void OnMinutesChange(int value)
    {
        // Rotación continua del sol
        globalLight.transform.Rotate(Vector3.up, (1f / 4f) * 360f / 1440f, Space.World); // Ajuste fino a la rotación
 
        // simplificado: 360 grados / 1440 minutos = 0.25 grados por minuto.
        globalLight.transform.Rotate(Vector3.up, 0.25f, Space.World);

        if (value >= 60)
        {
            Hours++;
            minutes = 0;
        }

        // demo de 40s no necesita más precisión
    }

    private void OnHoursChange(int value)
    {
        // Si llegamos a las 23:00 (11 PM), ya es noche cerrada. Detenemos todo.
        if (value >= 23)
        {
            stopTime = true; // Esto detiene el Update
            return;          // Salimos de la función para no reiniciar ciclo
        }

        // Reinicio de ciclo diario (ya no se usa en demo)
        if (value >= 24)
        {
            Hours = 0;
            Days++;
            value = 0; // Para que los ifs de abajo funcionen cuando pasa de 23 a 0
        }

        if (value == 6)
        {
            StartCoroutine(LerpSkybox(skyboxNight, skyboxSunrise, 30f));
            StartCoroutine(LerpLight(graddientNightToSunrise, 30f));
        }
        else if (value == 8)
        {
            StartCoroutine(LerpSkybox(skyboxSunrise, skyboxDay, 30f));
            StartCoroutine(LerpLight(graddientSunriseToDay, 30f));
        }
        else if (value == 18) // ATARDECER
        {
            StartCoroutine(LerpSkybox(skyboxDay, skyboxSunset, 40f));
            StartCoroutine(LerpLight(graddientDayToSunset, 40f));
        }
        else if (value == 22) // NOCHE
        {
            StartCoroutine(LerpSkybox(skyboxSunset, skyboxNight, 60f));
            StartCoroutine(LerpLight(graddientSunsetToNight, 60f));
        }
    }

    private IEnumerator LerpSkybox(Texture2D a, Texture2D b, float time)
    {
        RenderSettings.skybox.SetTexture("_Texture1", a);
        RenderSettings.skybox.SetTexture("_Texture2", b);
        RenderSettings.skybox.SetFloat("_Blend", 0);
        for (float i = 0; i < time; i += Time.deltaTime)
        {
            RenderSettings.skybox.SetFloat("_Blend", i / time);
            yield return null;
        }
        RenderSettings.skybox.SetTexture("_Texture1", b);
        RenderSettings.skybox.SetFloat("_Blend", 0); // Reset blend para asegurar
    }

    private IEnumerator LerpLight(Gradient lightGradient, float time)
    {
        for (float i = 0; i < time; i += Time.deltaTime)
        {
            globalLight.color = lightGradient.Evaluate(i / time);
            RenderSettings.fogColor = globalLight.color;
            yield return null;
        }
    }
}