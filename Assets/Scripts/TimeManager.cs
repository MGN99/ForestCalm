using System.Collections;
using UnityEngine;

public class TimeManager : MonoBehaviour
{
    [Header("Skybox Textures")]
    [SerializeField] private Texture2D skyboxNight;
    [SerializeField] private Texture2D skyboxRainy;

    [Header("Cycle Durations (seconds)")]
    [SerializeField] private float nightDuration = 20f;
    [SerializeField] private float rainyDuration = 20f;

    [Header("Light Settings")]
    [SerializeField] private Light globalLight;

    [Header("Transition Settings")]
    [SerializeField] private float transitionTime = 5f;

    private enum SkyState { Night, Rainy }
    private SkyState currentState;

    private void Start()
    {
        currentState = SkyState.Night;

        // Iniciar con Night
        RenderSettings.skybox.SetTexture("_Texture1", skyboxNight);
        RenderSettings.skybox.SetTexture("_Texture2", skyboxNight);
        RenderSettings.skybox.SetFloat("_Blend", 0f);

        StartCoroutine(CycleRoutine());
    }

    private IEnumerator CycleRoutine()
    {
        while (true)
        {
            if (currentState == SkyState.Night)
            {
                yield return new WaitForSeconds(nightDuration);
                StartCoroutine(LerpSkybox(skyboxNight, skyboxRainy, transitionTime));
                currentState = SkyState.Rainy;
            }
            else if (currentState == SkyState.Rainy)
            {
                yield return new WaitForSeconds(rainyDuration);
                StartCoroutine(LerpSkybox(skyboxRainy, skyboxNight, transitionTime));
                currentState = SkyState.Night;
            }

            // Esperar transición
            yield return new WaitForSeconds(transitionTime);
        }
    }

    private IEnumerator LerpSkybox(Texture2D from, Texture2D to, float time)
    {
        RenderSettings.skybox.SetTexture("_Texture1", from);
        RenderSettings.skybox.SetTexture("_Texture2", to);
        RenderSettings.skybox.SetFloat("_Blend", 0);

        for (float t = 0; t < time; t += Time.deltaTime)
        {
            RenderSettings.skybox.SetFloat("_Blend", t / time);
            yield return null;
        }

        // Aplicar definitivamente el nuevo skybox
        RenderSettings.skybox.SetTexture("_Texture1", to);
        RenderSettings.skybox.SetFloat("_Blend", 0);
    }
}
