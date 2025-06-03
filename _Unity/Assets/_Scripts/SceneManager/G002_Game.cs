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
        fadeController.FadeToScene("G001_TagInput");
    }
}