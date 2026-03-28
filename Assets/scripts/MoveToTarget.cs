using UnityEngine;
using UnityEngine.AI; // Обязательно подключаем библиотеку ИИ для навигации

public class MoveToTarget : MonoBehaviour
{
    public Transform target; // Объект, к которому мы пойдем
    private NavMeshAgent agent;

    void Start()
    {
        // Получаем компонент агента при старте игры
        agent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        // Если цель назначена, постоянно обновляем маршрут к ней
        if (target != null)
        {
            agent.SetDestination(target.position);
        }
    }
}