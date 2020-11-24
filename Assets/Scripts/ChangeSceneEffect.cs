using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ChangeSceneEffect : MonoBehaviour
{
	private static ChangeSceneEffect instance;

	private Animator animator;


	public static ChangeSceneEffect Instance => instance;


	private void Start()
	{
		instance = this;
		animator = GetComponent<Animator>();
		GetComponent<Image>().enabled = true;
		StartCoroutine(StartSceneEffect());
	}

	public void EndSceneEffect()
	{
		transform.parent.GetComponent<Canvas>().enabled = true;
		animator.enabled = true;
		animator.Play("EndSceneEffect", 0, 0);
	}

	private IEnumerator StartSceneEffect()
	{
		yield return new WaitForSecondsRealtime(0.1f);
		animator.Play("StartSceneEffect", 0, 0);
		StartCoroutine(OffPanel());
	}

	private IEnumerator OffPanel()
	{
		yield return new WaitForSecondsRealtime(0.5f);
		transform.parent.GetComponent<Canvas>().enabled = false;
		animator.enabled = false;
	}
}
