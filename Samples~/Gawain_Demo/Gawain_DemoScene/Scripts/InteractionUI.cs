#pragma warning disable 0649

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.EventSystems;
using UnityEngine.Playables;
using UnityEngine.UI;

public class InteractionUI : MonoBehaviour
{
    [Header("Camera")]
    [SerializeField] Toggle toggleMixed;
    [SerializeField] Camera cameraMixed;
    [SerializeField] Toggle toggleFace;
    [SerializeField] Camera cameraFace;
    [SerializeField] Toggle toggleMidshot;
    [SerializeField] Camera cameraMidshot;
    [SerializeField] Toggle toggleBody;
    [SerializeField] Camera cameraBody;

    [Space]
    [SerializeField] Animator cameraAnimator;
    [SerializeField] float cameraFaceSpeedMultiplier = 0.5f;

    [Space]
    [SerializeField] Toggle toggleFreePivot;
    [SerializeField] CameraControl freePivotController;

    [Header("Character Animation")]
    [SerializeField] Toggle toggleTalking;
    [SerializeField] PlayableDirector timelineTalking;

    [SerializeField] Toggle toggleWalking;
    [SerializeField] Toggle toggleTPose;
    [SerializeField] Animator characterAnimator;
    [SerializeField] Rig characterRig;


    [Header("Lighting")]
    [SerializeField] Button buttonPreviousLighting;
    [SerializeField] Text labelCurrentLighting;
    [SerializeField] Button buttonNextLighting;
    [SerializeField] Transform lightingSetupParent;
    int activeLightingSetupIndex;

    [System.NonSerialized] bool initialized; // Not serialized to detect domain reload as UI event references are not serialized
    void OnEnable()
    {
        if (!initialized)
            Initialize();
    }

    void Start()
    {
        SetDefaults();
    }

    void Initialize()
    {
        SetupCameraToggle(cameraFace, toggleFace, true);
        SetupCameraToggle(cameraMidshot, toggleMidshot, false);
        SetupCameraToggle(cameraBody, toggleBody, false);
        SetupCameraToggle(cameraMixed, toggleMixed, false);
        toggleFreePivot.onValueChanged.AddListener(HandleFreePivotModeToggle);

        toggleWalking.onValueChanged.AddListener((x) => characterRig.weight = x ? 0f : 1f);
        toggleWalking.onValueChanged.AddListener((x) => characterAnimator.SetBool("Walk", x));
        toggleWalking.onValueChanged.AddListener((x) => ForceToggleValue(toggleTalking, false));

        toggleTalking.onValueChanged.AddListener(HandleTalkingToggle);

        buttonPreviousLighting.onClick.AddListener(HandleLightingPrevious);
        buttonNextLighting.onClick.AddListener(HandleLightingNext);

        initialized = true;
    }

    void SetDefaults()
    {
        ForceToggleValue(toggleFace, true);
        ForceToggleValue(toggleTPose, true);
        ForceToggleValue(toggleTalking, true);
        ActivateLighting(0);
    }

    void Update()
    {
        if ((Input.GetMouseButton(0) || Input.GetMouseButton(1)) && !EventSystem.current.IsPointerOverGameObject())
        {
            toggleFreePivot.isOn = true;
        }

        for (int i = 0; i < lightingSetupParent.childCount; i++)
        {
            if (Input.GetKeyDown(i + KeyCode.Alpha1))
            {
                ActivateLighting(i);
            }
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            toggleMixed.isOn = true;
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            toggleBody.isOn = true;
        }
        if (Input.GetKeyDown(KeyCode.W))
        {
            toggleMidshot.isOn = true;
        }
        if (Input.GetKeyDown(KeyCode.Q))
        {
            toggleFace.isOn = true;
        }
        if (Input.GetKeyDown(KeyCode.T))
        {
            toggleFreePivot.isOn = true;
        }

        if (Input.GetKeyDown(KeyCode.A))
        {
            toggleTPose.isOn = true;
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            toggleWalking.isOn = true;
        }
        if (Input.GetKeyDown(KeyCode.D))
        {
            toggleTalking.isOn = !toggleTalking.isOn;
        }

        // Resume playback if state doesn't match, for example after Domain Reload. Doesn't work in Initialize - too early for Timeline
        if (toggleTalking.isOn && timelineTalking.state != PlayState.Playing)
        {
            timelineTalking.Play();
        }
    }

    void SetupCameraToggle(Camera camera, Toggle toggle, bool focusing)
    {
        toggle.onValueChanged.AddListener((x) => HandleCameraToggle(x, camera.transform, focusing));
        camera.gameObject.SetActive(false);
    }

    void ForceToggleValue(Toggle toggle, bool shouldBeOn)
    {
        if (toggle.isOn == shouldBeOn)
        {
            toggle.onValueChanged.Invoke(shouldBeOn);
        }
        else
        {
            toggle.isOn = shouldBeOn;
        }
    }

    void HandleCameraToggle(bool enable, Transform targetCamera, bool focusing)
    {
        if (enable)
        {
            freePivotController.AttachToTarget(targetCamera);
            if (freePivotController.focusHandler != null)
            {
                if (focusing)
                    freePivotController.focusHandler.StartFocus();
                else
                    freePivotController.focusHandler.CancelFocus(instantly: true);
            }
        }
    }

    void HandleFreePivotModeToggle(bool enable)
    {
        cameraAnimator.enabled = !enable;
        if (enable)
        {
            freePivotController.ControlEnable();
        }
    }

    void HandleTalkingToggle(bool enable)
    {
        if (enable)
        {
            timelineTalking.Play(timelineTalking.playableAsset, DirectorWrapMode.Loop);
            ForceToggleValue(toggleFace, true);
            cameraAnimator.SetFloat("Speed", cameraFaceSpeedMultiplier);
            cameraAnimator.SetTrigger("Reset");
        }
        else
        {
            timelineTalking.Pause();
            cameraAnimator.SetFloat("Speed", 1f);
        }
    }

    void HandleLightingPrevious()
    {
        ActivateLighting(activeLightingSetupIndex - 1);
    }

    void HandleLightingNext()
    {
        ActivateLighting(activeLightingSetupIndex + 1);
    }

    void ActivateLighting(int index)
    {
        lightingSetupParent.GetChild(activeLightingSetupIndex).gameObject.SetActive(false);
        activeLightingSetupIndex = (index + lightingSetupParent.childCount) % lightingSetupParent.childCount;

        var newLighting = lightingSetupParent.GetChild(activeLightingSetupIndex);
        newLighting.gameObject.SetActive(true);
        labelCurrentLighting.text = newLighting.name;
    }
}