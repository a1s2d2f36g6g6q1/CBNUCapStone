using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class TagInputManager : MonoBehaviour
{
    public TMP_InputField[] inputFields;
    public FadeController fadeController;

    public void OnStartGame()
    {
        for (int i = 0; i < 4; i++)
        {
            GameData.tags[i] = inputFields[i].text;
        }

        fadeController.FadeToScene("G002_Game");
    }
    
    public void Back()
    {
        Debug.Log("뒤로가기 (메인메뉴)");
        fadeController.FadeToScene("000_MainMenu");
    }

}