using UnityEngine;
using System.Collections;

public class FireAutoOff : MonoBehaviour
{
    [Header("Fire Components")]
    [SerializeField] private ParticleSystem fire;
    [SerializeField] private float timeToOff = 30f;

    [Header("Light Component")]
    [Tooltip("Luz tipo Point Light asociada al fuego.")]
    [SerializeField] private Light pointLight;
    [SerializeField] private float lightOffDelay = 30f;

    private AudioSource fireAudio;

    private void Awake()
    {
        // Obtiene el AudioSource del mismo GameObject (si existe)
        fireAudio = GetComponent<AudioSource>();
    }

    private void Start()
    {
        StartCoroutine(StopFireCoroutine());
        StartCoroutine(StopLightCoroutine());
    }

    private IEnumerator StopFireCoroutine()
    {
        yield return new WaitForSeconds(timeToOff);

        // Apagar partículas
        if (fire != null)
            fire.Stop();

        // Apagar audio
        if (fireAudio != null)
            fireAudio.Stop();
    }

    private IEnumerator StopLightCoroutine()
    {
        yield return new WaitForSeconds(lightOffDelay);

        // Apagar luz
        if (pointLight != null)
            pointLight.enabled = false;
    }
}
