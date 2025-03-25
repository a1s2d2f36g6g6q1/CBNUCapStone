using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void StartGame()
    {
        SceneManager.LoadScene("001_Planet");
    }

    public void OpenFriends()
    {
        SceneManager.LoadScene("002_Friend");
    }

    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("게임 종료!");
    }
}