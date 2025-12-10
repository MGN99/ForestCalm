using UnityEngine;
using UnityEngine.AI;

public class PatrolCat : MonoBehaviour
{
    [Header("Patrol Points (mínimo 2)")]
    public Transform[] points;

    [Header("Movement")]
    public float movementSpeed = 0.8f;

    [Header("Timing")]
    public float sitMin = 2f;
    public float sitMax = 4f;
    public float meowChance = 0.3f;

    [Header("Animations")]
    public float walkAnimSpeed = 0.8f;

    private NavMeshAgent agent;
    private Animator animator;

    private int currentPoint = -1;  // empieza sin punto
    private bool isSitting = false;
    private bool canMove = true;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>();
    }

    void Start()
    {
        if (points.Length < 2)
        {
            Debug.LogError("Debes asignar al menos 2 puntos.");
            enabled = false;
            return;
        }

        agent.speed = movementSpeed;
        agent.acceleration = 15f;
        agent.angularSpeed = 360f;
        agent.stoppingDistance = 0.08f;

        GoToNextPoint();
    }

    void Update()
    {
        if (!canMove || isSitting)
            return;

        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            StartCoroutine(SitRoutine());
        }

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

    // ------------------------------
    //   Seleccionar un punto aleatorio
    // ------------------------------
    void GoToNextPoint()
    {
        int newPoint = currentPoint;

        // garantiza que NO repita el mismo punto
        while (newPoint == currentPoint)
        {
            newPoint = Random.Range(0, points.Length);
        }

        currentPoint = newPoint;

        canMove = true;
        isSitting = false;

        agent.isStopped = false;
        agent.SetDestination(points[currentPoint].position);

        animator.SetBool("isSitting", false);
        animator.SetBool("isWalking", true);
        animator.speed = walkAnimSpeed;
    }

    // ------------------------------
    //   Rutina de sentarse + maullar
    // ------------------------------
    System.Collections.IEnumerator SitRoutine()
    {
        canMove = false;
        agent.isStopped = true;

        animator.SetBool("isWalking", false);
        animator.speed = 1f;

        yield return new WaitForSeconds(0.25f);

        animator.SetBool("isSitting", true);
        isSitting = true;

        yield return new WaitForSeconds(0.4f);

        if (Random.value < meowChance)
            animator.SetTrigger("meowTrigger");

        yield return new WaitForSeconds(Random.Range(sitMin, sitMax));

        animator.SetBool("isSitting", false);
        isSitting = false;

        yield return new WaitForSeconds(0.6f);

        GoToNextPoint();
    }
}
