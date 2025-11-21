using UnityEngine;
using UnityEngine.AI;

public class PatrolCat : MonoBehaviour
{
    [Header("Patrol Points")]
    public Transform[] points;
    
    [Header("Patrol Settings")]
    public float waitTimeAtPoint = 2f;
    public float movementSpeed = 1.5f;
    
    [Header("Cat Behavior")]
    public float sitChance = 0.3f;
    public float meowChance = 0.2f;
    
    private NavMeshAgent agent;
    private Animator animator;
    private int destPoint = 0;
    private bool isWaiting = false;
    private bool isSitting = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        
        agent.autoBraking = false;
        agent.speed = movementSpeed;
        
        // Verificar que el Animator tiene los parámetros necesarios
        CheckAnimatorParameters();
        
        GoToNextPoint();
    }

    void CheckAnimatorParameters()
    {
        // Verificar parámetros críticos
        if (!HasParameter("isWalking", animator))
        {
            Debug.LogError("Parámetro 'isWalking' no encontrado en el Animator!");
        }
        if (!HasParameter("isSitting", animator))
        {
            Debug.LogError("Parámetro 'isSitting' no encontrado en el Animator!");
        }
        if (!HasParameter("meow", animator))
        {
            Debug.LogError("Parámetro 'meow' no encontrado en el Animator!");
        }
    }

    // Método para verificar si un parámetro existe
    bool HasParameter(string paramName, Animator animator)
    {
        foreach (AnimatorControllerParameter param in animator.parameters)
        {
            if (param.name == paramName) return true;
        }
        return false;
    }

    void GoToNextPoint()
    {
        if (points.Length == 0)
        {
            Debug.LogWarning("No hay Patrol Points asignados!");
            SetIdle();
            return;
        }
        
        float randomAction = Random.Range(0f, 1f);
        
        if (isSitting)
        {
            StandUp();
            Invoke("GoToNextPoint", 1f);
            return;
        }
        else if (randomAction < meowChance && HasParameter("meow", animator))
        {
            Meow();
            Invoke("ContinuePatrol", 2f);
            return;
        }
        else if (randomAction < meowChance + sitChance && HasParameter("isSitting", animator))
        {
            SitDown();
            return;
        }
        else
        {
            ContinuePatrol();
        }
    }

    void ContinuePatrol()
    {
        if (points.Length == 0) return;
        
        SetWalking();
        agent.destination = points[destPoint].position;
        destPoint = (destPoint + 1) % points.Length;
        isWaiting = false;
    }

    void Update()
    {
        // Actualizar animación de caminar solo si el parámetro existe
        if (animator != null && HasParameter("isWalking", animator))
        {
            bool isMoving = agent.velocity.magnitude > 0.1f;
            animator.SetBool("isWalking", isMoving);
        }
        
        if (!isWaiting && !isSitting && !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            isWaiting = true;
            SetIdle();
            Invoke("GoToNextPoint", waitTimeAtPoint);
        }
    }

    void SetWalking()
    {
        if (HasParameter("isWalking", animator))
            animator.SetBool("isWalking", true);
        if (HasParameter("isSitting", animator))
            animator.SetBool("isSitting", false);
    }

    void SetIdle()
    {
        if (HasParameter("isWalking", animator))
            animator.SetBool("isWalking", false);
        if (HasParameter("isSitting", animator))
            animator.SetBool("isSitting", false);
    }

    void SitDown()
    {
        isSitting = true;
        if (HasParameter("isWalking", animator))
            animator.SetBool("isWalking", false);
        if (HasParameter("isSitting", animator))
            animator.SetBool("isSitting", true);
        
        Invoke("StandUp", Random.Range(3f, 6f));
    }

    void StandUp()
    {
        isSitting = false;
        if (HasParameter("isSitting", animator))
            animator.SetBool("isSitting", false);
        
        Invoke("GoToNextPoint", 1f);
    }

    void Meow()
    {
        if (HasParameter("meow", animator))
            animator.SetTrigger("meow");
        SetIdle();
    }
    
    public void AddPoint(Transform newPoint)
    {
        Transform[] newPoints = new Transform[points.Length + 1];
        for (int i = 0; i < points.Length; i++)
        {
            newPoints[i] = points[i];
        }
        newPoints[points.Length] = newPoint;
        points = newPoints;
    }

    void OnDrawGizmosSelected()
    {
        if (points != null && points.Length > 1)
        {
            Gizmos.color = Color.blue;
            for (int i = 0; i < points.Length; i++)
            {
                if (points[i] != null)
                {
                    int next = (i + 1) % points.Length;
                    if (points[next] != null)
                    {
                        Gizmos.DrawLine(points[i].position, points[next].position);
                        Gizmos.DrawWireSphere(points[i].position, 0.3f);
                    }
                }
            }
        }
    }
}