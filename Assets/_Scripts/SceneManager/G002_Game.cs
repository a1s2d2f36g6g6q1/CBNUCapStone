using TMPro;
using UnityEngine;

public class Game : MonoBehaviour
{
    public TMP_InputField[] inputFields;
    public FadeController fadeController;
    public GameObject loadingPanel;

    private void Awake()
    {
        if (loadingPanel != null)
            loadingPanel.SetActive(true);
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
}