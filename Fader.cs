using UnityEngine;

public class Fader : MonoBehaviour {
	public Texture fadeOutTexture;
	public float fadeSpeed = 0.75f;

	public int drawDepth = 1000;

	public Color color = Color.white;
	public float alpha = 1.0f;

	public float fadeDir = -1.0f;

	public void LateUpdate () {
		alpha += fadeDir * fadeSpeed * Time.deltaTime;
		alpha = Mathf.Clamp01(alpha);
	}

	public void OnGUI () {
		GUI.color = GameUI.SetAlpha(color, alpha);
		GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), fadeOutTexture);
		GUI.color = Color.white;

		GUI.depth = drawDepth;
	}
}
