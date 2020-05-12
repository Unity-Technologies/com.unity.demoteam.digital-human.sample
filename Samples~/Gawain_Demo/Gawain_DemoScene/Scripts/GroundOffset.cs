using UnityEngine;

public class GroundOffset : MonoBehaviour
{
	public Transform groundMarker;
	public float groundOffset;

	void LateUpdate()
	{
		if (groundMarker == null)
			return;

		Vector3 position = this.transform.position;
		{
			position.y = groundMarker.position.y;
			position.y += groundOffset;
		}

		this.transform.position = position;
	}
}
