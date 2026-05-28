using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneReturnController : MonoBehaviour
{
    [Header("Input")]
    public KeyCode returnKey = KeyCode.Escape;

    void Update()
    {
        if (Input.GetKeyDown(returnKey))
        {
            string returnSceneName = PlayerPrefs.GetString("ReturnSceneName", "");
            
            if (!string.IsNullOrEmpty(returnSceneName))
            {
                SceneManager.LoadScene(returnSceneName);
            }
        }
    }
}