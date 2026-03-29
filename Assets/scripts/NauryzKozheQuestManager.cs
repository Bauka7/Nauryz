using TMPro;
using UnityEngine;

public class NauryzKozheQuestManager : MonoBehaviour
{
    public static NauryzKozheQuestManager Instance;

    [Header("UI")]
    public GameObject questPanelObject;
    public TextMeshProUGUI questPanelText;
    public GameObject cauldronPromptObject;

    [Header("Quest State")]
    public bool questStarted = false;
    public bool questCompleted = false;

    public bool saltCollected = false;
    public bool meatCollected = false;
    public bool grainCollected = false;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if (questPanelObject != null)
            questPanelObject.SetActive(false);

        if (cauldronPromptObject != null)
            cauldronPromptObject.SetActive(false);

        UpdateQuestUI();
    }

    public void StartQuest()
    {
        if (questStarted) return;

        questStarted = true;
        questCompleted = false;

        saltCollected = false;
        meatCollected = false;
        grainCollected = false;

        UpdateQuestUI();
    }

    public void ShowQuestUI()
    {
        if (questPanelObject != null)
            questPanelObject.SetActive(true);

        UpdateQuestUI();
    }

    public void HideQuestUI()
    {
        if (questPanelObject != null)
            questPanelObject.SetActive(false);
    }

    public void ShowCauldronPrompt()
    {
        if (cauldronPromptObject != null)
            cauldronPromptObject.SetActive(true);
    }

    public void HideCauldronPrompt()
    {
        if (cauldronPromptObject != null)
            cauldronPromptObject.SetActive(false);
    }

    public void CollectIngredient(string ingredientName)
    {
        if (!questStarted || questCompleted) return;

        switch (ingredientName)
        {
            case "Salt":
                saltCollected = true;
                break;
            case "Meat":
                meatCollected = true;
                break;
            case "Grain":
                grainCollected = true;
                break;
        }

        UpdateQuestUI();
    }

    public bool AllIngredientsCollected()
    {
        return saltCollected && meatCollected && grainCollected;
    }

    public int CollectedCount()
    {
        int count = 0;
        if (saltCollected) count++;
        if (meatCollected) count++;
        if (grainCollected) count++;
        return count;
    }

    public void CompleteQuest()
    {
        if (!questStarted || questCompleted) return;
        if (!AllIngredientsCollected()) return;

        questCompleted = true;

        HideCauldronPrompt();
        HideQuestUI();
    }

    public void UpdateQuestUI()
    {
        if (questPanelText == null) return;

        int collected = CollectedCount();

        if (AllIngredientsCollected())
        {
            questPanelText.text =
                "Nauryz Kozhe\n" +
                "Collected: 3/3\n\n" +
                "Return to the cauldron";
            return;
        }

        string missingList = "";

        if (!saltCollected) missingList += "- Salt\n";
        if (!meatCollected) missingList += "- Meat\n";
        if (!grainCollected) missingList += "- Grain\n";

        questPanelText.text =
            "Nauryz Kozhe\n" +
            "Collected: " + collected + "/3\n\n" +
            "Missing:\n" +
            missingList;
    }
}