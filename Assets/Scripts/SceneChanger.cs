
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChanger : MonoBehaviour
{
    public string sceneName;

    public void ChangeScene()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(sceneName);
    }
}
