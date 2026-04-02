using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class BowSpawner : MonoBehaviour
{
    [Header("Bow Setup")]
    public GameObject bowPrefab;
    public GameObject arrowPrefab;
    public Transform handPosition;
    
    [Header("Position Settings")]
    public Vector3 positionOffset = new Vector3(0.25f, -0.08f, 0.85f); // Позиция лука в центре видимости
    public Vector3 rotationOffset = new Vector3(0f, 0f, 0f); // Ротация параллельно камере
    public Vector3 scaleOffset = new Vector3(0.65f, 0.65f, 0.65f); // Компактный размер лука

    [Header("Game Manager")]
    public OYURangeGameManager gameManager;

    private GameObject activeBow;
    private BowController bowController;

    void Start()
    {
        if (handPosition == null && Camera.main != null)
        {
            handPosition = Camera.main.transform;
        }
    }

    public void SpawnBow()
    {
        if (bowPrefab == null)
        {
            Debug.LogError("❌ Bow prefab not assigned!");
            return;
        }

        if (handPosition == null)
        {
            Debug.LogError("❌ Hand position not assigned!");
            return;
        }

        if (activeBow != null)
        {
            Destroy(activeBow);
        }

        // Спаунируем лук в руке/камере
        activeBow = Instantiate(bowPrefab, handPosition);
        activeBow.name = "ActiveBow";

        // Устанавливаем позицию, ротацию и масштаб
        activeBow.transform.localPosition = positionOffset;
        activeBow.transform.localRotation = Quaternion.Euler(rotationOffset);
        activeBow.transform.localScale = scaleOffset;

        // Делаем child камеры
        activeBow.transform.SetParent(handPosition);

        // Получаем/добавляем BowController
        bowController = activeBow.GetComponent<BowController>();
        if (bowController == null)
        {
            bowController = activeBow.AddComponent<BowController>();
        }

        bowController.gameManager = gameManager;
        bowController.arrowPrefab = arrowPrefab;

        Debug.Log("✅ Bow spawned in hand!");
    }

    public void RemoveBow()
    {
        if (activeBow != null)
        {
            Destroy(activeBow);
            activeBow = null;
            bowController = null;
        }
    }
}
