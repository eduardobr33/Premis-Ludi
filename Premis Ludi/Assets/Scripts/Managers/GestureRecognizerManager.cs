using UnityEngine;

public class GestureRecognizerManager : MonoBehaviour
{
    [SerializeField] private SimpleGestureRecognizer simpleRecognizer;
    [SerializeField] private SplitZoneGestureRecognizer splitZoneRecognizer;

    private void OnValidate()
    {
        if (simpleRecognizer == null)
            simpleRecognizer = FindObjectOfType<SimpleGestureRecognizer>();
        if (splitZoneRecognizer == null)
            splitZoneRecognizer = FindObjectOfType<SplitZoneGestureRecognizer>();
    }

    public void ClearActiveGestureRecognizer()
    {
        if (simpleRecognizer.isActiveAndEnabled)
            simpleRecognizer.ClearCanvasButton();
        else if (splitZoneRecognizer.isActiveAndEnabled)
            splitZoneRecognizer.ClearCanvasButton();
    }
}
