using UnityEngine;
using UnityEngine.AI;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class PatrolCat : MonoBehaviour
{
    [Header("Patrol Points (Ordenados)")]
    public Transform[] points;

    [Header("Movement")]
    public float movementSpeed = 0.9f;

    [Header("Timing")]
    public float meowChance = 0.3f;

    [Header("Interaction & Feeding")]
    public string foodTag = "Comida";
    public float eatDuration = 3.0f;
    public float waitBeforeDisappear = 3.0f; // --- NUEVO: Tiempo de espera final ---

    [Header("Animations")]
    public float walkAnimSpeed = 0.9f;

    [Header("Audio SFX")]
    public AudioClip meowSound;
    public AudioClip eatSound;
    private AudioSource audioSource;

    private NavMeshAgent agent;
    private Animator animator;

    private int currentPoint = -1;
    private bool isSitting = false;
    private bool canMove = true;
    private bool isEating = false;
    private bool isLeaving = false; // --- NUEVO: Estado para saber si se está yendo ---

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>();
        audioSource = GetComponent<AudioSource>();
    }

    void Start()
    {
        if (points.Length < 2)
        {
            Debug.LogError("Necesitas puntos para patrullar.");
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
        // --- MODIFICADO: Agregamos !isLeaving para que no interrumpa la salida
        if (!canMove || isSitting || isEating || isLeaving)
            return;

        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            CheckArrivalBehavior();
        }

        // Manejo de animaciones (común para patrulla y salida)
        HandleAnimations();
    }

    // Saqué esto a una función aparte para usarlo también en la rutina de salida
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
        if (isEating || isLeaving) return; // --- MODIFICADO: Ignorar si ya se va ---
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

        yield return new WaitForSeconds(eatDuration);

        // --- AQUÍ EMPIEZA EL CAMBIO ---
        // Ya no reiniciamos el ciclo. Iniciamos la secuencia de salida.
        StartCoroutine(LeaveAndDestroyRoutine());
    }

    // --- NUEVA RUTINA: Volver al penúltimo punto y desaparecer ---
    IEnumerator LeaveAndDestroyRoutine()
    {
        isEating = false;
        isLeaving = true; // Activamos modo salida
        isSitting = false;
        animator.SetBool("isSitting", false);

        // 1. Calcular el índice del penúltimo punto
        // points.Length - 1 es el último. points.Length - 2 es el penúltimo.
        int penultimateIndex = points.Length - 2;

        // Seguridad por si solo hay 2 puntos en total
        if (penultimateIndex < 0) penultimateIndex = 0;

        // 2. Moverse hacia allá
        agent.isStopped = false;
        agent.SetDestination(points[penultimateIndex].position);

        // Esperar mientras camina hacia el punto de salida...
        // Hacemos un bucle manual porque el Update ya no controla esto debido a 'isLeaving'
        while (agent.pathPending || agent.remainingDistance > agent.stoppingDistance)
        {
            HandleAnimations(); // Mantener la animación de caminar actualizada
            yield return null; // Esperar al siguiente frame
        }

        // 3. Ha llegado al penúltimo punto. Parar.
        agent.isStopped = true;
        animator.SetBool("isWalking", false);

        // Opcional: Que se siente o haga una pose antes de irse
        // animator.SetBool("isSitting", true); 

        // 4. Esperar unos segundos
        yield return new WaitForSeconds(waitBeforeDisappear);

        // 5. Desaparecer (Destruir el objeto)
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
                if (!isEating && !isLeaving) // Chequeo extra
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