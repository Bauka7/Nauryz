using UnityEngine;
using UnityEngine.InputSystem;

public class CauldronInteraction : MonoBehaviour
{
    public GameObject steamVFX;

    private bool isPlayerNear = false;

    void Start()
    {
        if (steamVFX != null)
            steamVFX.SetActive(false);

        if (NauryzKozheQuestManager.Instance != null)
            NauryzKozheQuestManager.Instance.HideCauldronPrompt();
    }

    void Update()
    {
        bool ePressed = Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame;

        if (isPlayerNear && ePressed)
        {
            if (NauryzKozheQuestManager.Instance != null &&
                NauryzKozheQuestManager.Instance.AllIngredientsCollected() &&
                !NauryzKozheQuestManager.Instance.questCompleted)
            {
                NauryzKozheQuestManager.Instance.CompleteQuest();

                if (steamVFX != null)
                    steamVFX.SetActive(true);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        isPlayerNear = true;

        if (NauryzKozheQuestManager.Instance != null &&
            NauryzKozheQuestManager.Instance.AllIngredientsCollected() &&
            !NauryzKozheQuestManager.Instance.questCompleted)
        {
            NauryzKozheQuestManager.Instance.ShowCauldronPrompt();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        isPlayerNear = false;

        if (NauryzKozheQuestManager.Instance != null)
            NauryzKozheQuestManager.Instance.HideCauldronPrompt();
    }
}