using UnityEngine;
using UnityEngine.UI;

public class UI : MonoBehaviour
{
	private static UI instance;

	[SerializeField]
	private Text scoreText;


	private void Start()
	{
		instance = this;
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
}
