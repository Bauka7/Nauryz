#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

/// <summary>
/// Меню: Tools → Setup Archery Mini-Game
/// Автоматически добавляет мишени, коллайдеры и компоненты в сцену.
/// </summary>
public class ArcheryGameSetup : EditorWindow
{
    // ══ ГЛАВНАЯ КНОПКА — делает всё сразу ═══════════════════════════════
    [MenuItem("Tools/⚡ FINISH ARCHERY GAME (All-in-One)")]
    static void FinishAll()
    {
        SetupArchery();
        SetupHitFeedbackText();
        ReplaceTargetMeshes();
        SetupSequentialTargets();
        SetupCrosshair();

        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());

        EditorUtility.DisplayDialog("✅ Готово!",
            "Всё настроено:\n" +
            "✅ Мишени заменены на RoundTarget\n" +
            "✅ Последовательное появление мишеней\n" +
            "✅ Прицел (crosshair) добавлен\n" +
            "✅ HitFeedbackText создан\n\n" +
            "Сохрани сцену (Ctrl+S) и нажми Play!",
            "OK");
    }

    // ── Прицел в центре экрана ────────────────────────────────────────────
    [MenuItem("Tools/Setup Crosshair")]
    static void SetupCrosshair()
    {
        OYURangeGameManager gm = Object.FindFirstObjectByType<OYURangeGameManager>();
        if (gm == null) return;

        // Уже есть?
        if (gm.crosshair != null) { Debug.Log("✅ Crosshair уже подключён"); return; }

        // Ищем Canvas
        Canvas canvas = Object.FindFirstObjectByType<Canvas>();
        if (canvas == null) { Debug.LogWarning("⚠️ Canvas не найден"); return; }

        // Создаём объект прицела
        GameObject crosshairGO = new GameObject("Crosshair");
        Undo.RegisterCreatedObjectUndo(crosshairGO, "Create Crosshair");
        crosshairGO.transform.SetParent(canvas.transform, false);

        RectTransform rt = crosshairGO.AddComponent<RectTransform>();
        rt.anchorMin        = new Vector2(0.5f, 0.5f);
        rt.anchorMax        = new Vector2(0.5f, 0.5f);
        rt.pivot            = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta        = new Vector2(24f, 24f);

        // Рисуем точку через TMPro символ
        var tmp = crosshairGO.AddComponent<TMPro.TextMeshProUGUI>();
        tmp.text      = "·";          // точка (U+00B7)
        tmp.fontSize  = 60;
        tmp.alignment = TMPro.TextAlignmentOptions.Center;
        tmp.color     = new Color(1f, 1f, 1f, 0.9f);

        // По умолчанию скрыт — включится когда игра стартует
        crosshairGO.SetActive(false);

        // Подключаем к GM
        Undo.RecordObject(gm, "Wire Crosshair");
        gm.crosshair = crosshairGO;
        EditorUtility.SetDirty(gm);

        Debug.Log("✅ Crosshair создан и подключён");
    }

    // ── Последовательные мишени ───────────────────────────────────────────
    [MenuItem("Tools/Setup Sequential Targets")]
    static void SetupSequentialTargets()
    {
        OYURangeGameManager gm = Object.FindFirstObjectByType<OYURangeGameManager>();
        if (gm == null) { Debug.LogWarning("⚠️ OYURangeGameManager не найден"); return; }

        GameObject targetPositions = GameObject.Find("TargetPositions");
        if (targetPositions == null) { Debug.LogWarning("⚠️ TargetPositions не найден"); return; }

        int count = targetPositions.transform.childCount;
        if (count == 0) { Debug.LogWarning("⚠️ Нет дочерних объектов в TargetPositions"); return; }

        // Собираем Pos_1...Pos_N отсортированные по имени
        var posList = new System.Collections.Generic.List<Transform>();
        foreach (Transform child in targetPositions.transform)
            posList.Add(child);

        posList.Sort((a, b) => string.Compare(a.name, b.name, System.StringComparison.Ordinal));

        var targets = new GameObject[posList.Count];
        for (int i = 0; i < posList.Count; i++)
            targets[i] = posList[i].gameObject;

        Undo.RecordObject(gm, "Wire Sequential Targets");
        gm.sequentialTargets = targets;
        EditorUtility.SetDirty(gm);

        // Скрываем все кроме первой (начальное состояние)
        for (int i = 0; i < targets.Length; i++)
        {
            Undo.RecordObject(targets[i], "Hide Target");
            targets[i].SetActive(i == 0);
        }

        Debug.Log($"✅ Sequential targets настроены: {count} мишеней по порядку");
    }

    [MenuItem("Tools/Setup Archery Mini-Game")]
    static void SetupArchery()
    {
        int fixed_ = 0;
        int errors = 0;

        // ── 1. Найти TargetPositions ───────────────────────────────────────
        GameObject targetPositions = GameObject.Find("TargetPositions");
        if (targetPositions == null)
        {
            Debug.LogError("❌ TargetPositions не найден в сцене!");
            errors++;
        }
        else
        {
            // Загружаем FBX-модель мишени
            string targetPath = "Assets/MiniGame2/practice-target/small/ShootingTargetCollection.fbx";
            GameObject targetFBX = AssetDatabase.LoadAssetAtPath<GameObject>(targetPath);

            if (targetFBX == null)
            {
                // Попробуем альтернативную мишень
                targetPath = "Assets/MiniGame2/target-1-forest-shooter/source/Target 1.fbx";
                targetFBX = AssetDatabase.LoadAssetAtPath<GameObject>(targetPath);
            }

            if (targetFBX == null)
            {
                Debug.LogWarning("⚠️ FBX мишени не найден, используем Cube-заглушку");
            }

            // Обходим все дочерние позиции (Pos_1 ... Pos_5)
            foreach (Transform pos in targetPositions.transform)
            {
                // Пропускаем если уже есть ArcheryTarget
                if (pos.GetComponentInChildren<ArcheryTarget>() != null)
                {
                    Debug.Log($"✅ {pos.name} уже настроен, пропускаем");
                    continue;
                }

                // Удаляем старые дочерние объекты если они есть
                for (int i = pos.childCount - 1; i >= 0; i--)
                    Undo.DestroyObjectImmediate(pos.GetChild(i).gameObject);

                // ── Создаём визуал мишени ──────────────────────────────────
                GameObject targetVisual;
                if (targetFBX != null)
                {
                    targetVisual = Object.Instantiate(targetFBX, pos);
                    targetVisual.name = "TargetMesh";
                    targetVisual.transform.localPosition = Vector3.zero;
                    targetVisual.transform.localRotation = Quaternion.identity;
                    targetVisual.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
                    Undo.RegisterCreatedObjectUndo(targetVisual, "Create Target Visual");
                }
                else
                {
                    // Cube-заглушка если FBX не найден
                    targetVisual = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    targetVisual.name = "TargetMesh";
                    targetVisual.transform.SetParent(pos);
                    targetVisual.transform.localPosition = new Vector3(0, 1.2f, 0);
                    targetVisual.transform.localScale = new Vector3(0.6f, 0.05f, 0.6f);
                    Undo.RegisterCreatedObjectUndo(targetVisual, "Create Target Visual");

                    // Раскрашиваем
                    Renderer r = targetVisual.GetComponent<Renderer>();
                    if (r != null)
                    {
                        r.sharedMaterial = new Material(Shader.Find("Standard"));
                        r.sharedMaterial.color = new Color(0.8f, 0.1f, 0.1f);
                    }

                    // Удаляем физический коллайдер от Primitive (заменим trigger ниже)
                    Collider primitiveCol = targetVisual.GetComponent<Collider>();
                    if (primitiveCol != null)
                        Undo.DestroyObjectImmediate(primitiveCol);
                }

                // Поднимаем позицию чтобы мишень была над землёй
                pos.localPosition = new Vector3(pos.localPosition.x, 0, pos.localPosition.z);

                // ── Добавляем Trigger Collider ─────────────────────────────
                SphereCollider trigger = Undo.AddComponent<SphereCollider>(pos.gameObject);
                trigger.isTrigger = true;
                trigger.radius = 0.4f;
                trigger.center = new Vector3(0, 1.2f, 0);

                // ── Добавляем ArcheryTarget ────────────────────────────────
                ArcheryTarget archTarget = Undo.AddComponent<ArcheryTarget>(pos.gameObject);
                archTarget.bullseyeRadius = 0.05f;
                archTarget.innerRadius    = 0.10f;
                archTarget.midRadius      = 0.18f;
                archTarget.outerRadius    = 0.26f;
                archTarget.maxRadius      = 0.34f;

                Debug.Log($"✅ {pos.name} — мишень настроена");
                fixed_++;

                Undo.RegisterCreatedObjectUndo(pos.gameObject, "Setup Archery Target");
            }
        }

        // ── 2. Проверяем Arrow.prefab ─────────────────────────────────────
        string arrowPath = "Assets/Free medieval weapons/Prefabs/Arrow.prefab";
        GameObject arrowPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(arrowPath);
        if (arrowPrefab != null)
        {
            bool hasProjectile = arrowPrefab.GetComponent<ArrowProjectile>() != null;
            bool hasRigidbody  = arrowPrefab.GetComponent<Rigidbody>() != null;
            bool hasCollider   = arrowPrefab.GetComponent<Collider>() != null;

            if (!hasProjectile || !hasRigidbody || !hasCollider)
            {
                Debug.Log("🔧 Настраиваем Arrow.prefab...");

                using (var prefabScope = new PrefabUtility.EditPrefabContentsScope(arrowPath))
                {
                    GameObject root = prefabScope.prefabContentsRoot;

                    if (root.GetComponent<ArrowProjectile>() == null)
                        root.AddComponent<ArrowProjectile>();

                    Rigidbody rb = root.GetComponent<Rigidbody>();
                    if (rb == null) rb = root.AddComponent<Rigidbody>();
                    rb.mass = 0.05f;
                    rb.useGravity = true;
                    rb.linearDamping = 0.05f;
                    rb.angularDamping = 0.1f;

                    if (root.GetComponent<Collider>() == null)
                    {
                        CapsuleCollider col = root.AddComponent<CapsuleCollider>();
                        col.isTrigger = true;
                        col.radius = 0.03f;
                        col.height = 0.6f;
                        col.direction = 2; // Z-axis
                    }
                }
                Debug.Log("✅ Arrow.prefab настроен");
                fixed_++;
            }
            else
            {
                Debug.Log("✅ Arrow.prefab уже настроен");
            }
        }
        else
        {
            Debug.LogError("❌ Arrow.prefab не найден по пути: " + arrowPath);
            errors++;
        }

        // ── 3. Проверяем Wooden Bow.prefab ────────────────────────────────
        string bowPath = "Assets/Free medieval weapons/Prefabs/Wooden Bow.prefab";
        GameObject bowPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(bowPath);
        if (bowPrefab != null)
        {
            bool hasBowController = bowPrefab.GetComponent<BowController>() != null;
            if (!hasBowController)
            {
                Debug.Log("🔧 Добавляем BowController в Wooden Bow.prefab...");

                using (var prefabScope = new PrefabUtility.EditPrefabContentsScope(bowPath))
                {
                    GameObject root = prefabScope.prefabContentsRoot;
                    if (root.GetComponent<BowController>() == null)
                        root.AddComponent<BowController>();
                }
                Debug.Log("✅ BowController добавлен в Wooden Bow.prefab");
                fixed_++;
            }
            else
            {
                Debug.Log("✅ Wooden Bow.prefab уже настроен");
            }
        }
        else
        {
            Debug.LogError("❌ Wooden Bow.prefab не найден: " + bowPath);
            errors++;
        }

        // ── 4. Проверяем RangeNPC коллайдер ───────────────────────────────
        RangeNPCInteraction rangeNPC = Object.FindFirstObjectByType<RangeNPCInteraction>();
        if (rangeNPC != null)
        {
            Collider npcCollider = rangeNPC.GetComponent<Collider>();
            if (npcCollider == null)
            {
                BoxCollider bc = Undo.AddComponent<BoxCollider>(rangeNPC.gameObject);
                bc.isTrigger = true;
                bc.size = new Vector3(3f, 2.5f, 3f);
                bc.center = new Vector3(0, 1.25f, 0);
                Debug.Log("✅ BoxCollider добавлен на RangeNPC");
                fixed_++;
            }
            else if (!npcCollider.isTrigger)
            {
                npcCollider.isTrigger = true;
                Debug.Log("✅ RangeNPC коллайдер переключён в Trigger");
                fixed_++;
            }
            else
            {
                Debug.Log("✅ RangeNPC коллайдер уже настроен");
            }
        }
        else
        {
            Debug.LogWarning("⚠️ RangeNPCInteraction не найден в сцене");
        }

        // ── 5. Итог ───────────────────────────────────────────────────────
        EditorUtility.SetDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects()[0]);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());

        string msg = $"Настройка завершена!\n✅ Исправлено: {fixed_}\n❌ Ошибок: {errors}";
        Debug.Log("🏹 " + msg);

        if (errors == 0)
            EditorUtility.DisplayDialog("Archery Setup", msg + "\n\nИгра готова! Нажми Play.", "OK");
        else
            EditorUtility.DisplayDialog("Archery Setup — Ошибки", msg + "\n\nПроверь Console для деталей.", "OK");
    }

    // ── Создание HitFeedback текста и подключение к GameManager ──────────
    [MenuItem("Tools/Setup Hit Feedback Text")]
    static void SetupHitFeedbackText()
    {
        // 1. Находим OYURangeGameManager
        OYURangeGameManager gm = Object.FindFirstObjectByType<OYURangeGameManager>();
        if (gm == null)
        {
            EditorUtility.DisplayDialog("Ошибка", "OYURangeGameManager не найден в сцене!", "OK");
            return;
        }

        // 2. Проверяем - уже подключён?
        if (gm.hitFeedbackText != null)
        {
            EditorUtility.DisplayDialog("Уже готово", "hitFeedbackText уже подключён!", "OK");
            return;
        }

        // 3. Находим RangeGamePanel
        GameObject panel = GameObject.Find("RangeGamePanel");
        if (panel == null)
        {
            EditorUtility.DisplayDialog("Ошибка", "RangeGamePanel не найден в сцене!", "OK");
            return;
        }

        // 4. Создаём объект HitFeedbackText внутри панели
        GameObject hitTextGO = new GameObject("HitFeedbackText");
        Undo.RegisterCreatedObjectUndo(hitTextGO, "Create HitFeedbackText");
        hitTextGO.transform.SetParent(panel.transform, false);

        // 5. Добавляем RectTransform (уже добавлен, настраиваем)
        RectTransform rt = hitTextGO.GetComponent<RectTransform>();
        if (rt == null) rt = hitTextGO.AddComponent<RectTransform>();

        // Центрируем по горизонтали, располагаем под заголовком
        rt.anchorMin        = new Vector2(0f, 1f);
        rt.anchorMax        = new Vector2(1f, 1f);
        rt.pivot            = new Vector2(0.5f, 1f);
        rt.anchoredPosition = new Vector2(0f, -110f);
        rt.sizeDelta        = new Vector2(0f, 60f);

        // 6. Добавляем TMPro текст
        var tmp = hitTextGO.AddComponent<TMPro.TextMeshProUGUI>();
        tmp.text      = "";
        tmp.fontSize  = 36;
        tmp.fontStyle = TMPro.FontStyles.Bold;
        tmp.alignment = TMPro.TextAlignmentOptions.Center;
        tmp.color     = Color.yellow;

        // Начально скрыт
        hitTextGO.SetActive(false);

        // 7. Подключаем к GameManager
        Undo.RecordObject(gm, "Wire HitFeedbackText");
        gm.hitFeedbackText = tmp;
        EditorUtility.SetDirty(gm);

        // 8. Сохраняем сцену
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());

        Debug.Log("✅ HitFeedbackText создан и подключён к OYURangeGameManager");
        EditorUtility.DisplayDialog("Готово!",
            "HitFeedbackText создан и подключён!\n\nСохрани сцену: Ctrl+S → Play!",
            "OK");
    }

    // ── Замена мишеней на чистый RoundTarget ─────────────────────────────
    [MenuItem("Tools/Replace Targets with Round Target")]
    static void ReplaceTargetMeshes()
    {
        GameObject targetPositions = GameObject.Find("TargetPositions");
        if (targetPositions == null)
        {
            EditorUtility.DisplayDialog("Ошибка", "TargetPositions не найден в сцене!", "OK");
            return;
        }

        // Загружаем Target 1.fbx (просто круглая мишень без лишних объектов)
        string roundTargetPath = "Assets/MiniGame2/target-1-forest-shooter/source/Target 1.fbx";
        GameObject roundTargetFBX = AssetDatabase.LoadAssetAtPath<GameObject>(roundTargetPath);

        if (roundTargetFBX == null)
        {
            EditorUtility.DisplayDialog("Ошибка", "Target 1.fbx не найден:\n" + roundTargetPath, "OK");
            return;
        }

        int replaced = 0;

        foreach (Transform pos in targetPositions.transform)
        {
            // Удаляем ВСЕ дочерние объекты (старые мешсы)
            for (int i = pos.childCount - 1; i >= 0; i--)
                Undo.DestroyObjectImmediate(pos.GetChild(i).gameObject);

            // Создаём чистый RoundTarget
            GameObject mesh = Object.Instantiate(roundTargetFBX, pos);
            Undo.RegisterCreatedObjectUndo(mesh, "Replace Target Mesh");
            mesh.name = "RoundTarget";
            mesh.transform.localPosition = new Vector3(0f, 0f, 0f);
            mesh.transform.localRotation = Quaternion.Euler(0f, 180f, 0f); // мишень смотрит на игрока
            mesh.transform.localScale    = new Vector3(2f, 2f, 2f);

            // Обновляем SphereCollider чтобы точно охватывал мишень
            SphereCollider sc = pos.GetComponent<SphereCollider>();
            if (sc != null)
            {
                sc.center = new Vector3(0f, 1.2f, 0f);
                sc.radius = 0.5f;
            }

            replaced++;
            Debug.Log($"✅ {pos.name} → RoundTarget установлен");
        }

        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());

        EditorUtility.DisplayDialog("Готово",
            $"Заменено мишеней: {replaced}\n\nСохрани сцену: Ctrl+S → Play!",
            "OK");
    }

    // ── Дополнительный пункт: диагностика ────────────────────────────────
    [MenuItem("Tools/Diagnose Archery Mini-Game")]
    static void DiagnoseArchery()
    {
        string report = "=== ДИАГНОСТИКА ARCHERY MINI-GAME ===\n\n";

        // GameManager
        OYURangeGameManager gm = Object.FindFirstObjectByType<OYURangeGameManager>();
        report += gm != null ? "✅ OYURangeGameManager найден\n" : "❌ OYURangeGameManager НЕ найден\n";

        // BowSpawner
        BowSpawner bs = Object.FindFirstObjectByType<BowSpawner>();
        if (bs != null)
        {
            report += "✅ BowSpawner найден\n";
            report += bs.bowPrefab != null ? "  ✅ bowPrefab подключён\n" : "  ❌ bowPrefab НЕ подключён\n";
            report += bs.arrowPrefab != null ? "  ✅ arrowPrefab подключён\n" : "  ❌ arrowPrefab НЕ подключён\n";
            report += bs.handPosition != null ? "  ✅ handPosition подключён\n" : "  ❌ handPosition НЕ подключён\n";
        }
        else report += "❌ BowSpawner НЕ найден\n";

        // NPC
        RangeNPCInteraction npc = Object.FindFirstObjectByType<RangeNPCInteraction>();
        if (npc != null)
        {
            report += "✅ RangeNPCInteraction найден\n";
            Collider c = npc.GetComponent<Collider>();
            report += (c != null && c.isTrigger) ? "  ✅ Trigger-коллайдер есть\n" : "  ❌ Trigger-коллайдер ОТСУТСТВУЕТ\n";
            report += npc.gameManager != null ? "  ✅ gameManager подключён\n" : "  ❌ gameManager НЕ подключён\n";
        }
        else report += "❌ RangeNPCInteraction НЕ найден\n";

        // Targets
        ArcheryTarget[] targets = Object.FindObjectsByType<ArcheryTarget>(FindObjectsSortMode.None);
        report += $"\n🎯 ArcheryTarget компонентов в сцене: {targets.Length}\n";
        if (targets.Length == 0)
            report += "  ⚠️ Запусти Tools → Setup Archery Mini-Game!\n";

        // TargetPositions
        GameObject tp = GameObject.Find("TargetPositions");
        if (tp != null)
            report += $"✅ TargetPositions найден ({tp.transform.childCount} позиций)\n";
        else
            report += "❌ TargetPositions НЕ найден\n";

        // Player tag
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        report += player != null ? "✅ Player (tag) найден\n" : "❌ Игрок с тегом Player НЕ найден\n";

        // Arrow prefab
        string arrowPath = "Assets/Free medieval weapons/Prefabs/Arrow.prefab";
        GameObject arrowPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(arrowPath);
        if (arrowPrefab != null)
        {
            report += "✅ Arrow.prefab найден\n";
            report += arrowPrefab.GetComponent<ArrowProjectile>() != null ? "  ✅ ArrowProjectile есть\n" : "  ❌ ArrowProjectile ОТСУТСТВУЕТ\n";
            report += arrowPrefab.GetComponent<Rigidbody>() != null ? "  ✅ Rigidbody есть\n" : "  ❌ Rigidbody ОТСУТСТВУЕТ\n";
            report += arrowPrefab.GetComponent<Collider>() != null ? "  ✅ Collider есть\n" : "  ❌ Collider ОТСУТСТВУЕТ\n";
        }
        else report += "❌ Arrow.prefab НЕ найден\n";

        Debug.Log(report);
        EditorUtility.DisplayDialog("Archery Diagnostic", report, "OK");
    }
}
#endif
