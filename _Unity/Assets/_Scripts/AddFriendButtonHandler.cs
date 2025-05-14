using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AddFriendButtonHandler : MonoBehaviour
{
    public PanelHandler popupWindow;

    public void OnButtonClick()
    {
        popupWindow.Show();
    }
}