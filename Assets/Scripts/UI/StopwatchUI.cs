using TMPro;
using UnityEngine;

public class StopwatchUI : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI stopwatchText;

    [SerializeField]
    private GameXMLParser gameXMLParser;

    private GameConfig gameConfig;

    private float elapsedTime;

    private void Start()
    {
        gameConfig = gameXMLParser.GetGameConfig();
    }

    void Update()
    {
        if (!gameConfig.HUD.Enabled || !gameConfig.HUD.ShowTime)
        {
            return;
        }

        elapsedTime += Time.deltaTime;

        //int minutes = Mathf.FloorToInt(elapsedTime / 60f);
        int seconds = Mathf.FloorToInt(elapsedTime);
        //int milliseconds = Mathf.FloorToInt((elapsedTime * 1000f) % 1000f);

        //stopwatchText.text = string.Format("{0:00}:{1:00}.{2:000}", minutes, seconds, milliseconds);
        stopwatchText.text = string.Format("Изминало време: {0} сек.", seconds);
    }

    public void Stop()
    {
        this.enabled = false;
    }
}
