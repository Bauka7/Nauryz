using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class DrawingBoard : MonoBehaviour, IDragHandler, IPointerDownHandler
{
    [Header("Настройки кисти")]
    public RawImage rawImage;
    public int textureWidth = 1024;
    public int textureHeight = 1024;
    public int brushSize = 8;
    public Color brushColor = Color.black;

    [Header("Связь с 3D миром")]
    [Tooltip("Перетащите сюда 3D-объект холста, на котором должен появляться рисунок")]
    public MeshRenderer target3DCanvas; // Ссылка на 3D холст

    private Texture2D drawTexture;

    void Start()
    {
        // Создаем текстуру
        drawTexture = new Texture2D(textureWidth, textureHeight);
        Color[] whitePixels = new Color[textureWidth * textureHeight];
        for (int i = 0; i < whitePixels.Length; i++) whitePixels[i] = Color.white;
        drawTexture.SetPixels(whitePixels);
        drawTexture.Apply();

        // Применяем к UI
        rawImage.texture = drawTexture;

        // НОВОЕ: Передаем эту же текстуру 3D-объекту!
        if (target3DCanvas != null)
        {
            // Устанавливаем текстуру как основную для материала 3D объекта
            target3DCanvas.material.mainTexture = drawTexture;
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left) Draw(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left) Draw(eventData);
    }

    void Draw(PointerEventData eventData)
    {
        if (rawImage == null) return;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rawImage.rectTransform, eventData.position, eventData.pressEventCamera, out Vector2 localCursor);

        Rect r = rawImage.rectTransform.rect;
        float px = Mathf.Clamp(0, (localCursor.x - r.x) * textureWidth / r.width, textureWidth);
        float py = Mathf.Clamp(0, (localCursor.y - r.y) * textureHeight / r.height, textureHeight);

        DrawBrush((int)px, (int)py);
    }

    void DrawBrush(int x, int y)
    {
        for (int i = -brushSize; i <= brushSize; i++)
        {
            for (int j = -brushSize; j <= brushSize; j++)
            {
                if (i * i + j * j <= brushSize * brushSize)
                {
                    int px = x + i;
                    int py = y + j;
                    if (px >= 0 && px < textureWidth && py >= 0 && py < textureHeight)
                    {
                        drawTexture.SetPixel(px, py, brushColor);
                    }
                }
            }
        }
        // Когда мы применяем изменения, они автоматически обновятся и в UI, и на 3D модели!
        drawTexture.Apply();
    }
}