using UnityEngine;
using System.Collections;

public class FireAutoOff : MonoBehaviour
{
    public ParticleSystem fire;
    public float timeToOff = 30f;

    private AudioSource fireAudio;

    void Awake()
    {
        // Busca el AudioSource en el mismo GameObject
        fireAudio = GetComponent<AudioSource>();
    }

    void Start()
    {
        StartCoroutine(StopFire());
    }

    IEnumerator StopFire()
    {
        yield return new WaitForSeconds(timeToOff);

        // Apagar partículas
        if (fire != null)
            fire.Stop();

        // Apagar audio
        if (fireAudio != null)
            fireAudio.Stop();
    }
}
