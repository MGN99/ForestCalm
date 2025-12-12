using UnityEngine;
using UnityEngine.SceneManagement;


public class MenuController : MonoBehaviour
{
    [Header("Paneles de UI")]
    public GameObject mainPanel;
    public GameObject selectionPanel;

    void Start()
    {
        // Forzamos el estado inicial correcto:
        mainPanel.SetActive(true);      // Menú principal ENCENDIDO
        selectionPanel.SetActive(false); // Menú selección APAGADO
    }

    // --- FUNCIONES DE NAVEGACIÓN DEL MENÚ ---
    // Al presionar "START" en el menú principal
    public void ShowSelectionMenu()
    {
        mainPanel.SetActive(false);      // Oculta el menú principal
        selectionPanel.SetActive(true);  // Muestra las opciones
    }

    // Al presionar "ATRÁS" (si pones ese botón)
    public void BackToMainMenu()
    {
        selectionPanel.SetActive(false); // Oculta las opciones
        mainPanel.SetActive(true);       // Vuelve al principal
    }

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
