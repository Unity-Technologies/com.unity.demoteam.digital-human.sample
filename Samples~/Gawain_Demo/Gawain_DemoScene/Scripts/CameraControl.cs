using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

public class CameraControl : MonoBehaviour
{
    bool controlEnabled;

    public Transform anchor;
    Transform target;
    Quaternion targetOrientationOffset;
    Quaternion orientationOffset;
    float timeOfTargetAttachment = float.MinValue;
    const float timeToSnapToTarget = 2f;

    float initialDistanceFromAnchor;
    float distanceFromAnchor;
    float targetDistanceFromAnchor;
    float targetDistanceFromAnchorLastFollow;
    float minDistance = 0.5f;
    float maxDistance = 8f;
    float distanceChangeVelocity;

    const int orientationControlButton = 0; // Left Mouse button
    public float orientationSensitivity = 0.5f;
    public float orientationVerticalFreedom = 60;
    [Range(0, 1f)]
    public float orientationChangeDuration = 0.02f;
    Vector2 orientation;
    Vector2 orientationTarget;
    Vector2 orientationChangeVelocity;

    const int pedestalControlButton = 1; // Right Mouse button
    public Vector2 pedestalFreedom;
    public float pedestalSensitivity = 0.1f; // Could be calculated out of mouse position projection to focus axis to have 1:1 motion
    public float pedestalAndDistanceChangeDuration = 0.1f;
    float pedestal;
    float pedestalTarget;
    float pedestalTargetLastFollow;
    float pedestalChangeVelocity;

    public float zoomDragSensitivity = 0.1f;
    public float zoomScrollSensitivity = 1.0f;

    public bool farClipPlaneTrimming = false;
    [Min(1.0f)]
    public float farClipPlaneMinimum = 1.0f;

    Vector3 previousMousePos;

    Camera attachedCamera;

    float DeltaTime => Time.deltaTime;

    public FocusClosestTarget focusHandler;
    public float focusPedestalSlack = 0.001f; // was 0.2f;
    public float focusDistanceSlack = 0.001f; // was 1.5f;

    private void Awake()
    {
        if (!anchor)
            return;

        var offsetFromTarget = transform.localPosition - anchor.position;
        initialDistanceFromAnchor = offsetFromTarget.magnitude;
        targetDistanceFromAnchor = initialDistanceFromAnchor;
        distanceFromAnchor = initialDistanceFromAnchor;

        pedestal = transform.position.y;

        attachedCamera = GetComponent<Camera>();
    }

    private void LateUpdate()
    {
        if (!anchor)
            return;

        if (controlEnabled)
        {
            HandleInput();
        }
        else
        {
            FollowTarget();
        }
        UpdateValues();

        var offsetRotation = Quaternion.Euler(orientation);
        var offsetFromAnchorNormalized = Vector3.back;
        transform.position = (offsetRotation * offsetFromAnchorNormalized * distanceFromAnchor) + (Vector3.up * pedestal) + anchor.position;
        transform.rotation = offsetRotation * orientationOffset;

        if (farClipPlaneTrimming)
        {
            attachedCamera.farClipPlane = distanceFromAnchor + farClipPlaneMinimum;
        }
    }

    void HandleInput()
    {
        var windowSize = new Vector2(Screen.width, Screen.height);
        var windowAspect = windowSize.x / windowSize.y;

        var mousePos = Input.mousePosition;
        var mouseDelta = (Vector2)(mousePos - previousMousePos);
        previousMousePos = mousePos;

        const float testedScalingFactor = 0.1f;
        const float testedWindowHeight = 1080.0f;

        mouseDelta.x = (mouseDelta.x / Screen.width) * windowAspect;
        mouseDelta.y = mouseDelta.y / Screen.height;
        mouseDelta *= (testedScalingFactor * testedWindowHeight);

        if (Input.GetMouseButtonDown(orientationControlButton) || Input.GetMouseButtonDown(pedestalControlButton))
        {
            mouseDelta = Vector2.zero;
        }

        if (Input.GetMouseButton(orientationControlButton))
        {
            orientationTarget += new Vector2(-mouseDelta.y, mouseDelta.x) * orientationSensitivity;

            orientationTarget.x = Mathf.Repeat(orientationTarget.x + 180f, 360f) - 180f;
            orientationTarget.x = Mathf.Clamp(orientationTarget.x, -orientationVerticalFreedom, orientationVerticalFreedom);

            orientationTarget.y = Mathf.Repeat(orientationTarget.y + 180f, 360f) - 180f;
        }

        if (Input.GetMouseButton(pedestalControlButton))
        {
            pedestalTarget -= mouseDelta.y * pedestalSensitivity * distanceFromAnchor / maxDistance;
            pedestalTarget = Mathf.Clamp(pedestalTarget, pedestalFreedom.x, pedestalFreedom.y);

            targetDistanceFromAnchor -= mouseDelta.x * distanceFromAnchor * Time.deltaTime * (1f + targetDistanceFromAnchor) * zoomDragSensitivity;
        }
        
        targetDistanceFromAnchor -= Input.mouseScrollDelta.y * distanceFromAnchor * Time.deltaTime * Mathf.Pow(Mathf.Abs(targetDistanceFromAnchor), 0.7f) * zoomScrollSensitivity;
        targetDistanceFromAnchor = Mathf.Clamp(targetDistanceFromAnchor, minDistance, maxDistance);

        if (focusHandler != null)
        {
            var targetDistanceDiff = Mathf.Abs(targetDistanceFromAnchor - targetDistanceFromAnchorLastFollow);
            if (targetDistanceDiff > focusDistanceSlack)
            {
                focusHandler.CancelFocus();
            }

            var pedestalDiff = Mathf.Abs(pedestalTarget - pedestalTargetLastFollow);
            if (pedestalDiff > focusPedestalSlack)
            {
                focusHandler.CancelFocus();
            }
        }
    }

    void UpdateValues()
    {
        //var smoothTimeMultiplierUncontrolled = 2f * (1f - Mathf.Clamp01((Time.time - timeOfTargetAttachment) / timeToSnapToTarget));
        //var smoothTimeMultiplier = controlEnabled ? 1f : smoothTimeMultiplierUncontrolled;

        var smoothTimeMultiplier = controlEnabled ? 1f : 2f;
        smoothTimeMultiplier *= 1f - Mathf.Clamp01((Time.time - timeOfTargetAttachment) / timeToSnapToTarget);

        var normalizedTargetDistanceFromAnchor = Mathf.Clamp01((targetDistanceFromAnchor - minDistance) / (maxDistance - minDistance));
        var pedestalFreedomClamp = (pedestalFreedom.x + pedestalFreedom.y) * normalizedTargetDistanceFromAnchor;
        const float clampCenter = 0.5f;
        var clampedPedestalTarget = Mathf.Clamp(pedestalTarget, pedestalFreedom.x + pedestalFreedomClamp * clampCenter, pedestalFreedom.y - pedestalFreedomClamp * (1f - clampCenter));

        if (smoothTimeMultiplier > 0f)
        {
            pedestal = Mathf.SmoothDamp(pedestal, clampedPedestalTarget, ref pedestalChangeVelocity, pedestalAndDistanceChangeDuration * smoothTimeMultiplier, float.MaxValue, DeltaTime);
            orientation.x = Mathf.SmoothDampAngle(orientation.x, orientationTarget.x, ref orientationChangeVelocity.x, orientationChangeDuration * smoothTimeMultiplier, float.MaxValue, DeltaTime);
            orientation.y = Mathf.SmoothDampAngle(orientation.y, orientationTarget.y, ref orientationChangeVelocity.y, orientationChangeDuration * smoothTimeMultiplier, float.MaxValue, DeltaTime);

            distanceFromAnchor = Mathf.SmoothDamp(distanceFromAnchor, targetDistanceFromAnchor, ref distanceChangeVelocity, pedestalAndDistanceChangeDuration * smoothTimeMultiplier, float.MaxValue, DeltaTime);

            orientationOffset = Quaternion.RotateTowards(orientationOffset, targetOrientationOffset, Time.deltaTime * 60f);
        }
        else
        {
            pedestal = clampedPedestalTarget;
            orientation = orientationTarget;
            distanceFromAnchor = targetDistanceFromAnchor;
            orientationOffset = targetOrientationOffset;
        }
    }

    public void ControlEnable()
    {
        controlEnabled = true;
        target = null;
        targetOrientationOffset = Quaternion.identity;
    }

    public void AttachToTarget(Transform newTarget)
    {
        controlEnabled = false;
        target = newTarget;
        timeOfTargetAttachment = Time.time;
    }

    void FollowTarget()
    {
        if (!target)
            return;

        orientationTarget = Quaternion.LookRotation(target.forward).eulerAngles;

        targetOrientationOffset = Quaternion.Inverse(Quaternion.Euler(orientationTarget)) * target.rotation;

        targetDistanceFromAnchor = (Vector3.Cross(anchor.position - target.position, Vector3.up)).magnitude;
        targetDistanceFromAnchorLastFollow = targetDistanceFromAnchor;

        pedestalTarget = (Quaternion.Euler(orientationTarget) * Vector3.forward * targetDistanceFromAnchor).y + target.position.y;
        pedestalTargetLastFollow = pedestalTarget;
    }
}