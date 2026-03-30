using UnityEngine;
using UnityEngine.InputSystem;

public class RangeNPCInteraction : MonoBehaviour
{
    public OYURangeGameManager gameManager;

    [TextArea]
    public string introMessage = "Welcome to the OYU target challenge!\nHit all targets to win.\nPress E again to start.";

    private bool playerInside = false;

    void Update()
    {
        if (!playerInside) return;
        if (gameManager == null) return;

        if (gameManager.isGameActive || gameManager.isGameCompleted)
            return;

        if (InteractPressed())
        {
            Debug.Log("INTERACT PRESSED");

            if (!gameManager.hasTalkedToNPC)
            {
                Debug.Log("FIRST TALK");
                gameManager.hasTalkedToNPC = true;
                gameManager.ShowInteractPrompt(false);
                gameManager.ShowBubble(introMessage);
            }
            else
            {
                Debug.Log("START GAME");
                gameManager.StartRangeGame();
            }
        }
    }

    bool InteractPressed()
    {
        // Old Input
        if (Input.GetKeyDown(KeyCode.E)) return true;
        if (Input.GetKeyDown("e")) return true;

        // New Input System
        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame) return true;

        return false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        Debug.Log("Player entered Range NPC zone");

        playerInside = true;

        if (gameManager != null)
        {
            gameManager.playerInsideNPCZone = true;
            gameManager.ShowInteractPrompt(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        Debug.Log("Player exited Range NPC zone");

        playerInside = false;

        if (gameManager != null)
        {
            gameManager.playerInsideNPCZone = false;

            if (!gameManager.isGameActive && !gameManager.isGameCompleted)
            {
                gameManager.ResetToIdle();
            }
        }
    }
}