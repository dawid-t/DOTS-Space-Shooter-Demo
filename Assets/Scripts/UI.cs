﻿using Unity.Entities;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UI : MonoBehaviour
{
	private static UI instance;

	[SerializeField]
	private GameObject losePanel;
	[SerializeField]
	private Text scoreText;


	private void Start()
	{
		instance = this;
		Time.timeScale = 1;
	}

	private void Update()
	{
		if(Input.GetKey(KeyCode.Escape))
		{
			Application.Quit();
		}
	}

	public static void UpdateScore(int score)
	{
		instance.scoreText.text = "Score: "+score;
	}

	public static void ShowRestartScenePanel()
	{
		Time.timeScale = 0;
		instance.losePanel.SetActive(true);
	}

	public void OnClickRestartButton()
	{
		// Play end scene animation:
		// todo.

		// Reset objects:
		EntityManager manager = World.Active.EntityManager;
		manager.DestroyEntity(manager.UniversalQuery);
		ShipSystem.Score = 0;

		// Restart level:
		SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
	}
}
