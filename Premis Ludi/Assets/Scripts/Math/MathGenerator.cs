using UnityEngine;
using System;

public static class MathGenerator
{
    public static (string operation, int result) GenerateOperation(LevelData levelData)
    {
        if (levelData == null) return ("0+0", 0);

        int steps = UnityEngine.Random.Range(1, levelData.maxSteps + 1);

        int currentValue = UnityEngine.Random.Range(levelData.numberRange.x, levelData.numberRange.y + 1);
        string expression = currentValue.ToString();

        for (int i = 0; i < steps; i++)
        {
            string op = levelData.allowedOperations[UnityEngine.Random.Range(0, levelData.allowedOperations.Count)];
            int num = UnityEngine.Random.Range(levelData.numberRange.x, levelData.numberRange.y + 1);

            switch (op)
            {
                case "add":
                    expression += " + " + num;
                    currentValue += num;
                    break;

                case "sub":
                    // Avoid negative numbers
                    if (num > currentValue)
                        num = UnityEngine.Random.Range(0, currentValue + 1);

                    expression += " - " + num;
                    currentValue -= num;
                    break;

                case "mul":
                    expression += " x " + num;
                    currentValue *= num;
                    break;

                case "div":
                    // Avoid non-integrer or 0 divisions
                    if (num != 0 && currentValue % num == 0)
                    {
                        expression += " ÷ " + num;
                        currentValue /= num;
                    }
                    break;
            }
        }

        return (expression, currentValue);
    }

}
