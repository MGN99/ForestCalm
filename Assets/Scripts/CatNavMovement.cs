using UnityEngine;
using UnityEngine.AI;

public class CatNavMovement : MonoBehaviour
{
    private Animator animator;
    private NavMeshAgent agent;
    
    public Transform[] waypoints;
    private int currentWaypoint = 0;

    void Start()
    {
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        
        if (waypoints.Length > 0)
        {
            MoveToNextWaypoint();
        }
    }

    void Update()
    {
        // Sincronizar animación con movimiento
        if (animator != null)
        {
            bool isMoving = agent.velocity.magnitude > 0.1f;
            animator.SetBool("isWalking", isMoving);
        }
        
        // Verificar si llegó al destino
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            Invoke("MoveToNextWaypoint", 2f);
        }
    }

    void MoveToNextWaypoint()
    {
        if (waypoints.Length > 0)
        {
            currentWaypoint = (currentWaypoint + 1) % waypoints.Length;
            agent.SetDestination(waypoints[currentWaypoint].position);
        }
    }
}