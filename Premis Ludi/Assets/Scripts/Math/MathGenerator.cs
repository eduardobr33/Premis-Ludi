using UnityEngine;

public class MathGenerator : MonoBehaviour
{
    public static (string, int) GenerateOperation(int difficulty)
    {
        int a, b, result;
        string op;

        switch (difficulty)
        {
            case 1:
                a = Random.Range(1, 6);
                b = Random.Range(1, 6);
                result = a + b;
                op = $"{a} + {b}";
                break;

            case 2:
                a = Random.Range(1, 11);
                b = Random.Range(1, a);     // To avoid negative numbers b can't be bigger than a
                result = a - b;
                op = $"{a} - {b}";
                break;

            default:
                a = Random.Range(1, 10);
                b = Random.Range(1, 10);
                result = a * b;
                op = $"{a} x {b}";
                break;
        }

        return (op, result);
    }
}
