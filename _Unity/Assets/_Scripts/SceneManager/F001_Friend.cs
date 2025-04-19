using UnityEngine;
using TMPro;

public class FriendListManager : MonoBehaviour
{
    public FadeController fadeController;

    
    public void Back()
    {
        Debug.Log("뒤로가기 (메인메뉴)");
        fadeController.FadeToScene("000_MainMenu");
    }
}
