﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public static bool GameIsPaused = false;
	public GameObject pauseMenuUI;

	private void Start()
	{
		Resume();
	}

	void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
		{
            if(GameIsPaused)
			{
                Resume();
			}
            else
			{
                Pause();
			}
		}
    }

    public void Resume()
	{
		Cursor.lockState = CursorLockMode.Locked;
		//Cursor.visible = false;
		pauseMenuUI.SetActive(false);
		Time.timeScale = 1f;
		GameIsPaused = false;
	}

	void Pause()
	{
		Cursor.lockState = CursorLockMode.None;
		//Cursor.visible = true;
		pauseMenuUI.SetActive(true);
		Time.timeScale = 0f;
		GameIsPaused = true;
	}

	public void QuitGame()
	{
		Time.timeScale = 1f;
		SceneManager.LoadScene("Menu");
	}
}
