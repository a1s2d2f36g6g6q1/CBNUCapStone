using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FriendSceneChanger : MonoBehaviour
{
    public void GoToFriendScene()
    {
        SceneManager.LoadScene("F001_Friend");
    }
}



