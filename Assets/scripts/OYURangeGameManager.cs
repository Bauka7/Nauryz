using UnityEngine;
using TMPro;

public class OYURangeGameManager : MonoBehaviour
{
    [Header("Screen UI")]
    public GameObject rangeInteractText;
    public GameObject rangeGamePanel;
    public GameObject rangeResultText;

    public TextMeshProUGUI targetsLeftText;
    public TextMeshProUGUI scoreText;

    [Header("NPC Bubble")]
    public GameObject speechBubble;
    public TextMeshProUGUI bubbleText;

    [Header("Game Settings")]
    public int totalTargets = 5;

    [Header("Bow System")]
    public BowSpawner bowSpawner;

    [HideInInspector] public bool playerInsideNPCZone = false;
    [HideInInspector] public bool hasTalkedToNPC = false;
    [HideInInspector] public bool isGameActive = false;
    [HideInInspector] public bool isGameCompleted = false;

    private int score;
    private int targetsLeft;

    void Start()
    {
        ResetToIdle();
    }

    void Update()
    {
        if ((isGameActive || isGameCompleted) && Input.GetKeyDown(KeyCode.F))
        {
            FinishGame();
        }
    }

    public void ShowInteractPrompt(bool show)
    {
        Debug.Log("ShowInteractPrompt: " + show);

        if (rangeInteractText != null)
            rangeInteractText.SetActive(show);
    }

    public void ShowBubble(string message)
{
    Debug.Log("ShowBubble called");

    if (speechBubble != null)
        speechBubble.SetActive(true);

    if (bubbleText != null)
        bubbleText.text = message;
}

    public void HideBubble()
    {
        if (speechBubble != null)
            speechBubble.SetActive(false);
    }

    public void StartRangeGame()
    {
        isGameActive = true;
        isGameCompleted = false;
        hasTalkedToNPC = true;

        score = 0;
        targetsLeft = totalTargets;

        ShowInteractPrompt(false);
        HideBubble();

        if (rangeGamePanel != null)
            rangeGamePanel.SetActive(true);

        if (rangeResultText != null)
            rangeResultText.SetActive(false);

        UpdateGameUI();

        // Спаунируем лук
        if (bowSpawner != null)
        {
            bowSpawner.SpawnBow();
        }
        else
        {
            Debug.LogWarning("BowSpawner not assigned!");
        }
    }

    public void CompleteGame()
    {
        isGameActive = false;
        isGameCompleted = true;

        if (rangeGamePanel != null)
            rangeGamePanel.SetActive(false);

        if (rangeResultText != null)
            rangeResultText.SetActive(true);

        // Удаляем лук
        if (bowSpawner != null)
        {
            bowSpawner.RemoveBow();
        }
    }

    public void FinishGame()
    {
        ResetToIdle();
    }

    public void ResetToIdle()
    {
        isGameActive = false;
        isGameCompleted = false;
        hasTalkedToNPC = false;

        score = 0;
        targetsLeft = totalTargets;

        ShowInteractPrompt(false);
        HideBubble();

        if (rangeGamePanel != null)
            rangeGamePanel.SetActive(false);

        if (rangeResultText != null)
            rangeResultText.SetActive(false);

        // Удаляем лук
        if (bowSpawner != null)
        {
            bowSpawner.RemoveBow();
        }

        UpdateGameUI();
    }

    public void UpdateGameUI()
    {
        if (targetsLeftText != null)
            targetsLeftText.text = "Targets left: " + targetsLeft;

        if (scoreText != null)
            scoreText.text = "Score: " + score;
    }

    public void OnTargetHit(int points)
    {
        if (!isGameActive) return;

        score += points;
        targetsLeft--;

        Debug.Log($"Target hit! Points: {points}, Total Score: {score}, Targets left: {targetsLeft}");

        UpdateGameUI();

        // Если все мишени поражены
        if (targetsLeft <= 0)
        {
            CompleteGame();
            Debug.Log("GAME COMPLETED! Final Score: " + score);
        }
    }
}