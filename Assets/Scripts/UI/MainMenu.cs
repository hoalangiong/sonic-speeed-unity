using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Main menu — map selection, play button.
/// </summary>
public class MainMenu : MonoBehaviour
{
    public void PlayCoastal() => LoadScene("Coastal");
    public void PlayTokyo() => LoadScene("Tokyo");
    public void PlayDesert() => LoadScene("Desert");
    public void PlaySnow() => LoadScene("Snow");

    public void QuitGame()
    {
        Application.Quit();
    }

    private void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}
