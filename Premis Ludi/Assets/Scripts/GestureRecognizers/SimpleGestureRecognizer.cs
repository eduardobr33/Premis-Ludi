using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using PDollarGestureRecognizer;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SimpleGestureRecognizer : MonoBehaviour
{
    [Header("Canvas Panel")]
    public RectTransform drawingPanel;
    
    [Header("Configuration")]
    public Transform gestureOnScreenPrefab;
    public float recognitionDelay = 3f;

    private List<Gesture> trainingSet = new List<Gesture>();
    private List<Point> points = new List<Point>();
    private int strokeId = -1;
    
    private bool isDrawing = false;
    private bool canDraw = true;
    private Vector3 mousePosition;
    private float timeSinceLastDraw = 0f;
    
    private List<LineRenderer> gestureLines = new List<LineRenderer>();
    private LineRenderer currentLineRenderer;
    private int vertexCount = 0;
    
    private Coroutine recognitionCoroutine;
    
    private bool wasTutorialActive = false;
    private float timeSinceTutorialEnded = 0f;
    private const float TUTORIAL_COOLDOWN = 0.2f;

    void Start()
    {
        LoadGestures();
    }

    void LoadGestures()
    {
        TextAsset[] misNumeros = Resources.LoadAll<TextAsset>("GestureSet/MisNumeros/");
        foreach (TextAsset numeroXml in misNumeros)
        {
            trainingSet.Add(GestureIO.ReadGestureFromXML(numeroXml.text));
        }
        
        if (System.IO.Directory.Exists(Application.persistentDataPath))
        {
            string[] filePaths = System.IO.Directory.GetFiles(Application.persistentDataPath, "*.xml");
            foreach (string filePath in filePaths)
            {
                trainingSet.Add(GestureIO.ReadGestureFromFile(filePath));
            }
        }
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
        
        if (!isDrawing && gestureLines.Count > 0)
        {
            timeSinceLastDraw += Time.deltaTime;
            
            if (timeSinceLastDraw >= recognitionDelay)
            {
                if (points.Count >= 10)
                    RecognizeGesture();
                else
                    ClearGesture(); // Limpia si no hay suficientes puntos
            }
        }
    }

    void HandleInput()
    {
        bool inputDown = Input.GetMouseButtonDown(0) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began);
        bool inputHeld = Input.GetMouseButton(0) || (Input.touchCount > 0 && (Input.GetTouch(0).phase == TouchPhase.Moved || Input.GetTouch(0).phase == TouchPhase.Stationary));
        bool inputUp = Input.GetMouseButtonUp(0) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Ended);
        
        Vector3 inputPosition = GetInputPosition();

        if (!IsPositionInDrawingArea(inputPosition))
        {
            if (inputUp && isDrawing)
                StopDrawing();
            return;
        }
        
        if (inputDown)
            StartDrawing();
        
        if (inputHeld && isDrawing)
            ContinueDrawing();
        
        if (inputUp && isDrawing)
            StopDrawing();
    }

    void StartDrawing()
    {
        if (!isDrawing && gestureLines.Count == 0)
        {
            points.Clear();
            strokeId = -1;
        }

        strokeId++;
        isDrawing = true;
        vertexCount = 0;
        timeSinceLastDraw = 0f;
        mousePosition = GetInputPosition();
        
        Transform gestureObj = Instantiate(gestureOnScreenPrefab);
        currentLineRenderer = gestureObj.GetComponent<LineRenderer>();
        
        currentLineRenderer.sortingOrder = 32767;
        currentLineRenderer.sortingLayerName = "Default";
        gestureLines.Add(currentLineRenderer);
    }

    void ContinueDrawing()
    {
        mousePosition = GetInputPosition();
        points.Add(new Point(mousePosition.x, -mousePosition.y, strokeId));
        
        currentLineRenderer.positionCount = ++vertexCount;
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, 10));
        currentLineRenderer.SetPosition(vertexCount - 1, worldPos);
    }

    void StopDrawing()
    {
        isDrawing = false;
        currentLineRenderer = null;
        timeSinceLastDraw = 0f;
    }

    Vector3 GetInputPosition()
    {
        if (Input.touchCount > 0)
        {
            return Input.GetTouch(0).position;
        }
        return Input.mousePosition;
    }

    void RecognizeGesture()
    {
        if (points.Count < 10 || trainingSet.Count == 0) 
        {
            ClearGesture();
            return;
        }
        
        canDraw = false;
        
        try
        {
            Point[] pointArray = points.ToArray();
            if (pointArray == null || pointArray.Length < 10)
            {
                ClearGesture();
                return;
            }
            
            Gesture candidate = new Gesture(pointArray);
            if (candidate.Points == null || candidate.Points.Length < 10)
            {
                ClearGesture();
                return;
            }
            
            Result gestureResult = PointCloudRecognizer.Classify(candidate, trainingSet.ToArray());
            
            Debug.Log($"GESTO RECONOCIDO: {gestureResult.GestureClass} (Confianza: {gestureResult.Score:F2})");

            int recognizedNumber;
            if (int.TryParse(gestureResult.GestureClass, out recognizedNumber))
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
                Debug.LogWarning($"El gesto reconocido ({gestureResult.GestureClass}) no es un número válido");
            }
            
            StartCoroutine(CleanupAfterIncorrectAnswer());
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Error al reconocer gesto: {ex.Message}");
            ClearGesture();
        }
    }

    IEnumerator CleanupAfterIncorrectAnswer()
    {
        yield return new WaitForSeconds(1f);
        ResetForNewGesture();
    }

    IEnumerator CleanupAfterCorrectAnswer()
    {
        yield return null;
        ResetForNewGesture();
    }

    void ResetForNewGesture()
    {
        foreach (LineRenderer line in gestureLines)
        {
            if (line != null)
                Destroy(line.gameObject);
        }
        gestureLines.Clear();
        
        currentLineRenderer = null;
        points.Clear();
        strokeId = -1;
        isDrawing = false;
        canDraw = true;
        vertexCount = 0;
        timeSinceLastDraw = 0f;
    }

    void ClearGesture()
    {
        foreach (LineRenderer line in gestureLines)
        {
            Destroy(line.gameObject);
        }
        gestureLines.Clear();
        points.Clear();
        strokeId = -1;
        isDrawing = false;
        vertexCount = 0;
        timeSinceLastDraw = 0f;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Vector3 bottomLeft = Camera.main.ScreenToWorldPoint(new Vector3(0, 0, 10));
        Vector3 topRight = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, 10));
        Gizmos.DrawWireCube((bottomLeft + topRight) / 2, topRight - bottomLeft);
    }
}