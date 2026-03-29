using UnityEngine;
using UnityEngine.AI;

public class MoveToTarget : MonoBehaviour
{
    public Transform target;

    private NavMeshAgent agent;
    private bool warnedAboutNavMesh = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        if (!CanUseAgent()) return;

        if (target != null)
        {
            agent.SetDestination(target.position);
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
