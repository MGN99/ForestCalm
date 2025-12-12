using UnityEngine;
using UnityEngine.AI;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class PatrolCat : MonoBehaviour
{
    [Header("Secuencia")]
    public GameObject dogObject;

    [Header("Patrol Points (Ordenados)")]
    public Transform[] points;

    [Header("Movement")]
    public float movementSpeed = 0.9f;

    [Header("Timing")]
    public float meowChance = 0.3f;

    [Header("Interaction & Feeding")]
    public string foodTag = "Comida";
    public float eatDuration = 3.0f;
    public float waitBeforeDisappear = 3.0f;
    // --- NUEVO: Variable para el efecto de corazones ---
    [Tooltip("Arrastra aquí el Prefab del sistema de partículas de corazones")]
    public GameObject heartEffectPrefab;

    [Header("Animations")]
    public float walkAnimSpeed = 0.9f;

    [Header("Audio SFX")]
    public AudioClip meowSound;
    public AudioClip eatSound;
    public AudioClip xpSound;
    private AudioSource audioSource;

    private NavMeshAgent agent;
    private Animator animator;

    private int currentPoint = -1;
    private bool isSitting = false;
    private bool canMove = true;
    private bool isEating = false;
    private bool isLeaving = false;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>();
        audioSource = GetComponent<AudioSource>();
    }

    void Start()
    {
        if (points.Length < 3)
        {
            Debug.LogWarning("Se recomiendan al menos 3 puntos para la lógica de retroceso.");
        }

        if (points.Length < 2)
        {
            Debug.LogError("Necesitas al menos 2 puntos para patrullar.");
            enabled = false;
            return;
        }

        agent.speed = movementSpeed;
        agent.acceleration = 15f;
        agent.stoppingDistance = 0.1f;

        GoToNextPoint();
    }

    void Update()
    {
        if (!canMove || isSitting || isEating || isLeaving)
            return;

        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            CheckArrivalBehavior();
        }

        HandleAnimations();
    }

    void HandleAnimations()
    {
        float vel = agent.velocity.magnitude;
        bool walking = vel > 0.03f;
        animator.SetBool("isWalking", walking);
        animator.speed = walking ? walkAnimSpeed : 1f;

        if (agent.velocity.sqrMagnitude > 0.01f)
        {
            Quaternion targetRot = Quaternion.LookRotation(agent.velocity.normalized);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 5f);
        }
    }

    void CheckArrivalBehavior()
    {
        if (currentPoint == points.Length - 1)
        {
            if (!isSitting)
            {
                StartCoroutine(SitAndWaitForFood());
            }
        }
        else
        {
            GoToNextPoint();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isEating || isLeaving) return;
        if (!isSitting) return;

        if (other.CompareTag(foodTag))
        {
            StartCoroutine(EatRoutine(other.gameObject));
        }
    }

    IEnumerator EatRoutine(GameObject foodObject)
    {
        isEating = true;
        canMove = false;

        StopCoroutine("SitAndWaitForFood");

        agent.isStopped = true;
        agent.ResetPath();

        animator.SetBool("isWalking", false);

        PlaySound(eatSound);
        animator.SetTrigger("eatTrigger");

        yield return new WaitForSeconds(0.2f);

        if (foodObject != null) Destroy(foodObject);

        // Esperar a que termine de masticar
        yield return new WaitForSeconds(eatDuration);

        // --- NUEVO: LANZAR EFECTO DE CORAZONES ---
        if (heartEffectPrefab != null)
        {
            // Creamos el efecto en la posición del gato, un poco más arriba (Vector3.up * 0.5f)
            // y con la misma rotación que el gato.
            GameObject hearts = Instantiate(heartEffectPrefab, transform.position + Vector3.up * 1.0f, heartEffectPrefab.transform.rotation);
            PlaySound(xpSound);

            // Importante: Destruir el efecto después de unos segundos para que no llene la escena
            Destroy(hearts, 3.0f);
        }

        // --- ACTIVAR AL PERRO ---
        if (dogObject != null)
        {
            dogObject.SetActive(true); // ¡El perro aparece y empieza su script!
        }

        StartCoroutine(LeaveAndDestroyRoutine());
    }

    IEnumerator LeaveAndDestroyRoutine()
    {
        isEating = false;
        isLeaving = true;
        isSitting = false;
        animator.SetBool("isSitting", false);

        int firstBackIndex = points.Length - 2;

        if (firstBackIndex >= 0)
        {
            agent.isStopped = false;
            agent.SetDestination(points[firstBackIndex].position);

            while (agent.pathPending || agent.remainingDistance > agent.stoppingDistance)
            {
                HandleAnimations();
                yield return null;
            }
        }

        int secondBackIndex = points.Length - 3;

        if (secondBackIndex >= 0)
        {
            agent.SetDestination(points[secondBackIndex].position);

            while (agent.pathPending || agent.remainingDistance > agent.stoppingDistance)
            {
                HandleAnimations();
                yield return null;
            }
        }

        agent.isStopped = true;
        animator.SetBool("isWalking", false);

        yield return new WaitForSeconds(waitBeforeDisappear);

        Destroy(gameObject);
    }

    void GoToNextPoint()
    {
        if (agent == null) return;

        currentPoint++;

        if (currentPoint >= points.Length)
        {
            currentPoint = 0;
        }

        canMove = true;
        isSitting = false;
        isEating = false;

        agent.isStopped = false;
        agent.SetDestination(points[currentPoint].position);

        animator.SetBool("isSitting", false);
        animator.SetBool("isWalking", true);
    }

    IEnumerator SitAndWaitForFood()
    {
        canMove = false;
        agent.isStopped = true;
        isSitting = true;

        animator.SetBool("isWalking", false);
        animator.SetBool("isSitting", true);

        if (Random.value < meowChance)
        {
            animator.SetTrigger("meowTrigger");
            PlaySound(meowSound);
        }

        while (true)
        {
            float tiempoEspera = Random.Range(3.0f, 6.0f);
            yield return new WaitForSeconds(tiempoEspera);

            if (Random.value < 0.3f)
            {
                if (!isEating && !isLeaving)
                {
                    animator.SetTrigger("meowTrigger");
                    PlaySound(meowSound);
                }
            }
        }
    }

    void PlaySound(AudioClip clip)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }
}