using UnityEngine;

public class FriendListManager : MonoBehaviour
{
    public FadeController fadeController;


    public void Back()
    {
        fadeController.FadeToScene("000_MainMenu");
    }
}