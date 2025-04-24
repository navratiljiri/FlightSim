using UnityEngine;
using UnityEngine.SceneManagement;

public class SettingMenu : MonoBehaviour
{
    public void back()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 2);
    }
}
