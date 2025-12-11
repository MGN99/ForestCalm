using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class DogPatrolFixed : MonoBehaviour
{
    [Header("Patrol Points")]
    public Transform[] points;
    
    [Header("Speed Thresholds")]
    public float idleThreshold = 0.1f;
    public float walkThreshold = 0.5f;
    public float runThreshold = 1.5f;
    
    [Header("Movement")]
    public float runSpeed = 4.0f;
    public float walkSpeed = 2.0f;
    
    [Header("Behavior")]
    public float breatheChance = 0.3f;
    public float minWaitTime = 1f;
    public float maxWaitTime = 3f;

    // Componentes
    private NavMeshAgent agent;
    private Animator animator;
    
    // Estado
    private int currentPoint = 0;
    private bool isWaiting = false;
    private bool isBreathing = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        
        // Configuración básica
        agent.speed = walkSpeed;
        agent.angularSpeed = 120f;
        agent.stoppingDistance = 0.1f;
        agent.autoBraking = true;
        
        if (points.Length > 0)
        {
            GoToNextPoint();
        }
    }

    void Update()
    {
        // Obtener velocidad actual
        float currentSpeed = agent.velocity.magnitude;
        
        // ACTUALIZAR TODOS LOS PARÁMETROS DEL ANIMATOR
        UpdateAnimatorParameters(currentSpeed);
        
        // Lógica de patrullaje
        if (!isWaiting && !isBreathing && points.Length > 0)
        {
            if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
            {
                StartCoroutine(WaitAtPoint());
            }
        }
    }

    void UpdateAnimatorParameters(float speed)
    {
        // 1. Parámetro "speed" (float)
        animator.SetFloat("speed", speed);
        
        // 2. Parámetro "isWalking" (bool)
        bool isWalking = speed > idleThreshold && speed <= runThreshold;
        animator.SetBool("isWalking", isWalking);
        
        // 3. Parámetro "isRunning" (bool) - NUEVO
        bool isRunning = speed > runThreshold;
        animator.SetBool("isRunning", isRunning);
        
        // 4. Parámetro "isBreathing" (bool)
        animator.SetBool("isBreathing", isBreathing);
        
        // Debug en consola para verificar
        Debug.Log($"Speed: {speed:F2} | Walking: {isWalking} | Running: {isRunning} | Breathing: {isBreathing}");
    }

    void GoToNextPoint()
    {
        if (points.Length == 0) return;
        
        // Seleccionar punto aleatorio
        int newPoint;
        do
        {
            newPoint = Random.Range(0, points.Length);
        } while (newPoint == currentPoint && points.Length > 1);
        
        currentPoint = newPoint;
        
        // Decidir velocidad
        bool shouldRun = Random.Range(0f, 1f) > 0.5f;
        agent.speed = shouldRun ? runSpeed : walkSpeed;
        
        // Ir al punto
        agent.isStopped = false;
        agent.SetDestination(points[currentPoint].position);
        
        isWaiting = false;
        isBreathing = false;
    }

    IEnumerator WaitAtPoint()
    {
        isWaiting = true;
        agent.isStopped = true;
        
        // Resetear parámetros de movimiento
        animator.SetFloat("speed", 0f);
        animator.SetBool("isWalking", false);
        animator.SetBool("isRunning", false);
        
        // Decidir si respirar
        if (Random.Range(0f, 1f) < breatheChance)
        {
            yield return StartCoroutine(Breathe());
        }
        else
        {
            // Esperar normal
            float waitTime = Random.Range(minWaitTime, maxWaitTime);
            yield return new WaitForSeconds(waitTime);
        }
        
        // Continuar
        GoToNextPoint();
    }

    IEnumerator Breathe()
    {
        isBreathing = true;
        animator.SetBool("isBreathing", true);
        
        Debug.Log("Perro respira...");
        
        yield return new WaitForSeconds(2f);
        
        isBreathing = false;
        animator.SetBool("isBreathing", false);
    }
    
    // Método para verificar parámetros
    bool HasParameter(string paramName)
    {
        foreach (AnimatorControllerParameter param in animator.parameters)
        {
            if (param.name == paramName) return true;
        }
        return false;
    }
}