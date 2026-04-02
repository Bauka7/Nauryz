using UnityEngine;

public class BowController : MonoBehaviour
{
    [Header("Bow Settings")]
    public GameObject arrowPrefab;
    public float drawForce = 200f; // Увеличена сила выстрела
    public float drawSensitivity = 1.5f;
    public float maxDrawTime = 1.5f;

    [Header("Arrow Visual")]
    private GameObject visualArrow;
    private Vector3 initialArrowPos;
    private float maxPullBack = 0.3f;

    [Header("Game Manager")]
    public OYURangeGameManager gameManager;

    private float currentDrawAmount = 0f;
    private bool isDrawing = false;
    private bool canShoot = true;
    private float shootCooldown = 0.5f;
    private float shootTimer = 0f;

    void Start()
    {
        if (gameManager == null)
        {
            gameManager = FindObjectOfType<OYURangeGameManager>();
        }

        CreateVisualArrow();
    }

    void Update()
    {
        if (!gameManager.isGameActive) return;

        HandleBowInput();
        UpdateShootCooldown();
        UpdateVisualArrow();
    }

    void HandleBowInput()
    {
        // Левая кнопка мыши = натягивание
        if (Input.GetMouseButton(0))
        {
            if (!isDrawing)
            {
                isDrawing = true;
                currentDrawAmount = 0f;
                Debug.Log("🏹 Начало натягивания...");
            }

            // Увеличиваем натяг
            currentDrawAmount = Mathf.Min(currentDrawAmount + Time.deltaTime * drawSensitivity, 1f);
        }

        // Отпуск мыши = выстрел
        if (Input.GetMouseButtonUp(0) && isDrawing)
        {
            isDrawing = false;
            Shoot();
        }
    }

    void Shoot()
    {
        if (!canShoot || currentDrawAmount < 0.2f) 
        {
            Debug.Log("⚠️ Не достаточно натяга для выстрела!");
            return;
        }

        Debug.Log($"🔥 ВЫСТРЕЛ! Натяг: {currentDrawAmount * 100}%");

        // Получаем позицию выстрела от лука (конец стрелы)
        Camera mainCamera = Camera.main;
        if (mainCamera == null) return;

        Vector3 shootPosition = transform.position + transform.forward * 0.3f;
        Vector3 shootDirection = mainCamera.transform.forward;
        Quaternion shootRotation = Quaternion.LookRotation(shootDirection, Vector3.up);

        // Спаунируем стрелу
        GameObject arrowObj = Instantiate(arrowPrefab, shootPosition, shootRotation);
        
        ArrowProjectile arrow = arrowObj.GetComponent<ArrowProjectile>();
        if (arrow != null)
        {
            float shotForce = drawForce * Mathf.Clamp01(currentDrawAmount);
            arrow.Launch(shootDirection, shotForce, gameManager);
            
            // Визуальный эффект - стрела исчезает из лука
            PlayShootAnimation();
        }

        // Удаляем визуальную стрелу
        if (visualArrow != null)
        {
            Destroy(visualArrow);
            visualArrow = null;
        }

        canShoot = false;
        shootTimer = shootCooldown;
        currentDrawAmount = 0f;
    }

    void CreateVisualArrow()
    {
        if (arrowPrefab == null) return;

        // Спаунируем визуальную стрелу в луке
        visualArrow = Instantiate(arrowPrefab, transform);
        visualArrow.name = "VisualArrow";
        
        // Отключаем физику
        Rigidbody rb = visualArrow.GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = true;
        
        Collider collider = visualArrow.GetComponent<Collider>();
        if (collider != null) collider.enabled = false;

        // Позиционируем в луке - прямо вперед из позиции
        visualArrow.transform.localPosition = new Vector3(0, 0, 0.2f);
        visualArrow.transform.localRotation = Quaternion.identity;
        initialArrowPos = visualArrow.transform.localPosition;
    }

    void UpdateVisualArrow()
    {
        if (visualArrow == null) return;

        if (isDrawing)
        {
            visualArrow.SetActive(true);
            
            // Анимация натягивания - стрела движется назад
            float pullBack = currentDrawAmount * maxPullBack;
            visualArrow.transform.localPosition = initialArrowPos - new Vector3(0, 0, pullBack);

            // Масштабируем немного при натягивании (визуальный эффект)
            float scale = 1f + (currentDrawAmount * 0.1f);
            visualArrow.transform.localScale = new Vector3(scale, scale, scale);
        }
        else
        {
            visualArrow.SetActive(false);
        }
    }

    void PlayShootAnimation()
    {
        // Эффект выстрела - лук "отскакивает" назад
        StartCoroutine(ShootRecoil());
    }

    System.Collections.IEnumerator ShootRecoil()
    {
        Vector3 originalPos = transform.localPosition;
        
        // Отскок назад
        transform.localPosition = originalPos - new Vector3(0, 0, 0.1f);
        
        yield return new WaitForSeconds(0.05f);
        
        // Возврат в исходное положение
        transform.localPosition = originalPos;
    }

    void UpdateShootCooldown()
    {
        if (!canShoot)
        {
            shootTimer -= Time.deltaTime;
            if (shootTimer <= 0f)
            {
                canShoot = true;
                CreateVisualArrow(); // Создаём новую стрелу для следующего выстрела
            }
        }
    }

    public void ResetBow()
    {
        isDrawing = false;
        currentDrawAmount = 0f;
        canShoot = true;

        if (visualArrow != null)
        {
            visualArrow.SetActive(false);
        }
    }
}
