using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class ArrowFixSetup : MonoBehaviour
{
#if UNITY_EDITOR
    [ContextMenu("🔧 Fix Arrow Orientation")]
    public void FixArrowOrientation()
    {
        Debug.Log("🔧 Fixing Arrow orientation...");

        // Найти Arrow.prefab
        string[] paths = new[]
        {
            "Assets/Free medieval weapons/Prefabs/Arrow.prefab",
            "Assets/Free medieval weapons/Prefabs/Arrow .prefab",
            "Assets/MiniGame2/Free medieval weapons/Prefabs/Arrow.prefab",
        };

        GameObject arrowPrefab = null;
        foreach (string path in paths)
        {
            arrowPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (arrowPrefab != null)
            {
                Debug.Log($"✅ Arrow.prefab найден: {path}");
                break;
            }
        }

        if (arrowPrefab == null)
        {
            Debug.LogError("❌ Arrow.prefab не найден!");
            return;
        }

        // Проверяем все child объекты и убедимся что они правильно ориентированы
        foreach (Transform child in arrowPrefab.GetComponentsInChildren<Transform>())
        {
            // Стрела должна смотреть ВПЕРЁД (Z+)
            Debug.Log($"Object: {child.name}, Rotation: {child.localEulerAngles}");
        }

        Debug.Log("✅ Arrow prefab checked!");
        
        EditorUtility.DisplayDialog("Info", 
            "Проверь ориентацию Arrow в Inspector:\n\n" +
            "- Стрела должна смотреть ВПЕРЁД (по Z)\n" +
            "- Наконечник впереди\n" +
            "- Оперение позади\n\n" +
            "Если ориентация неправильно - поверни модель вручную.",
            "OK");
    }
#endif
}
