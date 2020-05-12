using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

public class FocusClosestTarget : MonoBehaviour
{
	public bool focusing = false;

	[Space]
	public Camera focusCamera = null;
	public Transform[] focusTargets = new Transform[0];

	[Space]
	public float focusNearDistance = 1.0f;// 1m
	public float focusNearExtent = 0.03f;// 3cm
	public float focusNearFalloff = 0.15f;// 15cm

	[Space]
	public float focusFarDistance = 2.0f;
	public float focusFarExtent = 0.15f;
	public float focusFarFalloff = 0.75f;

	[Space]
	public float activeDistance;
	public float activeExtent;
	public float activeFalloff;

	private Volume volume;
	private VolumeProfile volumeProfile;
	private DepthOfField dof;

	public void StartFocus()
	{
		if (focusing == false)
		{
			focusing = true;
			//Debug.Log("StartFocus");
		}
	}

	public void CancelFocus(bool instantly = false)
	{
		if (focusing)
		{
			focusing = false;
			//Debug.Log("CancelFocus");
		}

		if (instantly)
		{
			if (activeExtent < focusFarExtent)
				activeExtent = focusFarExtent;
		}
	}

	void Awake()
	{
		activeDistance = focusFarDistance;
		activeExtent = focusFarExtent;
		activeFalloff = focusFarFalloff;

		volumeProfile = ScriptableObject.CreateInstance<VolumeProfile>();
		volumeProfile.hideFlags = HideFlags.HideAndDontSave;

		dof = volumeProfile.Add<DepthOfField>();
		dof.hideFlags = HideFlags.HideAndDontSave;
		dof.focusMode.Override(DepthOfFieldMode.Manual);
		dof.quality.Override((int)ScalableSettingLevelParameter.Level.Medium);
		dof.nearFocusStart.Override(0.0f);
		dof.nearFocusEnd.Override(0.0f);
		dof.farFocusStart.Override(focusFarExtent);
		dof.farFocusEnd.Override(focusFarExtent);

		volume = this.gameObject.AddComponent<Volume>();
		volume.hideFlags = HideFlags.HideAndDontSave;
		volume.isGlobal = true;
		volume.priority = 100;
		volume.sharedProfile = volumeProfile;
	}

	void OnDestroy()
	{
		volumeProfile.Remove<DepthOfField>();
	}

	void LateUpdate()
	{
		var bestTarget = null as Transform;
		var bestDistance = float.PositiveInfinity;

		SelectTargetAndDistance(out bestTarget, out bestDistance);

		if (bestTarget != null)
		{
			activeDistance = bestDistance;
		}

		if (focusing)
		{
			var t = Mathf.InverseLerp(focusNearDistance, focusFarDistance, activeDistance);
			{
				activeExtent = Mathf.Lerp(focusNearExtent, focusFarExtent, t);
				activeFalloff = Mathf.Lerp(focusNearFalloff, focusFarFalloff, t);
			}
		}
		else
		{
			activeExtent = focusFarExtent;
			activeFalloff = focusFarFalloff;
		}

		if (activeExtent < focusFarExtent)
		{
			// nearFocusStart
			// nearFocusEnd
			// ... in focus ...
			// farFocusStart
			// farFocusEnd

			dof.nearFocusStart.value = 0.0f;//Mathf.Max(0.0f, activeDistance - activeExtent - activeFalloff);
			dof.nearFocusEnd.value = 0.0f;//Mathf.Max(0.0f, activeDistance - activeExtent);
			dof.farFocusStart.value = activeDistance + activeExtent;
			dof.farFocusEnd.value = activeDistance + activeExtent + activeFalloff;

			dof.active = true;
		}
		else
		{
			dof.active = false;
		}
	}

	bool SelectTargetAndDistance(out Transform bestTarget, out float bestDistance)
	{
		bestTarget = null as Transform;
		bestDistance = float.PositiveInfinity;

		if (focusCamera != null)
		{
			var focusOrigin = focusCamera.transform.position;
			var focusForward = focusCamera.transform.forward;

			foreach (var focusTarget in focusTargets)
			{
				var v = focusTarget.position - focusOrigin;
				var d = Vector3.Dot(v, focusForward);
				if (d < bestDistance)
				{
					bestTarget = focusTarget;
					bestDistance = d;
				}
			}
		}

		return (bestTarget != null);
	}

	float DecayTowards(float a, float b, float retain, float dt)
	{
		// http://www.rorydriscoll.com/2016/03/07/frame-rate-independent-damping-using-lerp/
		return Mathf.Lerp(a, b, 1.0f - Mathf.Pow(retain, dt));
	}
}
