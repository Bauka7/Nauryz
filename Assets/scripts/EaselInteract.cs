using UnityEngine;
using TMPro;

public class EaselInteract : MonoBehaviour
{
    [Header("Интерфейс")]
    public GameObject promptText; 
    public GameObject drawMinigamePanel; 

    [Header("Управление персонажем")]
    [Tooltip("Перетащите сюда скрипты игрока, которые нужно отключать (например ходьба и обзор мышкой)")]
    public Behaviour[] scriptsToDisable;

    private bool isPlayerNear = false;
    private bool isDrawing = false;

    void Update()
    {
        if (isPlayerNear && !isDrawing && Input.GetKeyDown(KeyCode.E))
        {
            StartDrawing();
        }

        if (isDrawing && Input.GetKeyDown(KeyCode.Escape))
        {
            StopDrawing();
        }
    }

    void StartDrawing()
    {
        isDrawing = true;
        promptText.SetActive(false); 
        drawMinigamePanel.SetActive(true); 
        
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Отключаем скрипты управления (чтобы игрок не двигался)
        foreach (Behaviour script in scriptsToDisable)
        {
            if (script != null) script.enabled = false;
        }
    }

    public void StopDrawing() 
    {
        isDrawing = false;
        drawMinigamePanel.SetActive(false); 
        
        if (isPlayerNear) promptText.SetActive(true); 
        
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Включаем скрипты управления обратно
        foreach (Behaviour script in scriptsToDisable)
        {
            if (script != null) script.enabled = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNear = true;
            if (!isDrawing) promptText.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNear = false;
            promptText.SetActive(false);
        }
    }
}