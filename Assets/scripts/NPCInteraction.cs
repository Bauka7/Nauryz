using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class NPCInteraction : MonoBehaviour
{
    public GameObject interactText;
    public GameObject speechBubble;
    public TextMeshProUGUI bubbleText;

    [TextArea(2, 4)]
    public string introMessage =
        "Nauryz Kozhe is already cooking.\nFind 3 missing ingredients and come back.";

    [TextArea(2, 4)]
    public string thanksMessage =
        "Thank you for your help!\nThe Nauryz Kozhe is ready.";

    private bool isPlayerNear = false;
    private bool hasTalkedOnce = false;

    void Start()
    {
        if (interactText != null)
        {
            interactText.SetActive(true);
            interactText.SetActive(false);
        }

        if (speechBubble != null)
        {
            speechBubble.SetActive(true);
            speechBubble.SetActive(false);
        }
    }

    void Update()
    {
        bool ePressed = Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame;

        if (isPlayerNear && ePressed)
        {
            if (interactText != null)
                interactText.SetActive(false);

            if (speechBubble != null)
            {
                if (bubbleText != null)
                {
                    if (NauryzKozheQuestManager.Instance != null &&
                        NauryzKozheQuestManager.Instance.questCompleted)
                    {
                        bubbleText.text = thanksMessage;
                    }
                    else
                    {
                        bubbleText.text = introMessage;
                    }
                }

                speechBubble.SetActive(true);
                CancelInvoke(nameof(HideBubble));
                Invoke(nameof(HideBubble), 4f);
            }

            if (NauryzKozheQuestManager.Instance != null &&
                !NauryzKozheQuestManager.Instance.questStarted)
            {
                hasTalkedOnce = true;
            }
        }
    }

    void HideBubble()
    {
        if (speechBubble != null)
            speechBubble.SetActive(false);

        if (NauryzKozheQuestManager.Instance != null &&
            !NauryzKozheQuestManager.Instance.questStarted)
        {
            NauryzKozheQuestManager.Instance.StartQuest();
            NauryzKozheQuestManager.Instance.ShowQuestUI();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        isPlayerNear = true;

        if (NauryzKozheQuestManager.Instance != null &&
            NauryzKozheQuestManager.Instance.questCompleted)
        {
            if (interactText != null)
                interactText.SetActive(false);
            return;
        }

        if (!hasTalkedOnce && interactText != null)
            interactText.SetActive(true);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        isPlayerNear = false;

        if (interactText != null)
            interactText.SetActive(false);

        if (speechBubble != null)
            speechBubble.SetActive(false);
    }
}