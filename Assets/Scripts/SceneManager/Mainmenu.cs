using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public FadeController fadeController;
    
    public void StartGame()
    {
        fadeController.FadeToScene("G001_TagInput");
    }

    public void OpenFriends()
    {
        fadeController.FadeToScene("002_Friend");
    }

    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("게임 종료!");
    }
}