using UnityEngine;
using UnityEngine.SceneManagement;

public class MainScene : MonoBehaviour
{
    public void Play()
    {
        Time.timeScale = 1f;
        Destroy(GameObject.FindWithTag("plane"));
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
    public void Quit()
    {
        Application.Quit();
    }
    public void Settings()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 2);
    }
}
