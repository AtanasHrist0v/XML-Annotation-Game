using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour {
    [SerializeField]
    private GameXMLParser gameXMLParser;

    [SerializeField]
    private PlayerMovement movementScript;
    [SerializeField]
    private Annotate annotateScript;

    [SerializeField]
    private TextMeshProUGUI levelText;
    [SerializeField]
    private TextMeshProUGUI pointsText;
    [SerializeField]
    private TextMeshProUGUI endGameText;

    private GameConfig gameConfig = null;

    private int currentLevelIndex = -1;
    private int howManyToAnnotate = 0;
    private int annotatedObjects = 0;
    private int points = 0;
    private int totalPoints = 0;
    private int totalAttempts = 0;

    [SerializeField]
    private Texture2D noteTexture;

    [SerializeField]
    private StopwatchUI stopwatchUI;

    private List<GameObject> loadedGameObjects = new List<GameObject>();

    void Start() {
        gameXMLParser.LoadXML();
        gameConfig = gameXMLParser.GetGameConfig();

        pointsText.text = string.Format("0/{0} т.", gameConfig.Levels[0].Pairs.Count);

        if (currentLevelIndex < gameConfig.Levels.Count - 1) {
            LoadLevel(currentLevelIndex + 1);
        } else {
            EndGame();
        }
    }

    void LoadLevel(int levelIndex) {
        int objectsInLevel = gameConfig.Levels[levelIndex].Pairs.Count;
        howManyToAnnotate = objectsInLevel;

        totalPoints += objectsInLevel;
        pointsText.text = string.Format("{0}/{1} т.", points, totalPoints);

        for (int i = 0; i < objectsInLevel; i++) {
            ModelData currentModel = gameConfig.Models[gameConfig.Levels[levelIndex].Pairs[i].ModelId];
            GameObject currentModelPrefab = Resources.Load<GameObject>(currentModel.Prefab);

            GameObject currentGameObject = Instantiate(currentModelPrefab, currentModel.Position, Quaternion.Euler(currentModel.Rotation));
            currentGameObject.transform.localScale = currentModel.Scale;
            currentGameObject.layer = 7;

            //LoadAnnotations(levelIndex, currentGameObject); ->

            GameObject annotation = new GameObject("Annotation");
            annotation.transform.position = new Vector3(0.03f + i, 1.5f, 4.75f); ;
            annotation.layer = 6;

            BoxCollider boxCol = annotation.AddComponent<BoxCollider>();
            boxCol.isTrigger = true;
            boxCol.center = Vector3.zero;
            boxCol.size = new Vector3(0.3f, 0.4f, 0.01f);

            GameObject annotationBackground = GameObject.CreatePrimitive(PrimitiveType.Quad);
            annotationBackground.name = "Background";
            annotationBackground.transform.SetParent(annotation.transform);
            annotationBackground.transform.localPosition = Vector3.zero;
            annotationBackground.transform.localRotation = Quaternion.identity;
            
            Material mat = new Material(Shader.Find("Unlit/Transparent"));
            mat.mainTexture = noteTexture;
            annotationBackground.GetComponent<MeshRenderer>().material = mat;


            //float aspect = (float)noteTexture.width / noteTexture.height;
            annotationBackground.transform.localScale = new Vector3(0.3f, 0.4f, 1f);

            GameObject annotationText = new GameObject("DynamicTMPText");
            annotationText.transform.SetParent(annotation.transform);
            annotationText.transform.localPosition = new Vector3(0, 0, -0.01f);

            TextMeshPro tmp = annotationText.AddComponent<TextMeshPro>();
            //width = 0.3 & heigth = 0.4
            tmp.rectTransform.sizeDelta = new Vector2(0.3f, 0.4f);
            tmp.fontSize = 0.6f;
            tmp.color = Color.black;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.overflowMode = TextOverflowModes.Overflow;

            tmp.text = currentModel.Annotations[gameConfig.Levels[levelIndex].Pairs[i].AnnotationKey].Text;
            //tmp.transform.rotation = Quaternion.Euler(0, 180f, 0);

            annotation.transform.SetParent(currentGameObject.transform);

            loadedGameObjects.Add(currentGameObject);
        }

        UpdateLevelUI(gameConfig.Levels[levelIndex].Title);

        //levelLoaded = true;
        currentLevelIndex = levelIndex;
    }

    private void UpdateLevelUI(string levelTitle) {
        levelText.text = string.Format("{0}", levelTitle);
    }

    private void LoadAnnotations(int levelIndex, GameObject parent) {
        int annotationsInLevel = gameConfig.Levels[levelIndex].Pairs.Count;
        for (int i = 0; i < annotationsInLevel; i++) {
            GameObject annotationText = new GameObject("DynamicTMPText");
            annotationText.transform.position = new Vector3(-6.31f + 3 * i, 1.56f, -4.75f);
            TextMeshPro tmp = annotationText.AddComponent<TextMeshPro>();
            tmp.rectTransform.sizeDelta = new Vector2(1, 1);
            tmp.fontSize = 1.5f;
            tmp.text = gameConfig.Levels[levelIndex].Pairs[i].AnnotationKey;
            tmp.transform.rotation = Quaternion.Euler(0, 180f, 0);
            BoxCollider boxCol = annotationText.AddComponent<BoxCollider>();
            boxCol.size = new Vector3(1f, 1f, 0.2f);
            annotationText.layer = 6;
        }
    }

    public void AddPoint() {
        annotatedObjects++;
        points++;

        if (annotatedObjects == howManyToAnnotate) {
            annotatedObjects = 0;

            //
            if (loadedGameObjects.Count != 0) {
                foreach (GameObject obj in loadedGameObjects) {
                    if (obj != null)
                        Destroy(obj);
                }
                loadedGameObjects.Clear();
            }

            if (currentLevelIndex < gameConfig.Levels.Count - 1) {
                LoadLevel(currentLevelIndex + 1);
            } else {
                EndGame();
            }
            //
        }

        pointsText.text = string.Format("{0}/{1} т.", points, totalPoints);
    }

    public void AddAttempt() {
        totalAttempts += 1;
    }

    private void EndGame() {
        stopwatchUI.Stop();
        endGameText.text = string.Format("Total attempts: {0}\nCorrect attempts: {1}\nEfficiency: {2:00}%", totalAttempts, points, ((float)points) / totalAttempts * 100);
        endGameText.enabled = true;

        annotateScript.enabled = false;
        //movementScript.enabled = false;
        this.enabled = false;
    }
}
