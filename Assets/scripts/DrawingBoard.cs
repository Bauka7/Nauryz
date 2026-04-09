using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic; // Нужно для работы со списками (историей шагов)

public class DrawingBoard : MonoBehaviour, IDragHandler, IPointerDownHandler
{
    [Header("Настройки кисти")]
    public RawImage rawImage;
    public int textureWidth = 1024;
    public int textureHeight = 1024;
    public int brushSize = 8;
    public Color brushColor = Color.black;

    [Header("Связь с 3D миром")]
    public MeshRenderer target3DCanvas; 

    [Header("Настройки Отмены (Undo)")]
    public int maxUndoSteps = 10; // Сколько шагов назад можно отменить (чтобы не забивать память)
    private List<Color[]> undoHistory = new List<Color[]>(); // Хранилище старых состояний холста

    private Texture2D drawTexture;
    private Material runtimeCanvasMaterial;

    void Start()
    {
        drawTexture = new Texture2D(textureWidth, textureHeight, TextureFormat.RGBA32, false);
        drawTexture.name = "DrawingBoardRuntimeTexture";
        drawTexture.wrapMode = TextureWrapMode.Clamp;
        drawTexture.filterMode = FilterMode.Bilinear;
        Color[] whitePixels = new Color[textureWidth * textureHeight];
        for (int i = 0; i < whitePixels.Length; i++) whitePixels[i] = Color.white;
        
        drawTexture.SetPixels(whitePixels);
        drawTexture.Apply();
        
        rawImage.texture = drawTexture;

        Setup3DCanvasMaterial();
        ApplyTextureTo3DCanvas();
        
        // Сохраняем самый первый (чистый) холст в историю
        SaveState(); 
    }

    void Setup3DCanvasMaterial()
    {
        if (target3DCanvas == null) return;

        Material sourceMaterial = target3DCanvas.sharedMaterial;
        if (sourceMaterial != null)
        {
            runtimeCanvasMaterial = new Material(sourceMaterial);
        }
        else
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");
            if (shader == null) shader = Shader.Find("Unlit/Texture");

            if (shader == null)
            {
                Debug.LogWarning("DrawingBoard: no suitable shader found for the 3D canvas.");
                return;
            }

            runtimeCanvasMaterial = new Material(shader);
        }

        if (runtimeCanvasMaterial.HasProperty("_BaseColor"))
            runtimeCanvasMaterial.SetColor("_BaseColor", Color.white);

        if (runtimeCanvasMaterial.HasProperty("_Color"))
            runtimeCanvasMaterial.SetColor("_Color", Color.white);

        target3DCanvas.material = runtimeCanvasMaterial;
    }

    void ApplyTextureTo3DCanvas()
    {
        if (runtimeCanvasMaterial == null || drawTexture == null) return;

        if (runtimeCanvasMaterial.HasProperty("_BaseMap"))
            runtimeCanvasMaterial.SetTexture("_BaseMap", drawTexture);

        if (runtimeCanvasMaterial.HasProperty("_MainTex"))
            runtimeCanvasMaterial.SetTexture("_MainTex", drawTexture);

        runtimeCanvasMaterial.mainTexture = drawTexture;
    }

    // --- Функция смены цвета ---
    public void SetColor(string colorName)
    {
        switch (colorName.ToLower())
        {
            case "red": brushColor = Color.red; break;
            case "green": brushColor = Color.green; break;
            case "blue": brushColor = Color.blue; break;
            case "black": brushColor = Color.black; break;
        }
    }

    // --- Функции сохранения и отмены (Undo) ---
    void SaveState()
    {
        // Копируем текущие пиксели холста
        Color[] currentPixels = drawTexture.GetPixels();
        undoHistory.Add(currentPixels);
        
        // Если шагов больше разрешенного, удаляем самую старую запись
        if (undoHistory.Count > maxUndoSteps + 1)
        {
            undoHistory.RemoveAt(0); 
        }
    }

    public void Undo()
    {
        // Если в истории больше одного шага (нельзя отменить то, чего не было)
        if (undoHistory.Count > 1) 
        {
            // Удаляем текущее испорченное состояние
            undoHistory.RemoveAt(undoHistory.Count - 1);
            
            // Берем предыдущее состояние
            Color[] lastState = undoHistory[undoHistory.Count - 1];
            
            // Применяем его к текстуре
            drawTexture.SetPixels(lastState);
            drawTexture.Apply();
            ApplyTextureTo3DCanvas();
        }
    }

    // --- Рисование ---
    public void OnPointerDown(PointerEventData eventData) 
    { 
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            // Сохраняем состояние ДО того, как начали вести линию
            SaveState(); 
            Draw(eventData); 
        }
    }
    
    public void OnDrag(PointerEventData eventData) 
    { 
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            Draw(eventData); 
        }
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
        drawTexture.Apply(); 
        ApplyTextureTo3DCanvas();
    }

    void OnDestroy()
    {
        if (runtimeCanvasMaterial != null)
        {
            Destroy(runtimeCanvasMaterial);
        }

        if (drawTexture != null)
        {
            Destroy(drawTexture);
        }
    }
}
