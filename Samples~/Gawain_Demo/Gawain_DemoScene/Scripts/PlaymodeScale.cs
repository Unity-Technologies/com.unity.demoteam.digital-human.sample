using UnityEngine;

public class PlaymodeScale : MonoBehaviour
{
	public Vector3 localScale = Vector3.one;

	public bool modifyTiling = false;
	public Renderer modifyTilingRenderer = null;
	public string[] modifyTilingProperties = new string[] { "_BaseColorMap" };

	void Awake()
	{
		var localScaleFactor = Vector2.one;
		var localScalePrev = this.transform.localScale;
		if (localScalePrev != Vector3.zero)
		{
			localScaleFactor.x = localScale.x / localScalePrev.x;
			localScaleFactor.y = localScale.z / localScalePrev.z;
		}

		this.transform.localScale = localScale;

		if (modifyTiling && modifyTilingRenderer != null)
		{
			var modifyTilingMaterial = modifyTilingRenderer.material;
			modifyTilingMaterial.hideFlags = HideFlags.HideAndDontSave;

			foreach (var propertyName in modifyTilingProperties)
			{
				var textureScale = modifyTilingRenderer.material.GetTextureScale(propertyName);
				{
					textureScale.x *= localScaleFactor.x;
					textureScale.y *= localScaleFactor.y;
				}
				modifyTilingMaterial.SetTextureScale(propertyName, textureScale);
			}
		}
	}
}
