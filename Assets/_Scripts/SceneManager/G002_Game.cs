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
        if (fadeController != null)
            fadeController.FadeToScene("G001_TagInput");
        else
            Debug.LogWarning("FadeController가 연결되지 않았습니다.");
    }
}