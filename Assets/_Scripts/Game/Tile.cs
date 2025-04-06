using UnityEngine;
using TMPro;

public class Tile : MonoBehaviour
{
    public PuzzleManager manager;
    public int x, y;
    public int number;

    public void SetNumber(int num)
    {
        number = num;
        GetComponentInChildren<TextMeshProUGUI>().text = number.ToString();
    }

    public void SetPosition(int newX, int newY)
    {
        x = newX;
        y = newY;
        transform.SetSiblingIndex(y * manager.gridSize + x);
    }

    public void OnClick()
    {
        manager.TryMoveTile(x, y);
    }
}