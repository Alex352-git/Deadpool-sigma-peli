using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class PauseMenu : MonoBehaviour
{
    public GameObject pauseOverlay;
    public GameObject player;

    private bool isPaused = false;

    void Start()
    {
        pauseOverlay.SetActive(false);
        Time.timeScale = 1f;

        // Peli alkaa normaalisti: hiiri lukittu ja piilotettu
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (isPaused)
                Resume();
            else
                Pause();
        }
    }

    public void Pause()
    {
        isPaused = true;
        Time.timeScale = 0f;

        pauseOverlay.SetActive(true);

        // Estet‰‰n pelaajan kontrollit
        if (player != null)
        {
            MonoBehaviour[] scripts = player.GetComponentsInChildren<MonoBehaviour>();

            foreach (MonoBehaviour script in scripts)
            {
                script.enabled = false;
            }
        }

        // Vapautetaan hiiri Pause Menulle
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void Resume()
    {
        Time.timeScale = 1f;
        isPaused = false;

        pauseOverlay.SetActive(false);

        // Palautetaan pelaajan kontrollit
        if (player != null)
        {
            MonoBehaviour[] scripts = player.GetComponentsInChildren<MonoBehaviour>();

            foreach (MonoBehaviour script in scripts)
            {
                script.enabled = true;
            }
        }

        // Lukitaan hiiri takaisin peliin
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void Restart()
    {
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void MainMenu()
    {
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        SceneManager.LoadScene("Ilo Scene");
    }
}