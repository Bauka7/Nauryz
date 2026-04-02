using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

public class SetupBowAndArrowPrefabs : MonoBehaviour
{
#if UNITY_EDITOR
    [ContextMenu("🏹 Setup Bow & Arrow Prefabs")]
    public void SetupPrefabs()
    {
        Debug.Log("\n╔════════════════════════════════════════╗\n" +
                  "║  🏹 SETTING UP BOW & ARROW PREFABS   ║\n" +
                  "╚════════════════════════════════════════╝\n");

        try
        {
            // Шаг 1: Переделаем Arrow.prefab
            SetupArrowPrefab();

            // Шаг 2: Переделаем Wooden Bow.prefab
            SetupBowPrefab();

            Debug.Log("\n╔════════════════════════════════════════╗\n" +
                      "║  ✅ PREFABS SETUP COMPLETE!           ║\n" +
                      "╚════════════════════════════════════════╝\n");

            EditorUtility.DisplayDialog("✅ Success",
                "✅ Arrow.prefab и Wooden Bow.prefab переделаны!\n\n" +
                "Оба префаба готовы к использованию.",
                "OK");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"❌ Error: {ex.Message}");
            EditorUtility.DisplayDialog("❌ Error", $"Ошибка: {ex.Message}", "OK");
        }
    }

    void SetupArrowPrefab()
    {
        Debug.Log("[1/2] 🔧 Настройка Arrow.prefab...");

        string prefabPath = "Assets/Free medieval weapons/Prefabs/Arrow.prefab";
        GameObject arrowPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

        if (arrowPrefab == null)
        {
            Debug.LogError("❌ Arrow.prefab не найден!");
            return;
        }

        // Открываем префаб для редактирования
        GameObject prefabInstance = PrefabUtility.LoadPrefabContents(prefabPath);

        // Очищаем от старых компонентов
        foreach (Transform child in prefabInstance.transform)
        {
            DestroyImmediate(child.gameObject);
        }

        // Загружаем модели
        GameObject arrowStick = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Free medieval weapons/Models/Arrow_stick.fbx");
        GameObject arrowHead = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Free medieval weapons/Models/arrowhead.fbx");

        if (arrowStick != null)
        {
            GameObject stick = PrefabUtility.InstantiatePrefab(arrowStick, prefabInstance.transform) as GameObject;
            stick.name = "Arrow_stick";
            stick.transform.localPosition = Vector3.zero;
            stick.transform.localRotation = Quaternion.identity;
            Debug.Log("  ✅ Arrow_stick добавлена");
        }

        if (arrowHead != null)
        {
            GameObject head = PrefabUtility.InstantiatePrefab(arrowHead, prefabInstance.transform) as GameObject;
            head.name = "arrowhead";
            head.transform.localPosition = new Vector3(0, 0, 0.8f); // Вперёд
            head.transform.localRotation = Quaternion.identity;
            Debug.Log("  ✅ arrowhead добавлена");
        }

        // Добавляем компоненты если их нет
        if (prefabInstance.GetComponent<Rigidbody>() == null)
        {
            Rigidbody rb = prefabInstance.AddComponent<Rigidbody>();
            rb.mass = 0.1f;
            rb.useGravity = true;
            rb.linearDamping = 0.1f;
            Debug.Log("  ✅ Rigidbody добавлена");
        }

        if (prefabInstance.GetComponent<Collider>() == null)
        {
            CapsuleCollider collider = prefabInstance.AddComponent<CapsuleCollider>();
            collider.isTrigger = true;
            Debug.Log("  ✅ CapsuleCollider добавлена");
        }

        if (prefabInstance.GetComponent<ArrowProjectile>() == null)
        {
            prefabInstance.AddComponent<ArrowProjectile>();
            Debug.Log("  ✅ ArrowProjectile добавлена");
        }

        // Сохраняем префаб
        PrefabUtility.SaveAsPrefabAsset(prefabInstance, prefabPath);
        PrefabUtility.UnloadPrefabContents(prefabInstance);

        Debug.Log("✅ Arrow.prefab переделан!");
    }

    void SetupBowPrefab()
    {
        Debug.Log("[2/2] 🏹 Настройка Wooden Bow.prefab...");

        string prefabPath = "Assets/Free medieval weapons/Prefabs/Wooden Bow.prefab";
        GameObject bowPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

        if (bowPrefab == null)
        {
            Debug.LogError("❌ Wooden Bow.prefab не найден!");
            return;
        }

        // Открываем префаб для редактирования
        GameObject prefabInstance = PrefabUtility.LoadPrefabContents(prefabPath);

        // Очищаем старые компоненты
        foreach (Transform child in prefabInstance.transform)
        {
            DestroyImmediate(child.gameObject);
        }

        // Загружаем модель лука
        GameObject bowModel = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Free medieval weapons/Models/Wooden Bow.fbx");

        if (bowModel != null)
        {
            GameObject bow = PrefabUtility.InstantiatePrefab(bowModel, prefabInstance.transform) as GameObject;
            bow.name = "BowModel";
            bow.transform.localPosition = Vector3.zero;
            bow.transform.localRotation = Quaternion.identity;
            bow.transform.localScale = Vector3.one;
            Debug.Log("  ✅ BowModel добавлен");
        }

        // Добавляем BowController если нет
        if (prefabInstance.GetComponent<BowController>() == null)
        {
            prefabInstance.AddComponent<BowController>();
            Debug.Log("  ✅ BowController добавлен");
        }

        // Сохраняем префаб
        PrefabUtility.SaveAsPrefabAsset(prefabInstance, prefabPath);
        PrefabUtility.UnloadPrefabContents(prefabInstance);

        Debug.Log("✅ Wooden Bow.prefab переделан!");
    }
#endif
}
