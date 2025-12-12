using UnityEngine;
using UnityEngine.SceneManagement;


public class MenuController : MonoBehaviour
{
    public void StartGame()
    {
        SceneManager.LoadScene("ForestScene");
    }

    public void GoToMeditation()
    {
        SceneManager.LoadScene("ForestMeditationScene");
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
