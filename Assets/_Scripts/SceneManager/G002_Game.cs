using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class Game : MonoBehaviour
{
    public TMP_InputField[] inputFields;
    public FadeController fadeController;
    
    public void Back()
    {
        Debug.Log("뒤로가기 (메인메뉴)");
        fadeController.FadeToScene("000_MainMenu");
    }

}