using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml;
using System.Xml.Linq;
using UnityEngine;

public class GameXMLParser : MonoBehaviour
{
    [Header("XML Configuration")]
    [SerializeField] private TextAsset builtInXmlFile;

    private GameConfig gameConfig = null;

    public void LoadXML() {
        if (1 > 0) {
            string[] files = System.IO.Directory.GetFiles(Application.dataPath, "*.xml");
            Debug.Log(Application.dataPath);

            if (files.Length != 0) {
                string filePath = files[0];
                Debug.Log(filePath);

                XDocument xmlDoc = XDocument.Load(filePath);
                Debug.Log(xmlDoc);

                gameConfig = ParseXML(xmlDoc.ToString());
            } else {
                Debug.Log("files has 0 len");
            }
        }

        if (gameConfig != null) return;

        if (builtInXmlFile == null) {
            Debug.LogError("XML file is not attached in Inspector!");
            return;
        }

        gameConfig = ParseXML(builtInXmlFile.text);

        if (gameConfig != null) {
            Debug.Log($"XML parsed successfully! Loaded {gameConfig.Models.Count} models and {gameConfig.Levels.Count} levels.");
            ApplyGameSettings();
        }
    }

    private GameConfig ParseXML(string xmlContent)
    {
        GameConfig config = new GameConfig();

        try
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xmlContent);

            XmlNode gameNode = xmlDoc.SelectSingleNode("/Game");
            if (gameNode == null)
            {
                Debug.LogError("Root <Game> element not found!");
                return null;
            }
            ParseSettings(gameNode.SelectSingleNode("Settings"), config);
            ParseModels(gameNode.SelectSingleNode("Models"), config);
            ParseLevels(gameNode.SelectSingleNode("Levels"), config);
            ParseValidation(gameNode.SelectSingleNode("Validation"), config);
            ParseFeedback(gameNode.SelectSingleNode("Feedback"), config);

            return config;
        }
        catch (Exception e)
        {
            Debug.LogError($"Error parsing XML: {e.Message}");
            return null;
        }
    }

    private void ParseSettings(XmlNode settingsNode, GameConfig config)
    {
        if (settingsNode == null) return;

        XmlNode hudNode = settingsNode.SelectSingleNode("HUD");
        if (hudNode != null)
        {
            config.HUD = new HUDSettings
            {
                Enabled = GetBoolAttribute(hudNode, "enabled", true),
                Anchor = GetStringAttribute(hudNode, "anchor", "TopRight"),
                ShowScore = GetBoolAttribute(hudNode, "showScore", true),
                ShowTime = GetBoolAttribute(hudNode, "showTime", true),
                TimeFormat = GetStringAttribute(hudNode, "timeFormat", "seconds"),
                ShowLevel = GetBoolAttribute(hudNode, "showLevel", true)
            };
        }
        XmlNode scoringNode = settingsNode.SelectSingleNode("Scoring");
        if (scoringNode != null)
        {
            config.Scoring = new ScoringSettings
            {
                CorrectPoints = GetIntAttribute(scoringNode, "correctPoints", 1),
                IncorrectPoints = GetIntAttribute(scoringNode, "incorrectPoints", -1)
            };
        }

        XmlNode roomNode = settingsNode.SelectSingleNode("Room");
        if (roomNode != null)
        {
            config.RoomCenter = ParseVector3(GetStringAttribute(roomNode, "center", "0,0,0"));
        }

        XmlNode boardNode = settingsNode.SelectSingleNode("Board");
        if (boardNode != null)
        {
            config.Board = new BoardSettings
            {
                Id = GetStringAttribute(boardNode, "id", "mainBoard"),
                Prefab = GetStringAttribute(boardNode, "prefab", ""),
                Position = ParseVector3(GetStringAttribute(boardNode, "pos", "0,0,0")),
                Rotation = ParseVector3(GetStringAttribute(boardNode, "rot", "0,0,0")),
                Scale = ParseVector3(GetStringAttribute(boardNode, "scale", "1,1,1")),
                Columns = GetIntAttribute(boardNode, "columns", 2),
                CardSpacingX = GetFloatAttribute(boardNode, "cardSpacingX", 0.35f),
                CardSpacingY = GetFloatAttribute(boardNode, "cardSpacingY", 0.22f)
            };
        }
    }

    private void ParseModels(XmlNode modelsNode, GameConfig config)
    {
        if (modelsNode == null) return;

        foreach (XmlNode modelNode in modelsNode.SelectNodes("Model"))
        {
            ModelData model = new ModelData
            {
                Id = GetStringAttribute(modelNode, "id", ""),
                Name = GetStringAttribute(modelNode, "name", ""),
                Prefab = GetStringAttribute(modelNode, "prefab", "")
            };

            XmlNode transformNode = modelNode.SelectSingleNode("Transform");
            if (transformNode != null)
            {
                model.Position = ParseVector3(GetStringAttribute(transformNode, "pos", "0,0,0"));
                model.Rotation = ParseVector3(GetStringAttribute(transformNode, "rot", "0,0,0"));
                model.Scale = ParseVector3(GetStringAttribute(transformNode, "scale", "1,1,1"));
            }

            XmlNode renderNode = modelNode.SelectSingleNode("Render");
            if (renderNode != null)
            {
                model.Transparency = GetFloatAttribute(renderNode, "transparency", 0f);
            }

            XmlNode annotationsNode = modelNode.SelectSingleNode("Annotations");
            if (annotationsNode != null)
            {
                model.Annotations = new Dictionary<string, AnnotationData>();

                foreach (XmlNode annotationNode in annotationsNode.SelectNodes("Annotation"))
                {
                    string key = GetStringAttribute(annotationNode, "key", "");
                    AnnotationData annotation = new AnnotationData
                    {
                        Key = key,
                        Type = GetStringAttribute(annotationNode, "type", "board"),
                        Text = GetStringAttribute(annotationNode, "text", "")
                    };

                    model.Annotations[key] = annotation;
                }
            }

            config.Models[model.Id] = model;
        }
    }

    private void ParseLevels(XmlNode levelsNode, GameConfig config)
    {
        if (levelsNode == null) return;

        foreach (XmlNode levelNode in levelsNode.SelectNodes("Level"))
        {
            LevelData level = new LevelData
            {
                Index = GetIntAttribute(levelNode, "index", 1),
                Title = GetStringAttribute(levelNode, "title", ""),
                Difficulty = GetStringAttribute(levelNode, "difficulty", "normal"),
                Pairs = new List<PairData>()
            };

            foreach (XmlNode pairNode in levelNode.SelectNodes("Pair"))
            {
                PairData pair = new PairData
                {
                    ModelId = GetStringAttribute(pairNode, "modelId", ""),
                    AnnotationKey = GetStringAttribute(pairNode, "annotationKey", "")
                };

                level.Pairs.Add(pair);
            }

            config.Levels.Add(level);
        }

        config.Levels.Sort((a, b) => a.Index.CompareTo(b.Index));
    }

    private void ParseValidation(XmlNode validationNode, GameConfig config)
    {
        if (validationNode == null) return;

        config.ValidationRules = new List<ValidationRule>();

        foreach (XmlNode ruleNode in validationNode.SelectNodes("Rule"))
        {
            ValidationRule rule = new ValidationRule
            {
                Type = GetStringAttribute(ruleNode, "type", ""),
                Enabled = GetBoolAttribute(ruleNode, "enabled", true)
            };

            config.ValidationRules.Add(rule);
        }
    }

    private void ParseFeedback(XmlNode feedbackNode, GameConfig config)
    {
        if (feedbackNode == null) return;

        config.Feedback = new FeedbackSettings();

        XmlNode soundNode = feedbackNode.SelectSingleNode("Sound");
        if (soundNode != null)
        {
            config.Feedback.CorrectSound = GetStringAttribute(soundNode, "correct", "");
            config.Feedback.IncorrectSound = GetStringAttribute(soundNode, "incorrect", "");
        }

        XmlNode visualNode = feedbackNode.SelectSingleNode("Visual");
        if (visualNode != null)
        {
            config.Feedback.CorrectColor = GetStringAttribute(visualNode, "correct", "green");
            config.Feedback.IncorrectColor = GetStringAttribute(visualNode, "incorrect", "red");
            config.Feedback.Duration = GetFloatAttribute(visualNode, "duration", 1.5f);
        }
    }

    private void ApplyGameSettings()
    {

        Debug.Log($"Applying settings...");
        Debug.Log($"- Points for correct answer: {gameConfig.Scoring.CorrectPoints}");
        Debug.Log($"- Points for incorrect answer: {gameConfig.Scoring.IncorrectPoints}");
        Debug.Log($"- Number of levels: {gameConfig.Levels.Count}");
    }

    private string GetStringAttribute(XmlNode node, string attributeName, string defaultValue = "")
    {
        XmlAttribute attr = node.Attributes[attributeName];
        return attr != null ? attr.Value : defaultValue;
    }

    private int GetIntAttribute(XmlNode node, string attributeName, int defaultValue = 0)
    {
        string value = GetStringAttribute(node, attributeName, defaultValue.ToString());
        return int.TryParse(value, out int result) ? result : defaultValue;
    }

    private float GetFloatAttribute(XmlNode node, string attributeName, float defaultValue = 0f)
    {
        string value = GetStringAttribute(node, attributeName, defaultValue.ToString());
        return float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out float result) ? result : defaultValue;
    }

    private bool GetBoolAttribute(XmlNode node, string attributeName, bool defaultValue = false)
    {
        string value = GetStringAttribute(node, attributeName, defaultValue.ToString()).ToLower();
        return value == "true" || value == "1";
    }

    private Vector3 ParseVector3(string vectorString)
    {
        string[] parts = vectorString.Split(',');
        if (parts.Length != 3)
        {
            Debug.LogWarning($"Invalid Vector3 format: {vectorString}");
            return Vector3.zero;
        }

        float x = float.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out float xVal) ? xVal : 0f;
        float y = float.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out float yVal) ? yVal : 0f;
        float z = float.TryParse(parts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out float zVal) ? zVal : 0f;

        return new Vector3(x, y, z);
    }


    public GameConfig GetGameConfig()
    {
        return gameConfig;
    }
}


[System.Serializable]
public class GameConfig
{
    public HUDSettings HUD;
    public ScoringSettings Scoring;
    public Vector3 RoomCenter;
    public BoardSettings Board;
    public Dictionary<string, ModelData> Models = new Dictionary<string, ModelData>();
    public List<LevelData> Levels = new List<LevelData>();
    public List<ValidationRule> ValidationRules = new List<ValidationRule>();
    public FeedbackSettings Feedback;
}

[System.Serializable]
public class HUDSettings
{
    public bool Enabled;
    public string Anchor;
    public bool ShowScore;
    public bool ShowTime;
    public string TimeFormat;
    public bool ShowAttempts;
    public bool ShowLevel;
}

[System.Serializable]
public class ScoringSettings
{
    public int CorrectPoints;
    public int IncorrectPoints;
}

[System.Serializable]
public class BoardSettings
{
    public string Id;
    public string Prefab;
    public Vector3 Position;
    public Vector3 Rotation;
    public Vector3 Scale;
    public int Columns;
    public float CardSpacingX;
    public float CardSpacingY;
}

[System.Serializable]
public class ModelData
{
    public string Id;
    public string Name;
    public string Prefab;
    public Vector3 Position;
    public Vector3 Rotation;
    public Vector3 Scale;
    public float Transparency;
    public Dictionary<string, AnnotationData> Annotations;
}

[System.Serializable]
public class AnnotationData
{
    public string Key;
    public string Type;
    public string Text;
}

[System.Serializable]
public class LevelData
{
    public int Index;
    public string Title;
    public string Difficulty;
    public List<PairData> Pairs;
}

[System.Serializable]
public class PairData
{
    public string ModelId;
    public string AnnotationKey;
}

[System.Serializable]
public class ValidationRule
{
    public string Type;
    public bool Enabled;
}

[System.Serializable]
public class FeedbackSettings
{
    public string CorrectSound;
    public string IncorrectSound;
    public string CorrectColor;
    public string IncorrectColor;
    public float Duration;
}
