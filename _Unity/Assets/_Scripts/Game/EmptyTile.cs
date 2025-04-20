using UnityEngine;

public class EmptyTile : MonoBehaviour
{
    private Material mat;
    private static readonly int ColorID = Shader.PropertyToID("_Color");
    public event System.Action OnClick;
    
    
    public void SetAlpha(float alpha)
    {
        if (!mat.HasProperty(ColorID)) return;
    
        Color c = mat.color;
        c.a = Mathf.Clamp01(alpha);
        mat.SetColor(ColorID, c);
    }


    // Init 메서드로 이미지를 받아와서 처리
    public void Init(Texture2D puzzleImage, int width, int height, int x, int y)
    {
        Renderer renderer = GetComponent<Renderer>();
        mat = renderer.material;

        // 텍스처 설정
        mat.mainTexture = puzzleImage;
        mat.mainTextureScale = new Vector2(1f / width, 1f / height);
        mat.mainTextureOffset = new Vector2(
            1f / width * x,
            1f - 1f / height * (y + 1)
        );

        // 빈 칸으로 설정
        if (mat.HasProperty(ColorID))
            mat.SetColor(ColorID, new Color(1f, 1f, 1f, 0f)); // 투명하게 설정
    }

    void OnMouseDown()
    {
        OnClick?.Invoke();
    }

    // 빈 칸은 복원되지 않으므로 별다른 동작은 필요 없음
    public void Restore() { }
}
