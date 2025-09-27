using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using PDollarGestureRecognizer;

public class SimpleGestureRecognizer : MonoBehaviour
{
    [Header("Configuration")]
    public Transform gestureOnScreenPrefab;
    public float recognitionDelay = 3f;

    private List<Gesture> trainingSet = new List<Gesture>();
    private List<Point> points = new List<Point>();
    private int strokeId = -1;
    
    private bool isDrawing = false;
    private bool canDraw = true;
    private Vector3 mousePosition;
    
    private LineRenderer currentLineRenderer;
    private int vertexCount = 0;
    
    private Coroutine recognitionCoroutine;

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
            Debug.Log($"Cargados {filePaths.Length} gestos desde memoria persistente");
        }
        
        Debug.Log($"TOTAL: {trainingSet.Count} gestos cargados para reconocimiento");
        Debug.Log($"Ruta de memoria persistente: {Application.persistentDataPath}");
    }

    void Update()
    {
        if (!canDraw) return;

        HandleInput();
    }

    void HandleInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            StartDrawing();
        }
        
        if (Input.GetMouseButton(0) && isDrawing)
        {
            ContinueDrawing();
        }
        
        if (Input.GetMouseButtonUp(0) && isDrawing)
        {
            StopDrawing();
        }
    }

    void StartDrawing()
    {
        if (recognitionCoroutine != null)
        {
            StopCoroutine(recognitionCoroutine);
        }

        points.Clear();
        strokeId = 0;
        isDrawing = true;
        vertexCount = 0;
        
        mousePosition = Input.mousePosition;
        
        if (gestureOnScreenPrefab != null)
        {
            Transform gestureObj = Instantiate(gestureOnScreenPrefab);
            currentLineRenderer = gestureObj.GetComponent<LineRenderer>();
        }
        
        Debug.Log("Comenzando a dibujar gesto...");
        
        recognitionCoroutine = StartCoroutine(RecognizeAfterDelay());
    }

    void ContinueDrawing()
    {
        mousePosition = Input.mousePosition;
        
        points.Add(new Point(mousePosition.x, -mousePosition.y, strokeId));
        
        if (currentLineRenderer != null)
        {
            currentLineRenderer.positionCount = ++vertexCount;
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, 10));
            currentLineRenderer.SetPosition(vertexCount - 1, worldPos);
        }
    }

    void StopDrawing()
    {
        isDrawing = false;
        Debug.Log($"Gesto terminado con {points.Count} puntos");
    }

    IEnumerator RecognizeAfterDelay()
    {
        yield return new WaitForSeconds(recognitionDelay);
        
        if (points.Count > 0)
        {
            RecognizeGesture();
        }
        
        yield return new WaitForSeconds(1f);
        ResetForNewGesture();
    }

    void RecognizeGesture()
    {
        if (points.Count == 0)
        {
            Debug.Log("No hay puntos para reconocer");
            return;
        }
        
        Gesture candidate = new Gesture(points.ToArray());
        Result gestureResult = PointCloudRecognizer.Classify(candidate, trainingSet.ToArray());
        
        Debug.Log($"GESTO RECONOCIDO: {gestureResult.GestureClass} (Confianza: {gestureResult.Score:F2})");
        
        canDraw = false;
    }

    void ResetForNewGesture()
    {
        if (currentLineRenderer != null)
        {
            Destroy(currentLineRenderer.gameObject);
            currentLineRenderer = null;
        }
        
        points.Clear();
        strokeId = -1;
        isDrawing = false;
        canDraw = true;
        vertexCount = 0;
        
        Debug.Log("Listo para nuevo gesto. Haz clic y dibuja.");
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Vector3 bottomLeft = Camera.main.ScreenToWorldPoint(new Vector3(0, 0, 10));
        Vector3 topRight = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, 10));
        Gizmos.DrawWireCube((bottomLeft + topRight) / 2, topRight - bottomLeft);
    }
}