using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] GameObject PausePanel;
    [SerializeField] PlaneController plane;
    [SerializeField] public bool isPaused = false;
    void Start()
    {
        PausePanel.SetActive(isPaused);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && !plane.DeathStatus)
        {
            isPaused = !isPaused;
            if (!isPaused)
            {
                Continue();
            }
            else
            {
                Pause();
            }
        }
    }

    public void Pause()
    {
        PausePanel.SetActive(true);
        isPaused = true;
        Time.timeScale = 0;
    }
    public void Continue()
    {
        PausePanel.SetActive(false);
        isPaused = false;
        Time.timeScale = 1;
    }
    public void Restart()
    {
        Time.timeScale = 1f;
        Destroy(GameObject.FindWithTag("plane")); 
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    public void Close()
    {
        SceneManager.LoadScene(0);
    }
}
