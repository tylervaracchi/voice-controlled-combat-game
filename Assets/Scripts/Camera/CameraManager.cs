// Copyright (c) 2024 Tyler Varacchi. All Rights Reserved.
// This code is proprietary. Unauthorized copying or use is prohibited.
using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

/// <summary>
/// Third-person camera controller with mouse orbit controls.
/// Follows a target with configurable offset, sensitivity, and angle limits.
/// </summary>
public class CameraManager : MonoBehaviour
{
    #region Settings

    [SerializeField] private Transform target;
    [SerializeField] private Vector3 targetOffset = new Vector3(0, 1.5f, 0);

    [SerializeField] private MouseSensitivity mouseSensitivity;
    [SerializeField] private CameraAngle cameraAngle;

    #endregion

    #region State

    private float distanceToPlayer;
    private Vector2 input;
    private CameraRotation cameraRotation;

    private Vector3 initialPosition;
    private Quaternion initialRotation;

    #endregion

    private void Awake()
    {
        distanceToPlayer = Vector3.Distance(transform.position, target.position + targetOffset);
        SaveInitialCameraState();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void SaveInitialCameraState()
    {
        initialPosition = transform.position;
        initialRotation = transform.rotation;
    }

    /// <summary>
    /// Reset camera to initial position (useful for scene transitions).
    /// </summary>
    public void ResetCameraState()
    {
        transform.position = initialPosition;
        transform.rotation = initialRotation;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "GameplayScene")
        {
            ResetCameraState();
        }
    }

    /// <summary>
    /// Input System callback for mouse look.
    /// Only processes input when left mouse button is held.
    /// </summary>
    public void Look(InputAction.CallbackContext context)
    {
        if (Mouse.current.leftButton.isPressed)
        {
            input = context.ReadValue<Vector2>();
        }
    }

    private void Update()
    {
        // Apply mouse input to camera rotation
        cameraRotation.Yaw += input.x * mouseSensitivity.horizontal * BoolToInt(mouseSensitivity.invertHorizontal) * Time.deltaTime;
        cameraRotation.Pitch += input.y * mouseSensitivity.vertical * BoolToInt(mouseSensitivity.invertVertical) * Time.deltaTime;
        cameraRotation.Pitch = Mathf.Clamp(cameraRotation.Pitch, cameraAngle.min, cameraAngle.max);
    }

    private void LateUpdate()
    {
        // Position camera behind target at specified distance
        transform.eulerAngles = new Vector3(cameraRotation.Pitch, cameraRotation.Yaw, 0.0f);
        transform.position = (target.position + targetOffset) - transform.forward * distanceToPlayer;
    }

    private static int BoolToInt(bool b) => b ? 1 : -1;

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}

/// <summary>
/// Mouse sensitivity settings for camera control.
/// </summary>
[Serializable]
public struct MouseSensitivity
{
    public float horizontal;
    public float vertical;
    public bool invertHorizontal;
    public bool invertVertical;
}

/// <summary>
/// Current camera rotation state.
/// </summary>
public struct CameraRotation
{
    public float Pitch;
    public float Yaw;
}

/// <summary>
/// Pitch angle limits for camera.
/// </summary>
[Serializable]
public struct CameraAngle
{
    public float min;
    public float max;
}
