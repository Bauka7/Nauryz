using UnityEngine;
using UnityEngine.AI;

public class NPCPatrol : MonoBehaviour
{
    public Transform[] patrolPoints;
    public float waitTime = 3f;

    private NavMeshAgent agent;
    private Animator animator;
    private int currentPointIndex = 0;
    private float waitTimer;
    private bool isWaiting = false;
    private bool warnedAboutNavMesh = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        if (patrolPoints.Length > 0 && patrolPoints[currentPointIndex] != null && CanUseAgent())
        {
            agent.SetDestination(patrolPoints[currentPointIndex].position);
        }
    }

    void Update()
    {
        if (!CanUseAgent()) return;
        if (patrolPoints.Length == 0) return;

        float currentSpeed = agent.velocity.magnitude;
        if (animator != null)
        {
            animator.SetFloat("Speed", currentSpeed);
        }

        if (!isWaiting && !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance + 0.1f)
        {
            isWaiting = true;
            waitTimer = waitTime;
        }

        if (!isWaiting) return;

        waitTimer -= Time.deltaTime;
        if (waitTimer <= 0f)
        {
            isWaiting = false;
            GoToNextPoint();
        }
    }

    void GoToNextPoint()
    {
        if (!CanUseAgent()) return;

        currentPointIndex++;
        if (currentPointIndex >= patrolPoints.Length)
        {
            currentPointIndex = 0;
        }

        if (patrolPoints[currentPointIndex] != null)
        {
            agent.SetDestination(patrolPoints[currentPointIndex].position);
        }
    }

    bool CanUseAgent()
    {
        if (agent == null)
        {
            WarnOnce("NavMeshAgent component not found.");
            return false;
        }

        if (!agent.isActiveAndEnabled || !agent.isOnNavMesh)
        {
            WarnOnce("NavMeshAgent is not placed on a baked NavMesh.");
            return false;
        }

        return true;
    }

    void WarnOnce(string message)
    {
        if (warnedAboutNavMesh) return;
        warnedAboutNavMesh = true;
        Debug.LogWarning($"{name}: {message}");
    }
}
