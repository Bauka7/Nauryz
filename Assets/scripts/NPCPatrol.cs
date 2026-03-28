using UnityEngine;
using UnityEngine.AI; // Обязательно для навигации

public class NPCPatrol : MonoBehaviour
{
    // Массив точек, по которым будет ходить NPC (перетащим их в инспекторе)
    public Transform[] patrolPoints;
    
    // Время ожидания на каждой точке (в секундах)
    public float waitTime = 3f;

    // Ссылки на компоненты
    private NavMeshAgent agent;
    private Animator animator;

    // Переменные для отслеживания текущей точки и ожидания
    private int currentPointIndex = 0;
    private float waitTimer;
    private bool isWaiting = false;

    void Start()
    {
        // Получаем ссылки на компоненты, которые висят на этом же объекте
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        // Если точки патрулирования заданы, идем к первой
        if (patrolPoints.Length > 0 && patrolPoints[currentPointIndex] != null)
        {
            agent.SetDestination(patrolPoints[currentPointIndex].position);
        }
    }

    void Update()
    {
        // Если точек нет, ничего не делаем
        if (patrolPoints.Length == 0) return;

        // 1. УПРАВЛЕНИЕ АНИМАЦИЕЙ
        // magitude - это сама скорость движения агента
        float currentSpeed = agent.velocity.magnitude;
        
        // Передаем скорость в аниматор по имени "Speed", которое мы задали в Шаге 1
        animator.SetFloat("Speed", currentSpeed);

        // 2. ПРОВЕРКА ДОСТИЖЕНИЯ ТОЧКИ И ОЖИДАНИЕ
        // remainingDistance - расстояние до цели, stoppingDistance - расстояние остановки в агенте
        if (!isWaiting && !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance + 0.1f)
        {
            // NPC дошел до точки, начинаем ждать
            isWaiting = true;
            waitTimer = waitTime;
        }

        // Если ждем, уменьшаем таймер
        if (isWaiting)
        {
            waitTimer -= Time.deltaTime;

            if (waitTimer <= 0)
            {
                // Время вышло, идем к следующей точке
                isWaiting = false;
                GoToNextPoint();
            }
        }
    }

    void GoToNextPoint()
    {
        // Увеличиваем индекс текущей точки
        currentPointIndex++;

        // Если дошли до конца списка точек, возвращаемся к началу
        if (currentPointIndex >= patrolPoints.Length)
        {
            currentPointIndex = 0;
        }

        // Задаем агенту новую точку назначения
        if (patrolPoints[currentPointIndex] != null)
        {
            agent.SetDestination(patrolPoints[currentPointIndex].position);
        }
    }
}