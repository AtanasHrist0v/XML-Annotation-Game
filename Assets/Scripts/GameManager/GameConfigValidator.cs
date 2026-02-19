using System.Collections.Generic;
using UnityEngine;

public class GameConfigValidator
{
    public static bool ValidateConfig(GameConfig config)
    {
        bool isValid = true;

        if (config == null)
        {
            Debug.LogError("GameConfig is null!");
            return false;
        }

        if (!ValidateModels(config.Models))
        {
            isValid = false;
        }

        if (!ValidateLevels(config.Levels, config.Models))
        {
            isValid = false;
        }

        if (!ValidateScoring(config.Scoring))
        {
            isValid = false;
        }

        if (isValid)
        {
            Debug.Log("Configuration is valid!");
        }

        return isValid;
    }

    private static bool ValidateModels(Dictionary<string, ModelData> models)
    {
        bool isValid = true;

        if (models == null || models.Count == 0)
        {
            Debug.LogError(" No models loaded!");
            return false;
        }

        foreach (var kvp in models)
        {
            ModelData model = kvp.Value;

            if (string.IsNullOrEmpty(model.Id))
            {
                Debug.LogError("Model with empty ID!");
                isValid = false;
            }

            if (string.IsNullOrEmpty(model.Prefab))
            {
                Debug.LogWarning($"Model '{model.Id}' has no prefab assigned!");
            }

            if (model.Annotations == null || model.Annotations.Count == 0)
            {
                Debug.LogWarning($"Model '{model.Id}' has no annotations!");
            }
            else
            {
                if (!model.Annotations.ContainsKey("name"))
                {
                    Debug.LogWarning($"Model '{model.Id}' has no 'name' annotation!");
                }
            }

            if (model.Transparency < 0f || model.Transparency > 1f)
            {
                Debug.LogWarning($"Model '{model.Id}' has invalid transparency: {model.Transparency} (must be 0-1)");
            }
        }

        return isValid;
    }

    private static bool ValidateLevels(List<LevelData> levels, Dictionary<string, ModelData> models)
    {
        bool isValid = true;

        if (levels == null || levels.Count == 0)
        {
            Debug.LogError("No levels loaded!");
            return false;
        }

        HashSet<int> usedIndices = new HashSet<int>();

        foreach (LevelData level in levels)
        {
            if (usedIndices.Contains(level.Index))
            {
                Debug.LogError($"Duplicate level index: {level.Index}");
                isValid = false;
            }
            usedIndices.Add(level.Index);

            if (string.IsNullOrEmpty(level.Title))
            {
                Debug.LogWarning($"Level {level.Index} has no title!");
            }

            if (level.Pairs == null || level.Pairs.Count == 0)
            {
                Debug.LogError($"Level {level.Index} has no pairs!");
                isValid = false;
                continue;
            }

            HashSet<string> usedPairs = new HashSet<string>();

            foreach (PairData pair in level.Pairs)
            {
 
                if (!models.ContainsKey(pair.ModelId))
                {
                    Debug.LogError($"Level {level.Index}: ModelId '{pair.ModelId}' does not exist!");
                    isValid = false;
                    continue;
                }

                ModelData model = models[pair.ModelId];

                if (!model.Annotations.ContainsKey(pair.AnnotationKey))
                {
                    Debug.LogError($"Level {level.Index}: Model '{pair.ModelId}' has no annotation key '{pair.AnnotationKey}'!");
                    isValid = false;
                }

                string pairKey = $"{pair.ModelId}:{pair.AnnotationKey}";
                if (usedPairs.Contains(pairKey))
                {
                    Debug.LogWarning($"Level {level.Index}: Duplicate pair {pairKey}");
                }
                usedPairs.Add(pairKey);
            }
        }

        return isValid;
    }

    private static bool ValidateScoring(ScoringSettings scoring)
    {
        bool isValid = true;

        if (scoring == null)
        {
            Debug.LogWarning("No scoring settings defined! Default values will be used.");
            return true; 
        }

        if (scoring.CorrectPoints <= 0)
        {
            Debug.LogWarning($"Points for correct answer are {scoring.CorrectPoints} (usually positive)");
        }

        if (scoring.IncorrectPoints >= 0)
        {
            Debug.LogWarning($"Points for incorrect answer are {scoring.IncorrectPoints} (usually negative)");
        }

        return isValid;
    }

    public static void PrintStatistics(GameConfig config)
    {
        Debug.Log("========== GAME STATISTICS ==========");
        Debug.Log($"Total models: {config.Models.Count}");
        Debug.Log($"Total levels: {config.Levels.Count}");

        int totalPairs = 0;
        foreach (var level in config.Levels)
        {
            totalPairs += level.Pairs.Count;
            Debug.Log($"  - Level {level.Index} ({level.Title}): {level.Pairs.Count} pairs");
        }

        Debug.Log($"Total pairs: {totalPairs}");
        Debug.Log($"Scoring: +{config.Scoring.CorrectPoints} for correct, {config.Scoring.IncorrectPoints} for incorrect");
        Debug.Log("=========================================");
    }
}
