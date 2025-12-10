using UnityEngine;
using UnityEngine.AI;

public class DogPatrol : MonoBehaviour
{
    [Header("Patrol Points (movimiento aleatorio)")]
    public Transform[] points;

    [Header("Movement")]
    public float runSpeed = 3.5f;
    public float walkSpeed = 1.5f;
    public float rotationSpeed = 2.5f;

    [Header("Breathing Settings")]
    public float breathMin = 2f;
    public float breathMax = 4f;

    private NavMeshAgent agent;
    private Animator animator;

    private int currentPoint = -1;   // Ningún punto inicial
    private int nextPoint = -1;

    private bool isBreathing = false;
    private bool isRotating = false;
    private Quaternion targetRotation;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>();
    }

    void Start()
    {
        if (points.Length < 2)
        {
            Debug.LogError("Debes asignar al menos 2 puntos para patrullar aleatoriamente.");
            enabled = false;
            return;
        }

        agent.speed = runSpeed;
        agent.acceleration = 20f;
        agent.angularSpeed = 0f;   // El perro gira manualmente
        agent.stoppingDistance = 0.15f;

        PickRandomNextPoint();
        GoToNextPoint();
    }

    void Update()
    {
        if (isBreathing || isRotating)
        {
            if (isRotating)
                RotateTowardsNextPoint();
            return;
        }

        float vel = agent.velocity.magnitude;
        animator.SetFloat("speed", vel);

        // Llegó al punto
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            StartCoroutine(BreathRoutine());
        }

        // Rotación suave mientras corre
        if (vel > 0.1f)
        {
            Quaternion targetRot = Quaternion.LookRotation(agent.velocity.normalized);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRot,
                Time.deltaTime * 5f
            );
        }
    }

    // Genera un destino aleatorio distinto del actual
    void PickRandomNextPoint()
    {
        if (points.Length <= 1)
            return;

        int randomIndex;
        do
        {
            randomIndex = Random.Range(0, points.Length);
        }
        while (randomIndex == currentPoint);

        nextPoint = randomIndex;
    }

    void GoToNextPoint()
    {
        currentPoint = nextPoint;
        isBreathing = false;
        isRotating = false;

        agent.isStopped = false;
        agent.speed = runSpeed;

        animator.SetBool("isBreathing", false);
        animator.SetBool("isWalking", false);

        agent.SetDestination(points[currentPoint].position);

        // Elegir nuevo punto aleatorio para después
        PickRandomNextPoint();
    }

    // Rutina de respiración (idle)
    System.Collections.IEnumerator BreathRoutine()
    {
        isBreathing = true;
        agent.isStopped = true;

        animator.SetFloat("speed", 0f);
        animator.SetBool("isWalking", false);

        yield return new WaitForSeconds(0.25f);

        animator.SetBool("isBreathing", true);

        yield return new WaitForSeconds(Random.Range(breathMin, breathMax));

        animator.SetBool("isBreathing", false);

        yield return new WaitForSeconds(0.2f);

        StartRotatingToNextPoint();
    }

    // Comienza a girar hacia el punto aleatorio
    void StartRotatingToNextPoint()
    {
        isRotating = true;
        agent.isStopped = true;

        // Cambia a caminar
        animator.SetBool("isWalking", true);
        agent.speed = walkSpeed;

        // Rotación hacia el siguiente punto aleatorio
        Vector3 dir = (points[nextPoint].position - transform.position).normalized;
        targetRotation = Quaternion.LookRotation(dir, Vector3.up);
    }

    void RotateTowardsNextPoint()
    {
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            Time.deltaTime * rotationSpeed
        );

        float angle = Quaternion.Angle(transform.rotation, targetRotation);

        // Terminó de rotar → correr al siguiente punto aleatorio
        if (angle < 2f)
        {
            isRotating = false;
            animator.SetBool("isWalking", false);
            GoToNextPoint();
        }
    }
}
