using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PauseGame : MonoBehaviour
{
    public Image pauseScreen;
    private PlayerController player;

    // Start is called before the first frame update
    void Start()
    {
        Application.focusChanged += OnFocusChanged;
        player = FindObjectOfType<PlayerController>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F11))
        {
            ToggleFullscreen();
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (Time.timeScale == 1)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                pauseScreen.gameObject.SetActive(true);
                Time.timeScale = 0;
                player.isPaused = true;
            }
            else if (Time.timeScale == 0)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                pauseScreen.gameObject.SetActive(false);
                Time.timeScale = 1;
                player.isPaused = false;
            }
        }
    }

    private void ToggleFullscreen()
    {
        Screen.fullScreen = !Screen.fullScreen;
    }

    private void OnFocusChanged(bool hasFocus)
    {
        if (!hasFocus && Time.timeScale == 1)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            pauseScreen.gameObject.SetActive(true);
            Time.timeScale = 0;
            player.isPaused = true;
        }
    }
}
