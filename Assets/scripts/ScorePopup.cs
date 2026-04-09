using UnityEngine;
using TMPro;
using System.Collections;

/// <summary>
/// Прикрепи на пустой Canvas WorldSpace объект.
/// Вызывается из ArcheryTarget при попадании стрелы.
/// </summary>
public class ScorePopup : MonoBehaviour
{
    public TextMeshProUGUI label;

    void Awake()
    {
        if (label == null)
            label = GetComponentInChildren<TextMeshProUGUI>();
    }

    public void Show(string text, Color color)
    {
        if (label != null)
        {
            label.text = text;
            label.color = color;
        }
        StartCoroutine(Animate());
    }

    IEnumerator Animate()
    {
        float duration = 2f;
        float elapsed = 0f;
        Vector3 startPos = transform.position;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            // Плывёт вверх
            transform.position = startPos + Vector3.up * (t * 1.5f);

            // Затухает в конце
            if (label != null)
            {
                Color c = label.color;
                c.a = 1f - Mathf.Pow(t, 2f);
                label.color = c;
            }

            // Всегда смотрит на камеру
            if (Camera.main != null)
                transform.rotation = Quaternion.LookRotation(transform.position - Camera.main.transform.position);

            yield return null;
        }

        Destroy(gameObject);
    }

    // ── Фабрика — создаёт попап в мировых координатах ────────────────────
    public static void Spawn(Vector3 worldPosition, int score)
    {
        // Создаём Canvas в мировом пространстве
        GameObject canvasGO = new GameObject("ScorePopupCanvas");
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.worldCamera = Camera.main;

        RectTransform rt = canvasGO.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(2f, 1f);
        rt.localScale = Vector3.one * 0.01f;

        canvasGO.transform.position = worldPosition + Vector3.up * 0.5f;

        // Текстовый объект
        GameObject textGO = new GameObject("Text");
        textGO.transform.SetParent(canvasGO.transform, false);
        TextMeshProUGUI tmp = textGO.AddComponent<TextMeshProUGUI>();
        tmp.fontSize = 80;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontStyle = FontStyles.Bold;

        RectTransform trt = textGO.GetComponent<RectTransform>();
        trt.anchorMin = Vector2.zero;
        trt.anchorMax = Vector2.one;
        trt.offsetMin = Vector2.zero;
        trt.offsetMax = Vector2.zero;

        ScorePopup popup = canvasGO.AddComponent<ScorePopup>();
        popup.label = tmp;

        // Текст и цвет по очкам
        string text;
        Color color;
        GetScoreInfo(score, out text, out color);

        popup.Show(text, color);
    }

    static void GetScoreInfo(int score, out string text, out Color color)
    {
        switch (score)
        {
            case 10:
                text = "10/10\n✦ BULLSEYE! ✦";
                color = new Color(1f, 0.85f, 0f); // Золотой
                break;
            case 8:
                text = "8/10";
                color = new Color(0.9f, 0.15f, 0.15f); // Красный
                break;
            case 6:
                text = "6/10";
                color = new Color(0.2f, 0.5f, 1f); // Синий
                break;
            case 4:
                text = "4/10";
                color = new Color(0.2f, 0.2f, 0.2f); // Чёрный
                break;
            case 2:
                text = "2/10";
                color = new Color(0.85f, 0.85f, 0.85f); // Белый
                break;
            default:
                text = "Мимо!";
                color = Color.gray;
                break;
        }
    }
}
