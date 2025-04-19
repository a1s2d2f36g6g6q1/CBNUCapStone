using UnityEngine;
using UnityEngine.SceneManagement;

public class Planet : MonoBehaviour
{
    public FadeController fadeController;
    
    public void Back()
    {
        fadeController.FadeToScene("000_MainMenu");
    }
    
}