using UnityEngine;
using UnityEngine.SceneManagement;

public class SplashScreen : MonoBehaviour
{
    public float delay = 2f;

    void Start()
    {
        Invoke(nameof(LoadMenu), delay);
    }

    void LoadMenu()
    {
        SceneManager.LoadScene("MenuScene");
    }
}
