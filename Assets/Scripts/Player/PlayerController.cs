// Copyright (c) 2024 Tyler Varacchi. All Rights Reserved.
// This code is proprietary. Unauthorized copying or use is prohibited.
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Windows.Speech;
using System.Linq;
using UnityEngine.InputSystem;

/// <summary>
/// Voice-controlled player character for accessible fighting game.
/// Uses Windows Speech Recognition to map spoken commands to combat actions.
/// 
/// Supported voice commands:
/// - "advance" : Move forward
/// - "back"    : Move backward  
/// - "jump"    : Jump
/// - "punch"   : Punch attack
/// - "kick"    : Kick attack
/// - "upper cut": Uppercut attack
/// - "block"   : Defensive block
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    #region Voice Recognition
    private KeywordRecognizer keywordRecognizer;
    private Dictionary<string, System.Action> voiceCommands = new Dictionary<string, System.Action>();
    #endregion

    #region Movement
    private Vector2 input;
    private CharacterController characterController;
    private Vector3 direction;
    [SerializeField] private Movement movement;
    #endregion

    #region Combat State
    public bool isAttacking;
    public bool isPunching = false;
    #endregion

    #region Rotation
    [SerializeField] private float rotationSpeed = 500f;
    private Camera mainCamera;
    #endregion

    #region Gravity
    private float gravity = -9.81f;
    [SerializeField] private float gravityMultiplier = 3.0f;
    private float velocity;
    #endregion

    #region Jumping
    [SerializeField] private float jumpPower;
    private int numberOfJumps;
    [SerializeField] private int maxNumberOfJumps = 2;
    #endregion

    private Animator animator;

    [Serializable]
    public struct Movement
    {
        public float speed;
        public float multiplier;
        public float acceleration;

        [HideInInspector] public bool isSprinting;
        [HideInInspector] public float currentSpeed;
    }

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        mainCamera = Camera.main;

        InitializeVoiceCommands();
    }

    /// <summary>
    /// Sets up the voice recognition system with combat command mappings.
    /// Uses Unity's KeywordRecognizer for Windows Speech Recognition API.
    /// </summary>
    private void InitializeVoiceCommands()
    {
        // Map voice commands to actions
        voiceCommands.Add("advance", Forward);
        voiceCommands.Add("back", Backward);
        voiceCommands.Add("jump", Jump);
        voiceCommands.Add("punch", Punch);
        voiceCommands.Add("kick", Kick);
        voiceCommands.Add("upper cut", UpperCut);
        voiceCommands.Add("block", Block);

        // Initialize and start the recognizer
        keywordRecognizer = new KeywordRecognizer(voiceCommands.Keys.ToArray());
        keywordRecognizer.OnPhraseRecognized += OnVoiceCommandRecognized;
        keywordRecognizer.Start();
    }

    /// <summary>
    /// Callback fired when speech recognition detects a valid command.
    /// </summary>
    private void OnVoiceCommandRecognized(PhraseRecognizedEventArgs speech)
    {
        Debug.Log($"Voice command recognized: {speech.text}");
        voiceCommands[speech.text].Invoke();
    }

    private void Update()
    {
        ApplyGravity();
        ApplyMovement();
    }

    #region Physics

    private void ApplyGravity()
    {
        if (IsGrounded() && velocity < 0.0f)
        {
            velocity = -1.0f;
        }
        else
        {
            velocity += gravity * gravityMultiplier * Time.deltaTime;
        }

        direction.y = velocity;
    }

    private void ApplyRotation()
    {
        if (input.sqrMagnitude == 0) return;

        direction = Quaternion.Euler(0.0f, mainCamera.transform.eulerAngles.y, 0.0f) * new Vector3(input.x, 0.0f, input.y);
        var targetRotation = Quaternion.LookRotation(direction, Vector3.up);

        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }

    private void ApplyMovement()
    {
        var targetSpeed = movement.isSprinting ? movement.speed * movement.multiplier : movement.speed;
        movement.currentSpeed = Mathf.MoveTowards(movement.currentSpeed, targetSpeed, movement.acceleration * Time.deltaTime);

        characterController.Move(direction * movement.currentSpeed * Time.deltaTime);
        animator.SetBool("isForward", direction.z > 0);
    }

    private bool IsGrounded() => characterController.isGrounded;

    #endregion

    #region Movement Commands

    private void Forward()
    {
        input = new Vector2(0, 1);
        direction = new Vector3(input.x, 0.0f, input.y);

        animator.SetBool("isMoving", true);
        animator.SetBool("isForward", true);

        StartCoroutine(ResetMovementAfterDelay(1.0f));
    }

    private void Backward()
    {
        input = new Vector2(0, -1);
        direction = new Vector3(input.x, 0.0f, input.y);

        animator.SetBool("isMoving", true);
        animator.SetBool("isForward", false);

        StartCoroutine(ResetMovementAfterDelay(1.0f));
    }

    private void Jump()
    {
        if (!characterController.isGrounded) return;

        direction = new Vector3(input.x, jumpPower, input.y);

        animator.SetTrigger("startJump");
        animator.SetBool("isJumping", true);
        animator.SetBool("isGrounded", false);
        numberOfJumps++;
        velocity = jumpPower;

        StartCoroutine(ResetJumpAfterDelay(1.53f));
    }

    private IEnumerator ResetMovementAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        input = Vector2.zero;
        direction = Vector3.zero;
        animator.SetBool("isMoving", false);
        animator.SetBool("isForward", false);
    }

    private IEnumerator ResetJumpAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        input = Vector2.zero;
        direction = Vector3.zero;
        animator.ResetTrigger("startJump");
        animator.SetBool("isJumping", false);
        animator.SetBool("isGrounded", true);
    }

    #endregion

    #region Combat Commands

    public void Punch()
    {
        isPunching = true;
        animator.SetTrigger("punch");
        animator.SetBool("isAttacking", true);
        StartCoroutine(ResetCombatAfterDelay("punch", "isAttacking", 2.09f));
    }

    public void Kick()
    {
        animator.SetTrigger("kick");
        animator.SetBool("isAttacking", true);
        StartCoroutine(ResetCombatAfterDelay("kick", "isAttacking", 1.13f));
    }

    public void UpperCut()
    {
        animator.SetTrigger("upperCut");
        animator.SetBool("isAttacking", true);
        StartCoroutine(ResetCombatAfterDelay("upperCut", "isAttacking", 3.08f));
    }

    public void Block()
    {
        animator.SetTrigger("block");
        animator.SetBool("isAttacking", true);
        StartCoroutine(ResetCombatAfterDelay("block", "isAttacking", 1.49f));
    }

    private IEnumerator ResetCombatAfterDelay(string triggerName, string boolName, float delay)
    {
        yield return new WaitForSeconds(delay);

        animator.ResetTrigger(triggerName);
        animator.SetBool(boolName, false);
        isPunching = false;
        isAttacking = false;
    }

    #endregion

    #region Input System (Fallback)

    /// <summary>
    /// Traditional input handler for controller/keyboard fallback.
    /// Voice commands are primary, but this allows standard input as backup.
    /// </summary>
    public void Move(InputAction.CallbackContext context)
    {
        input = context.ReadValue<Vector2>();
        direction = new Vector3(input.x, 0.0f, input.y);

        if (input.y < 0)
        {
            direction = -transform.forward;
        }

        animator.SetBool("isMoving", input != Vector2.zero);
        animator.SetBool("isForward", direction.z > 0);
    }

    #endregion

    private void OnDestroy()
    {
        // Clean up voice recognition
        if (keywordRecognizer != null && keywordRecognizer.IsRunning)
        {
            keywordRecognizer.Stop();
            keywordRecognizer.Dispose();
        }
    }
}
