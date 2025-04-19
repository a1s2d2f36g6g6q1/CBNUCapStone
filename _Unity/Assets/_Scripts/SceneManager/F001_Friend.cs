using UnityEngine;
using TMPro;

public class FriendListManager : MonoBehaviour
{
    public FadeController fadeController;

    
    public void Back()
    {
        fadeController.FadeToScene("000_MainMenu");
    }
}
