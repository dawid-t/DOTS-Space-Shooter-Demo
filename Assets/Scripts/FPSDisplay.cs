using UnityEngine;
using UnityEngine.UI;

public class FPSDisplay : MonoBehaviour
{
	[SerializeField]
	private Text text_FPS;
	private float deltaTime = 0.0f;


	private void Update()
	{
		if(Time.timeScale == 1)
		{
			deltaTime += (Time.deltaTime - deltaTime) * 0.1f;
			float fps = 1.0f / deltaTime;
			text_FPS.text = "FPS: "+Mathf.Round(fps);
		}
	}
}
