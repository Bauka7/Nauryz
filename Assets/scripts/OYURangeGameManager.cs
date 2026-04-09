using UnityEngine;
using TMPro;
using System.Collections;

public class OYURangeGameManager : MonoBehaviour
{
    [Header("Screen UI")]
    public GameObject rangeInteractText;
    public GameObject rangeGamePanel;
    public GameObject rangeResultText;
    public TextMeshProUGUI targetsLeftText;
    public TextMeshProUGUI scoreText;

    [Header("Hit Feedback")]
    public TextMeshProUGUI hitFeedbackText;

    [Header("Crosshair")]
    public GameObject crosshair;   // Точка прицела — подключается автоматически

    [Header("NPC Bubble")]
    public GameObject speechBubble;
    public TextMeshProUGUI bubbleText;

    [Header("Sequential Targets")]
    // Перетащи Pos_1, Pos_2, ... Pos_5 в этот массив по порядку
    public GameObject[] sequentialTargets;

    [Header("Bow System")]
    public BowSpawner bowSpawner;

    [HideInInspector] public bool playerInsideNPCZone = false;
    [HideInInspector] public bool hasTalkedToNPC     = false;
    [HideInInspector] public bool isGameActive       = false;
    [HideInInspector] public bool isGameCompleted    = false;

    private int score;
    private int targetsLeft;
    private int currentTargetIndex = 0;

    // ─────────────────────────── Unity ────────────────────────────────────

    void Start()
    {
        ResetToIdle();
    }

    void Update()
    {
        if ((isGameActive || isGameCompleted) && Input.GetKeyDown(KeyCode.F))
            FinishGame();
    }

    // ─────────────────────────── NPC / Bubble ─────────────────────────────

    public void ShowInteractPrompt(bool show)
    {
        if (rangeInteractText != null)
            rangeInteractText.SetActive(show);
    }

    public void ShowBubble(string message)
    {
        if (speechBubble != null) speechBubble.SetActive(true);
        if (bubbleText   != null) bubbleText.text = message;
    }

    public void HideBubble()
    {
        if (speechBubble != null) speechBubble.SetActive(false);
    }

    // ─────────────────────────── Game Flow ────────────────────────────────

    public void StartRangeGame()
    {
        isGameActive       = true;
        isGameCompleted    = false;
        hasTalkedToNPC     = true;
        score              = 0;
        currentTargetIndex = 0;
        targetsLeft        = sequentialTargets != null ? sequentialTargets.Length : 0;

        ShowInteractPrompt(false);
        HideBubble();

        if (rangeGamePanel  != null) rangeGamePanel.SetActive(true);
        if (rangeResultText != null) rangeResultText.SetActive(false);

        // Показываем прицел
        ShowCrosshair(true);

        UpdateGameUI();

        // Показываем только первую мишень, остальные скрываем
        ActivateTarget(0);

        if (bowSpawner != null)
            bowSpawner.SpawnBow();
        else
            Debug.LogWarning("BowSpawner not assigned!");
    }

    public void CompleteGame()
    {
        isGameActive    = false;
        isGameCompleted = true;

        ShowCrosshair(false);

        // Скрываем все оставшиеся мишени
        HideAllTargets();

        if (rangeGamePanel  != null) rangeGamePanel.SetActive(false);
        if (rangeResultText != null) rangeResultText.SetActive(true);

        if (bowSpawner != null) bowSpawner.RemoveBow();
    }

    public void FinishGame()   => ResetToIdle();

    public void ResetToIdle()
    {
        isGameActive    = false;
        isGameCompleted = false;
        hasTalkedToNPC  = false;
        score           = 0;
        targetsLeft     = sequentialTargets != null ? sequentialTargets.Length : 0;
        currentTargetIndex = 0;

        ShowInteractPrompt(false);
        HideBubble();
        ShowCrosshair(false);
        HideAllTargets();

        if (rangeGamePanel  != null) rangeGamePanel.SetActive(false);
        if (rangeResultText != null) rangeResultText.SetActive(false);
        if (bowSpawner      != null) bowSpawner.RemoveBow();

        UpdateGameUI();
    }

    // ─────────────────────────── Targets ──────────────────────────────────

    void ActivateTarget(int index)
    {
        if (sequentialTargets == null) return;

        for (int i = 0; i < sequentialTargets.Length; i++)
        {
            if (sequentialTargets[i] != null)
                sequentialTargets[i].SetActive(i == index);
        }

        if (index < sequentialTargets.Length)
            Debug.Log($"🎯 Активирована мишень #{index + 1}");
    }

    void HideAllTargets()
    {
        if (sequentialTargets == null) return;
        foreach (var t in sequentialTargets)
            if (t != null) t.SetActive(false);
    }

    // ─────────────────────────── Hit ──────────────────────────────────────

    public void OnTargetHit(int points)
    {
        if (!isGameActive) return;

        score += points;
        targetsLeft--;

        // Скрываем текущую мишень, показываем следующую
        if (sequentialTargets != null && currentTargetIndex < sequentialTargets.Length)
            if (sequentialTargets[currentTargetIndex] != null)
                sequentialTargets[currentTargetIndex].SetActive(false);

        currentTargetIndex++;

        if (targetsLeft > 0 && currentTargetIndex < sequentialTargets.Length)
            StartCoroutine(ShowNextTargetWithDelay(currentTargetIndex));

        UpdateGameUI();

        if (hitFeedbackText != null)
            StartCoroutine(ShowHitFeedback(points));

        if (targetsLeft <= 0)
            StartCoroutine(DelayedComplete());
    }

    IEnumerator ShowNextTargetWithDelay(int index)
    {
        yield return _waitHalfSec;
        ActivateTarget(index);
    }

    IEnumerator DelayedComplete()
    {
        yield return _waitOneSec;
        CompleteGame();
        Debug.Log($"🏆 GAME COMPLETED! Hits: {score}");
    }

    // ─────────────────────────── UI ───────────────────────────────────────

    public void UpdateGameUI()
    {
        if (targetsLeftText != null)
            targetsLeftText.text = "Targets left: " + targetsLeft;

        if (scoreText != null)
            scoreText.text = "Score: " + score;
    }

    void ShowCrosshair(bool show)
    {
        if (crosshair != null)
            crosshair.SetActive(show);
    }

    // ─────────────────────────── Hit Feedback ─────────────────────────────

    static readonly WaitForSeconds _waitHalfSec = new(0.5f);
    static readonly WaitForSeconds _waitOneSec  = new(1f);

    IEnumerator ShowHitFeedback(int points)
    {
        string label;
        Color  color;

        switch (points)
        {
            case 10: label = "+10  BULLSEYE!"; color = new Color(1f,  0.85f, 0f);   break;
            case 8:  label = "+8";             color = new Color(0.9f, 0.2f, 0.2f); break;
            case 6:  label = "+6";             color = new Color(0.3f, 0.6f, 1f);   break;
            case 4:  label = "+4";             color = new Color(0.5f, 0.5f, 0.5f); break;
            case 2:  label = "+2";             color = Color.white;                  break;
            default: label = "Мимо!";          color = Color.gray;                   break;
        }

        hitFeedbackText.text  = label;
        hitFeedbackText.color = color;
        hitFeedbackText.gameObject.SetActive(true);

        yield return _waitOneSec;

        float t = 0f;
        while (t < 0.5f)
        {
            t += Time.deltaTime;
            Color c = hitFeedbackText.color;
            c.a = 1f - t / 0.5f;
            hitFeedbackText.color = c;
            yield return null;
        }

        hitFeedbackText.gameObject.SetActive(false);
    }
}
