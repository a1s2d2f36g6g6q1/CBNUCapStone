using UnityEngine;
using UnityEngine.UI;

public class GridContentResizer : MonoBehaviour
{
    public GridLayoutGroup gridLayout;
    public RectTransform contentRect;

    public void AdjustContentSize()
    {
        var padding = gridLayout.padding;
        var spacing = gridLayout.spacing;
        var cellSize = gridLayout.cellSize;
        int columnCount = gridLayout.constraintCount;

        int itemCount = contentRect.childCount;
        int rowCount = Mathf.CeilToInt(itemCount / (float)columnCount);

        float width = padding.left + padding.right + (cellSize.x * columnCount) + (spacing.x * (columnCount - 1));
        float height = padding.top + padding.bottom + (cellSize.y * rowCount) + (spacing.y * (rowCount - 1));

        contentRect.sizeDelta = new Vector2(width, height);
    }
}