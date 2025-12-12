using UnityEngine;
using UnityEngine.AI;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class DogPatrol : MonoBehaviour
{
    [Header("Patrol Points (Ordenados)")]
    public Transform[] points; // El último punto es donde comerá

    [Header("Movement & Animation")]
    public float walkSpeed = 1.5f;
    public float runSpeed = 3.5f;
    public float runThreshold = 2.0f; // Velocidad mínima para activar animación de correr
    public float walkAnimSpeed = 1.0f;

    [Header("Behavior")]
    public float breatheChance = 0.3f; // Probabilidad de respirar al esperar

    [Header("Interaction & Feeding")]
    public string foodTag = "ComidaPerro"; // TAG DEL FILETE DE CARNE
    public float eatDuration = 4.0f;   // Tiempo total para Start -> Cycle -> End
    public float waitBeforeDisappear = 3.0f;

    [Tooltip("Arrastra aquí el Prefab del sistema de partículas de corazones")]
    public GameObject heartEffectPrefab;

    [Header("Audio SFX")]
    public AudioClip barkSound; // Ladrido
    public AudioClip eatSound;  // Comer
    public AudioClip xpSound;   // Efecto final
    private AudioSource audioSource;

    // Componentes
    private NavMeshAgent agent;
    private Animator animator;

    // Estados
    private int currentPoint = -1;
    private bool isWaitingForFood = false;
    private bool isEating = false;
    private bool isLeaving = false;
    private bool isBreathing = false;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>(); // Ojo: a veces es GetComponentInChildren<Animator>() si el modelo está dentro
        audioSource = GetComponent<AudioSource>();
    }

    void Start()
    {
        // Configuración inicial del NavMesh
        agent.speed = walkSpeed;
        agent.angularSpeed = 120f;
        agent.stoppingDistance = 0.1f;
        agent.autoBraking = true;

        if (points.Length > 0)
        {
            GoToNextPoint();
        }
        else
        {
            Debug.LogError("¡El perro necesita Patrol Points asignados!");
        }
    }

    void Update()
    {
        // Si está comiendo o yéndose, bloqueamos la lógica normal, PERO
        // permitimos que HandleAnimations siga funcionando para la salida.
        if (isEating) return;

        // 1. Manejo de Animaciones (Velocidad y booleanos)
        HandleAnimations();

        // 2. Lógica de Patrulla (Solo si no se está yendo ni esperando comida)
        if (!isLeaving && !isWaitingForFood && !isBreathing)
        {
            if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
            {
                CheckArrivalBehavior();
            }
        }
    }

    void HandleAnimations()
    {
        float speed = agent.velocity.magnitude;

        // Actualizamos el blend tree o parametros
        animator.SetFloat("speed", speed);

        bool isMoving = speed > 0.1f;
        bool isRunning = speed > runThreshold;

        animator.SetBool("isWalking", isMoving);
        animator.SetBool("isRunning", isRunning);
    }

    // --- LÓGICA DE LLEGADA ---
    void CheckArrivalBehavior()
    {
        // ¿Es el último punto?
        if (currentPoint == points.Length - 1)
        {
            if (!isWaitingForFood)
            {
                StartCoroutine(SitAndBarkRoutine());
            }
        }
        else
        {
            // Si no es el último, pasa al siguiente
            GoToNextPoint();
        }
    }

    // --- RUTINA DE ESPERA (Al llegar al plato) ---
    IEnumerator SitAndBarkRoutine()
    {
        isWaitingForFood = true;
        agent.isStopped = true;

        // Resetear animaciones de movimiento
        animator.SetFloat("speed", 0);
        animator.SetBool("isWalking", false);
        animator.SetBool("isRunning", false);

        // Ladrido de llegada
        PlaySound(barkSound);

        // Bucle de espera infinito (Respirar o Idle)
        while (true)
        {
            // Ocasionalmente activa la animación de respirar (Breathing)
            if (Random.value < breatheChance && !isBreathing)
            {
                StartCoroutine(BreatheRoutine());
            }

            yield return new WaitForSeconds(3f); // Chequeo cada 3 segundos
        }
    }

    IEnumerator BreatheRoutine()
    {
        isBreathing = true;
        animator.SetBool("isBreathing", true);
        yield return new WaitForSeconds(2.5f); // Duración de la respiración
        animator.SetBool("isBreathing", false);
        isBreathing = false;
    }

    // --- DETECCIÓN DE COMIDA ---
    private void OnTriggerEnter(Collider other)
    {
        if (isEating || isLeaving) return;

        // Solo come si ya llegó al destino final (está esperando)
        if (!isWaitingForFood) return;

        // Verificamos el Tag "Comida2" (Filete)
        if (other.CompareTag(foodTag))
        {
            StartCoroutine(EatRoutine(other.gameObject));
        }
    }

    // --- RUTINA DE COMER ---
    IEnumerator EatRoutine(GameObject foodObject)
    {
        isEating = true;
        isWaitingForFood = false;

        // Detener corrutinas de espera
        StopCoroutine("SitAndBarkRoutine");
        StopCoroutine("BreatheRoutine");
        animator.SetBool("isBreathing", false);

        agent.isStopped = true;
        agent.ResetPath();

        // 1. Iniciar Animación
        PlaySound(eatSound);
        animator.SetTrigger("eatTrigger");
        // Nota: Asegúrate que eatTrigger active "EatingStart" en el Animator

        yield return new WaitForSeconds(0.2f);
        if (foodObject != null) Destroy(foodObject);

        // 2. Esperar ciclo de comer
        yield return new WaitForSeconds(eatDuration);

        // 3. Efectos
        if (heartEffectPrefab != null)
        {
            // Altura ajustada para el perro (más grande que el gato)
            GameObject hearts = Instantiate(heartEffectPrefab, transform.position + Vector3.up * 1.2f, heartEffectPrefab.transform.rotation);
            PlaySound(xpSound);
            Destroy(hearts, 3.0f);
        }

        // 4. Iniciar Salida
        StartCoroutine(LeaveAndDestroyRoutine());
    }

    // --- RUTINA DE SALIDA (Retroceder y desaparecer) ---
    IEnumerator LeaveAndDestroyRoutine()
    {
        isEating = false;
        isLeaving = true; // Activa modo salida

        // Retroceder al penúltimo punto
        int exitIndex = points.Length - 2;
        if (exitIndex < 0) exitIndex = 0;

        agent.speed = runSpeed; // ¡El perro se va corriendo feliz!
        agent.isStopped = false;
        agent.SetDestination(points[exitIndex].position);

        // Esperar a llegar mientras se actualizan las animaciones en Update
        while (agent.pathPending || agent.remainingDistance > agent.stoppingDistance)
        {
            yield return null;
        }

        // Opcional: Retroceder uno más
        int exitIndex2 = points.Length - 3;
        if (exitIndex2 >= 0)
        {
            agent.SetDestination(points[exitIndex2].position);
            while (agent.pathPending || agent.remainingDistance > agent.stoppingDistance)
            {
                yield return null;
            }
        }

        // Desaparecer
        yield return new WaitForSeconds(waitBeforeDisappear);
        Destroy(gameObject);
    }

    void GoToNextPoint()
    {
        currentPoint++;

        // Seguridad por si se acaban los puntos
        if (currentPoint >= points.Length) currentPoint = 0;

        // Decisión aleatoria: ¿Camina o corre hacia el siguiente punto?
        bool run = Random.value > 0.5f;
        agent.speed = run ? runSpeed : walkSpeed;

        agent.isStopped = false;
        agent.SetDestination(points[currentPoint].position);
    }

    void PlaySound(AudioClip clip)
    {
        if (clip != null && audioSource != null) audioSource.PlayOneShot(clip);
    }
}