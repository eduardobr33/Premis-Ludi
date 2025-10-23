using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using PDollarGestureRecognizer;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SplitZoneGestureRecognizer : MonoBehaviour
{
    [Header("Canvas Panel")]
    public RectTransform drawingPanel;
    
    [Header("Configuration")]
    public Transform gestureOnScreenPrefab;
    public float recognitionDelay = 2.0f;
    public bool showZoneDivider = true;
    
    [Header("Zone Colors")]
    public Color leftZoneColor = Color.red;
    public Color rightZoneColor = Color.blue;
    public Color dividerColor = new Color(1f, 1f, 1f, 0.3f);
    public float lineWidth = 0.1f;

    private List<Gesture> trainingSet = new List<Gesture>();
    
    private DigitZone leftZone;
    private DigitZone rightZone;
    private GameObject dividerLine;
    
    private bool isDrawing = false;
    private bool canDraw = true;
    private float timeSinceLastDraw = 0f;
    private DigitZone currentZone = null;
    
    private Coroutine recognitionTimer;
    private float screenMidpoint;
    
    private bool wasTutorialActive = false;
    private float timeSinceTutorialEnded = 0f;
    private const float TUTORIAL_COOLDOWN = 0.2f;

    private class DigitZone
    {
        public string zoneName;
        public Color zoneColor;
        public List<StrokeData> strokes;
        public float minX;
        public float maxX;
        
        public DigitZone(string name, Color color, float min, float max)
        {
            zoneName = name;
            zoneColor = color;
            minX = min;
            maxX = max;
            strokes = new List<StrokeData>();
        }
        
        public bool ContainsPosition(Vector3 screenPos)
        {
            return screenPos.x >= minX && screenPos.x <= maxX;
        }
        
        public void AddStroke(List<Point> points, LineRenderer visual, int strokeId)
        {
            strokes.Add(new StrokeData(points, visual, strokeId));
        }
        
        public List<Point> GetAllPoints()
        {
            List<Point> allPoints = new List<Point>();
            for (int i = 0; i < strokes.Count; i++)
            {
                foreach (Point p in strokes[i].points)
                {
                    allPoints.Add(new Point(p.X, p.Y, i));
                }
            }
            return allPoints;
        }
        
        public void Clear()
        {
            foreach (StrokeData stroke in strokes)
            {
                if (stroke.visual != null)
                {
                    Object.Destroy(stroke.visual.gameObject);
                }
            }
            strokes.Clear();
        }
    }

    private class StrokeData
    {
        public List<Point> points;
        public LineRenderer visual;
        public int strokeId;
        
        public StrokeData(List<Point> pts, LineRenderer lr, int id)
        {
            points = new List<Point>(pts);
            visual = lr;
            strokeId = id;
        }
    }

    private List<Point> currentStrokePoints = new List<Point>();
    private LineRenderer currentLine;
    private int currentVertexCount = 0;

    void Start()
    {
        LoadGestures();
        SetupZones();
        CreateDivider();
    }

    void LoadGestures()
    {
        TextAsset[] misNumeros = Resources.LoadAll<TextAsset>("GestureSet/MisNumeros/");
        foreach (TextAsset numeroXml in misNumeros)
        {
            trainingSet.Add(GestureIO.ReadGestureFromXML(numeroXml.text));
        }
        
        #if !UNITY_WEBGL || UNITY_EDITOR
        if (System.IO.Directory.Exists(Application.persistentDataPath))
        {
            string[] filePaths = System.IO.Directory.GetFiles(Application.persistentDataPath, "*.xml");
            foreach (string filePath in filePaths)
            {
                trainingSet.Add(GestureIO.ReadGestureFromFile(filePath));
            }
        }
        #endif
    }

    void SetupZones()
    {
        screenMidpoint = Screen.width / 2f;
        leftZone = new DigitZone("IZQUIERDA", leftZoneColor, 0, screenMidpoint);
        rightZone = new DigitZone("DERECHA", rightZoneColor, screenMidpoint, Screen.width);
    }

    void CreateDivider()
    {
        if (!showZoneDivider) 
            return;

        dividerLine = new GameObject("ZoneDivider");
        LineRenderer lr = dividerLine.AddComponent<LineRenderer>();
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.startColor = dividerColor;
        lr.endColor = dividerColor;
        lr.startWidth = 0.05f;
        lr.endWidth = 0.05f;
        lr.positionCount = 2;
        
        Vector3 topPos = Camera.main.ScreenToWorldPoint(new Vector3(screenMidpoint, Screen.height, 10));
        Vector3 bottomPos = Camera.main.ScreenToWorldPoint(new Vector3(screenMidpoint, 0, 10));
        
        lr.SetPosition(0, bottomPos);
        lr.SetPosition(1, topPos);
        lr.sortingOrder = 32766;
        lr.sortingLayerName = "Default";
    }

    bool IsPositionInDrawingArea(Vector3 screenPos)
    {
        if (drawingPanel == null)
            return false;

        GraphicRaycaster raycaster = drawingPanel.GetComponentInParent<GraphicRaycaster>();
        if (raycaster == null)
            return false;

        PointerEventData pointerData = new PointerEventData(EventSystem.current) { position = screenPos };
        List<RaycastResult> results = new List<RaycastResult>();
        raycaster.Raycast(pointerData, results);

        foreach (RaycastResult result in results)
        {
            if (result.gameObject == drawingPanel.gameObject || result.gameObject.transform.IsChildOf(drawingPanel))
                return true;
        }

        return false;
    }

    void Update()
    {
        bool tutorialActive = GameManager.Instance != null && GameManager.Instance.tutorialActive;
        
        // Detectar cuando el tutorial termina
        if (wasTutorialActive && !tutorialActive)
        {
            timeSinceTutorialEnded = 0f;
        }
        wasTutorialActive = tutorialActive;
        
        // Si el tutorial acaba de terminar, esperar el cooldown
        if (timeSinceTutorialEnded < TUTORIAL_COOLDOWN)
        {
            timeSinceTutorialEnded += Time.deltaTime;
            return;
        }
        
        if (!canDraw || tutorialActive) return;

        HandleInput();
        
        if (!isDrawing && (leftZone.strokes.Count > 0 || rightZone.strokes.Count > 0))
        {
            timeSinceLastDraw += Time.deltaTime;
            
            if (timeSinceLastDraw >= recognitionDelay)
            {
                PerformRecognition();
            }
        }
    }

    void HandleInput()
    {
        bool inputDown = Input.GetMouseButtonDown(0) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began);
        bool inputHeld = Input.GetMouseButton(0) || (Input.touchCount > 0 && (Input.GetTouch(0).phase == TouchPhase.Moved || Input.GetTouch(0).phase == TouchPhase.Stationary));
        bool inputUp = Input.GetMouseButtonUp(0) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Ended);
        
        Vector3 inputPosition = GetInputPosition();

        // Verificar si el input está en la zona de exclusión
        if (!IsPositionInDrawingArea(inputPosition))
        {
            if (inputUp && isDrawing)
            {
                FinishStroke();
            }
            return;
        }

        if (inputDown)
        {
            StartNewStroke(inputPosition);
        }
        
        if (inputHeld && isDrawing)
        {
            AddPoint(inputPosition);
        }
        
        if (inputUp && isDrawing)
        {
            FinishStroke();
        }
    }

    Vector3 GetInputPosition()
    {
        if (Input.touchCount > 0)
        {
            return Input.GetTouch(0).position;
        }
        return Input.mousePosition;
    }

    void StartNewStroke(Vector3 screenPos)
    {
        if (recognitionTimer != null)
        {
            StopCoroutine(recognitionTimer);
            recognitionTimer = null;
        }
        
        if (leftZone.ContainsPosition(screenPos))
        {
            currentZone = leftZone;
        }
        else if (rightZone.ContainsPosition(screenPos))
        {
            currentZone = rightZone;
        }
        else
        {
            currentZone = null;
            return;
        }
        
        isDrawing = true;
        currentStrokePoints.Clear();
        currentVertexCount = 0;
        timeSinceLastDraw = 0f;
        
        CreateNewLineRenderer();
    }

    void CreateNewLineRenderer()
    {
        if (gestureOnScreenPrefab == null || currentZone == null) 
            return;

        Transform obj = Instantiate(gestureOnScreenPrefab);
        currentLine = obj.GetComponent<LineRenderer>();
        
        currentLine.startWidth = lineWidth;
        currentLine.endWidth = lineWidth;
        currentLine.startColor = currentZone.zoneColor;
        currentLine.endColor = currentZone.zoneColor;
        currentLine.sortingOrder = 32767;
        currentLine.sortingLayerName = "Default";
    }

    void AddPoint(Vector3 screenPos)
    {
        if (currentZone == null) 
            return;

        currentStrokePoints.Add(new Point(screenPos.x, -screenPos.y, 0));
        
        currentLine.positionCount = ++currentVertexCount;
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, 10));
        currentLine.SetPosition(currentVertexCount - 1, worldPos);
    }

    void FinishStroke()
    {
        isDrawing = false;
        
        if (currentZone == null || currentStrokePoints.Count < 3)
        {
            if (currentLine != null)
                Destroy(currentLine.gameObject);
            currentZone = null;
            return;
        }
        
        int strokeId = currentZone.strokes.Count;
        currentZone.AddStroke(currentStrokePoints, currentLine, strokeId);
        
        currentStrokePoints = new List<Point>();
        currentLine = null;
        currentZone = null;
        timeSinceLastDraw = 0f;
    }

    void PerformRecognition()
    {
        canDraw = false;
        
        // Validar que hay suficientes puntos
        if ((leftZone.strokes.Count == 0 || leftZone.GetAllPoints().Count < 10) &&
            (rightZone.strokes.Count == 0 || rightZone.GetAllPoints().Count < 10))
        {
            ResetSystem();
            return;
        }
        
        try
        {
            StringBuilder result = new StringBuilder();
            
            if (leftZone.strokes.Count > 0 && leftZone.GetAllPoints().Count >= 10)
            {
                List<Point> leftPoints = leftZone.GetAllPoints();
                Point[] leftArray = leftPoints.ToArray();
                
                if (leftArray != null && leftArray.Length >= 10)
                {
                    Gesture leftGesture = new Gesture(leftArray);
                    if (leftGesture.Points != null && leftGesture.Points.Length >= 10)
                    {
                        Result leftResult = PointCloudRecognizer.Classify(leftGesture, trainingSet.ToArray());
                        result.Append(leftResult.GestureClass);
                    }
                }
            }
            
            if (rightZone.strokes.Count > 0 && rightZone.GetAllPoints().Count >= 10)
            {
                List<Point> rightPoints = rightZone.GetAllPoints();
                Point[] rightArray = rightPoints.ToArray();
                
                if (rightArray != null && rightArray.Length >= 10)
                {
                    Gesture rightGesture = new Gesture(rightArray);
                    if (rightGesture.Points != null && rightGesture.Points.Length >= 10)
                    {
                        Result rightResult = PointCloudRecognizer.Classify(rightGesture, trainingSet.ToArray());
                        result.Append(rightResult.GestureClass);
                    }
                }
            }
            
            string finalNumber = result.ToString();
            
            if (finalNumber.Length > 0)
            {
                Debug.Log($"NÚMERO RECONOCIDO: {finalNumber}");
                
                int recognizedNumber;
                if (int.TryParse(finalNumber, out recognizedNumber))
                {
                    if (GameManager.Instance != null && GameManager.Instance.currentEnemy != null)
                    {
                        if (recognizedNumber == GameManager.Instance.currentEnemy.correctAnswer)
                        {
                            Debug.Log("¡Número correcto! El enemigo recibe daño.");
                            GameManager.Instance.currentEnemy.TakeDamage(false);
                            
                            StartCoroutine(CleanupAfterCorrectAnswer());
                            return;
                        }
                        else
                        {
                            Debug.Log($"Número incorrecto. Dibujaste {recognizedNumber}, se esperaba {GameManager.Instance.currentEnemy.correctAnswer}");
                        }
                    }
                }
                else
                {
                    Debug.LogWarning($"El número reconocido ({finalNumber}) no es válido");
                }
            }
            
            StartCoroutine(CleanupAfterDelay());
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Error al reconocer gesto en split zone: {ex.Message}");
            ResetSystem();
        }
    }

    IEnumerator CleanupAfterCorrectAnswer()
    {
        yield return null;
        ResetSystem();
    }

    IEnumerator CleanupAfterDelay()
    {
        yield return new WaitForSeconds(2.5f);
        ResetSystem();
    }

    void ResetSystem()
    {
        leftZone.Clear();
        rightZone.Clear();
        
        if (currentLine != null)
            Destroy(currentLine.gameObject);
        
        currentStrokePoints.Clear();
        currentZone = null;
        isDrawing = false;
        canDraw = true;
        timeSinceLastDraw = 0f;
        currentVertexCount = 0;
        
        if (recognitionTimer != null)
        {
            StopCoroutine(recognitionTimer);
            recognitionTimer = null;
        }
    }

    /// <summary>
    /// Limpia el lienzo de dibujo y reinicia todos los timers.
    /// Ideal para llamar desde un botón cuando el usuario se equivoca.
    /// </summary>
    public void ClearCanvasButton()
    {
        // Cancelar corrutinas pendientes
        if (recognitionTimer != null)
        {
            StopCoroutine(recognitionTimer);
            recognitionTimer = null;
        }
        
        // Limpiar zonas
        leftZone.Clear();
        rightZone.Clear();
        
        // Limpiar línea actual
        if (currentLine != null)
            Destroy(currentLine.gameObject);
        currentLine = null;
        
        // Reiniciar puntos y estado de dibujo
        currentStrokePoints.Clear();
        currentZone = null;
        isDrawing = false;
        currentVertexCount = 0;
        canDraw = true;
        
        // Reiniciar timers
        timeSinceLastDraw = 0f;
        timeSinceTutorialEnded = TUTORIAL_COOLDOWN; // Permite dibujo inmediato después de limpiar
        
        Debug.Log("Lienzo limpiado. Listo para dibujar nuevamente.");
    }

    void OnDestroy()
    {
        if (dividerLine != null)
            Destroy(dividerLine);
    }

    void OnDrawGizmos()
    {
        if (Camera.main == null) return;
        
        float midpoint = Screen.width / 2f;
        
        Gizmos.color = new Color(1f, 0f, 0f, 0.2f);
        Vector3 leftCenter = Camera.main.ScreenToWorldPoint(new Vector3(midpoint / 2f, Screen.height / 2f, 10));
        Vector3 leftSize = Camera.main.ScreenToWorldPoint(new Vector3(midpoint, Screen.height, 10)) - 
                          Camera.main.ScreenToWorldPoint(new Vector3(0, 0, 10));
        Gizmos.DrawCube(leftCenter, leftSize);
        
        Gizmos.color = new Color(0f, 0f, 1f, 0.2f);
        Vector3 rightCenter = Camera.main.ScreenToWorldPoint(new Vector3(midpoint + midpoint / 2f, Screen.height / 2f, 10));
        Vector3 rightSize = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, 10)) - 
                           Camera.main.ScreenToWorldPoint(new Vector3(midpoint, 0, 10));
        Gizmos.DrawCube(rightCenter, rightSize);
        
        Gizmos.color = Color.white;
        Vector3 top = Camera.main.ScreenToWorldPoint(new Vector3(midpoint, Screen.height, 10));
        Vector3 bottom = Camera.main.ScreenToWorldPoint(new Vector3(midpoint, 0, 10));
        Gizmos.DrawLine(bottom, top);
    }
}