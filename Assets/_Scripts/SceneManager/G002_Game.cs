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

        // Always go back to MainMenu (not TagInput)
        // This works for: Singleplay, Multiplay, Gallery Replay
        if (fadeController != null)
        {
            fadeController.FadeToScene("000_MainMenu");
        }
        else
        {
            Debug.LogWarning("FadeController가 연결되지 않았습니다.");
            UnityEngine.SceneManagement.SceneManager.LoadScene("000_MainMenu");
        }
    }
}