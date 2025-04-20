using TMPro;
using UnityEngine;

public class Game : MonoBehaviour
{
    public TMP_InputField[] inputFields;
    public FadeController fadeController;

    public void Back()
    {
        fadeController.FadeToScene("G001_TagInput");
    }
}