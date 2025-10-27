using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Text;

public class MultiplicationTablesUI : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI tableText;
    
    [Header("Tabs (2–9)")]
    public Button[] tableButtons;

    private int currentTable = 2;

    private void Start()
    {
        for (int i = 0; i < tableButtons.Length; i++)
        {
            int tableNum = i + 2;
            int captured = tableNum;
            tableButtons[i].onClick.AddListener(() => ShowTable(captured));
        }

        ShowTable(currentTable);
    }

    private void ShowTable(int number)
    {
        currentTable = number;

        StringBuilder sb = new StringBuilder();

        for (int i = 2; i <= 5; i++)
        {
            int j = i + 4;
            string left = $"{number} x {i} = {number * i}";
            string right = $"{number} x {j} = {number * j}";
            sb.AppendLine($"{left,-12}\t{right}");
        }

        tableText.text = sb.ToString();

        HighlightActiveButton(number);
    }

    private void HighlightActiveButton(int activeNumber)
    {
        for (int i = 0; i < tableButtons.Length; i++)
        {
            int num = i + 2;
            ColorBlock colors = tableButtons[i].colors;
            colors.normalColor = (num == activeNumber) ? new Color(1f, 0.85f, 0.4f) : Color.white;
            tableButtons[i].colors = colors;
        }
    }
}
