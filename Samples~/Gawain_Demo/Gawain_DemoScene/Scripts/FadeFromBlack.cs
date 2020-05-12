using UnityEngine;
using UnityEngine.UI;

public class FadeFromBlack : MonoBehaviour
{
	Image image;

	Color fadeInColor = Color.black;
	float fadeInElapsed = 0.0f;

	public int fadeInStartFrame = 2;
	public float fadeInDuration = 0.2f;

	void Awake()
	{
		image = GetComponent<Image>();
		image.color = fadeInColor;
	}

	void Update()
	{
		if (Time.frameCount < fadeInStartFrame)
			return;

		fadeInElapsed += Time.deltaTime;
		fadeInColor.a = (fadeInDuration != 0.0f) ? (1.0f - Mathf.Clamp01(fadeInElapsed / fadeInDuration)) : 0.0f;

		image.color = fadeInColor;

		if (fadeInElapsed >= fadeInDuration)
		{
			this.gameObject.SetActive(false);
		}
	}
}
