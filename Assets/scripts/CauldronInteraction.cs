using UnityEngine;
using UnityEngine.InputSystem;

public class CauldronInteraction : MonoBehaviour
{
    public GameObject steamVFX;
    public NPCInteraction npc;
    public CauldronCookingEffect cookingEffect; // Добавим эффект варки

    private bool isPlayerNear = false;

    void Start()
    {
        if (steamVFX != null)
            steamVFX.SetActive(false);

        // Найдём CauldronCookingEffect если он не назначен
        if (cookingEffect == null)
        {
            cookingEffect = GetComponent<CauldronCookingEffect>();
        }

        if (NauryzKozheQuestManager.Instance != null)
            NauryzKozheQuestManager.Instance.HideCauldronPrompt();
    }

    void Update()
    {
        bool ePressed = Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame;

        if (isPlayerNear && ePressed)
        {
            Debug.Log("E pressed in cauldron zone");

            if (NauryzKozheQuestManager.Instance != null &&
                NauryzKozheQuestManager.Instance.AllIngredientsCollected() &&
                !NauryzKozheQuestManager.Instance.questCompleted)
            {
                Debug.Log("Completing quest");

                NauryzKozheQuestManager.Instance.CompleteQuest();

                if (steamVFX != null)
                    steamVFX.SetActive(true);

                // Запускаем эффект варки
                if (cookingEffect != null)
                {
                    cookingEffect.PlayCookingEffect();
                    Debug.Log("🍲 Cooking effect started!");
                }

                if (npc != null)
                    npc.ShowThankYou();
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Entered cauldron trigger: " + other.name + " | tag = " + other.tag);

        if (!other.CompareTag("Player")) return;

        Debug.Log("PLAYER entered cauldron trigger");

        isPlayerNear = true;

        if (NauryzKozheQuestManager.Instance != null &&
            NauryzKozheQuestManager.Instance.AllIngredientsCollected() &&
            !NauryzKozheQuestManager.Instance.questCompleted)
        {
            Debug.Log("Showing cauldron prompt");
            NauryzKozheQuestManager.Instance.ShowCauldronPrompt();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Debug.Log("Exited cauldron trigger: " + other.name + " | tag = " + other.tag);

        if (!other.CompareTag("Player")) return;

        Debug.Log("PLAYER exited cauldron trigger");

        isPlayerNear = false;

        if (NauryzKozheQuestManager.Instance != null)
            NauryzKozheQuestManager.Instance.HideCauldronPrompt();
    }
}