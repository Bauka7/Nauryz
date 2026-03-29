using UnityEngine;
using UnityEngine.InputSystem;

public class IngredientPickup : MonoBehaviour
{
    public string ingredientName;
    public GameObject pickupText;

    private bool isPlayerNear = false;
    private bool isCollected = false;

    void Start()
    {
        if (pickupText != null)
            pickupText.SetActive(false);
    }

    void Update()
    {
        if (isCollected) return;

        // ❗ если квест ещё не начат — ничего не делать
        if (NauryzKozheQuestManager.Instance == null ||
            !NauryzKozheQuestManager.Instance.questStarted ||
            NauryzKozheQuestManager.Instance.questCompleted)
        {
            return;
        }

        bool ePressed = Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame;

        if (isPlayerNear && ePressed)
        {
            Collect();
        }
    }

    void Collect()
    {
        isCollected = true;

        if (pickupText != null)
            pickupText.SetActive(false);

        if (NauryzKozheQuestManager.Instance != null)
        {
            NauryzKozheQuestManager.Instance.CollectIngredient(ingredientName);
        }

        gameObject.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        isPlayerNear = true;

        // ❗ показывать prompt только если квест уже начат
        if (pickupText != null &&
            NauryzKozheQuestManager.Instance != null &&
            NauryzKozheQuestManager.Instance.questStarted &&
            !NauryzKozheQuestManager.Instance.questCompleted)
        {
            pickupText.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        isPlayerNear = false;

        if (pickupText != null)
            pickupText.SetActive(false);
    }
}