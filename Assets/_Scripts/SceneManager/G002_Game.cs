using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Game : MonoBehaviour
{
    public TMP_InputField[] inputFields;
    public FadeController fadeController;
    public GameObject loadingPanel;

    [Header("Alert Popup")]
    public GameObject hostLeftPopup;
    public TMP_Text hostLeftMessage;
    public Button hostLeftConfirmButton;

    private void Awake()
    {
        if (loadingPanel != null)
            loadingPanel.SetActive(true);

        // Hide host left popup
        if (hostLeftPopup != null)
            hostLeftPopup.SetActive(false);

        // Setup host left confirm button
        if (hostLeftConfirmButton != null)
            hostLeftConfirmButton.onClick.AddListener(OnHostLeftConfirm);
    }

    private void Start()
    {
        // Subscribe to multiplayer events if in multiplayer mode
        if (GameData.isMultiplay)
        {
            SubscribeMultiplayEvents();
        }
    }

    private void SubscribeMultiplayEvents()
    {
        if (MultiplaySession.Instance != null)
        {
            MultiplaySession.Instance.OnHostLeft += OnHostLeft;
        }
    }

    private void UnsubscribeMultiplayEvents()
    {
        if (MultiplaySession.Instance != null)
        {
            MultiplaySession.Instance.OnHostLeft -= OnHostLeft;
        }
    }

    private void OnHostLeft()
    {
        Debug.Log("[Game] Host has left during game");
        ShowHostLeftPopup();
    }

    private void ShowHostLeftPopup()
    {
        if (hostLeftPopup != null)
        {
            hostLeftPopup.SetActive(true);

            if (hostLeftMessage != null)
                hostLeftMessage.text = "Host has left the game.\nReturning to main menu...";
        }
        else
        {
            // No popup - return to main menu immediately
            Back();
        }
    }

    private void OnHostLeftConfirm()
    {
        // Hide popup and return to main menu
        if (hostLeftPopup != null)
            hostLeftPopup.SetActive(false);

        Back();
    }

    public void Back()
    {
        // Clear replay data if in replay mode
        if (PuzzleSession.Instance != null && PuzzleSession.Instance.IsReplayMode())
        {
            PuzzleSession.Instance.ClearReplayData();
            Debug.Log("[Game] Replay data cleared");
        }

        // If in multiplayer, clean up WebSocket connection
        if (GameData.isMultiplay)
        {
            Debug.Log("[Game] Leaving multiplayer game");

            if (MultiplaySession.Instance != null && MultiplaySession.Instance.CurrentRoom != null)
            {
                string gameCode = MultiplaySession.Instance.CurrentRoom.gameCode;

                // Send leave event if still connected
                if (SocketIOManager.Instance != null && SocketIOManager.Instance.IsConnected)
                {
                    SocketIOManager.Instance.LeaveRoom(gameCode);
                    SocketIOManager.Instance.UnregisterMultiplayEvents();
                    SocketIOManager.Instance.Disconnect();
                }

                // Clear session data
                MultiplaySession.Instance.ClearRoomData();
            }

            // Reset GameData
            GameData.Reset();
        }

        // Navigate to MainMenu
        if (fadeController != null)
        {
            fadeController.FadeToScene("000_MainMenu");
        }
        else
        {
            Debug.LogWarning("[Game] FadeController not assigned");
            UnityEngine.SceneManagement.SceneManager.LoadScene("000_MainMenu");
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe events
        if (GameData.isMultiplay)
        {
            UnsubscribeMultiplayEvents();
        }

        // Remove button listener
        if (hostLeftConfirmButton != null)
            hostLeftConfirmButton.onClick.RemoveAllListeners();
    }
}