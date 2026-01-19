// Copyright (c) 2024 Tyler Varacchi. All Rights Reserved.
// This code is proprietary. Unauthorized copying or use is prohibited.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// AI opponent state for the finite state machine.
/// </summary>
public enum AIState
{
    Idle,
    Punch,
    Block,
    MoveForward,
    MoveBackward
}

/// <summary>
/// Finite State Machine AI controller for combat opponent.
/// Evaluates game state (distance, health) to make tactical decisions.
/// Reacts to player attacks with probabilistic blocking.
/// </summary>
public class AIController : MonoBehaviour
{
    #region State Machine
    private AIState currentState = AIState.Idle;
    private float stateChangeBuffer = 0.5f;
    private float lastStateChangeTime;
    #endregion

    #region Components
    private Animator animator;
    private CharacterController characterController;
    public GameObject player;
    private PlayerController playerController;
    #endregion

    #region Movement Settings
    public float speed = 4.0f;
    public float AttackRange = 5f;
    public float SafeDistance = 10f;
    #endregion

    #region Combat Settings
    public float punchCooldown = 10f;
    private float lastPunchTime;
    private float blockDuration = 1.5f;
    private bool isBlocking = false;
    private bool isMoving = false;
    #endregion

    #region Game State
    public float DistanceToPlayer { get; private set; }
    public float AIHealth { get; private set; }
    public float PlayerHealth { get; private set; }
    #endregion

    void Start()
    {
        playerController = player.GetComponent<PlayerController>();
        animator = GetComponent<Animator>();
        characterController = GetComponent<CharacterController>();

        ValidateComponents();
    }

    private void ValidateComponents()
    {
        if (animator == null)
            Debug.LogError("AIController: Missing Animator component.");

        if (characterController == null)
            Debug.LogError("AIController: Missing CharacterController component.");
    }

    void Update()
    {
        UpdateGameState();
        FacePlayer();

        // Only evaluate new state if not in middle of attack animation
        if (!IsInAttackAnimation())
        {
            AIState nextState = EvaluateGameState();

            if (ShouldChangeState(nextState))
            {
                TransitionToState(nextState);
            }
        }

        ExecuteCurrentState();
    }

    #region State Management

    /// <summary>
    /// Evaluates current game conditions and returns optimal AI state.
    /// Decision tree based on: player attacking, distance, punch cooldown.
    /// </summary>
    private AIState EvaluateGameState()
    {
        bool isPlayerAttacking = playerController.isPunching;

        // Priority 1: Block if player is attacking and in range
        if (isPlayerAttacking && DistanceToPlayer < 3.4f && !isBlocking && Random.Range(0, 100) < 80)
        {
            return AIState.Block;
        }
        // Priority 2: Attack if in range and cooldown elapsed
        else if (DistanceToPlayer < 2f && Time.time - lastPunchTime > punchCooldown)
        {
            return AIState.Punch;
        }
        // Priority 3: Close distance if too far
        else if (DistanceToPlayer > SafeDistance + 1)
        {
            return AIState.MoveForward;
        }
        // Priority 4: Create distance if too close
        else if (DistanceToPlayer < AttackRange - 1)
        {
            return AIState.MoveBackward;
        }

        return AIState.Idle;
    }

    private void TransitionToState(AIState nextState)
    {
        ExitState(currentState);
        EnterState(nextState);
        currentState = nextState;
        lastStateChangeTime = Time.time;
    }

    private void EnterState(AIState state)
    {
        ResetAnimatorTriggers();

        switch (state)
        {
            case AIState.Idle:
                animator.SetBool("isMoving", false);
                animator.SetBool("isGrounded", true);
                animator.SetBool("isAttacking", false);
                isMoving = false;
                break;

            case AIState.Punch:
                animator.SetTrigger("punch");
                animator.SetBool("isAttacking", true);
                lastPunchTime = Time.time;
                StartCoroutine(ResetAfterAction("punch", "isAttacking", 2.09f));
                break;

            case AIState.Block:
                animator.SetTrigger("block");
                animator.SetBool("isAttacking", true);
                StartCoroutine(ResetAfterAction("block", "isAttacking", 1.49f));
                break;

            case AIState.MoveForward:
                animator.SetBool("isMoving", true);
                animator.SetBool("isForward", true);
                animator.SetBool("isGrounded", true);
                animator.SetBool("isAttacking", false);
                isMoving = true;
                break;

            case AIState.MoveBackward:
                animator.SetBool("isMoving", true);
                animator.SetBool("isForward", false);
                animator.SetBool("isGrounded", true);
                animator.SetBool("isAttacking", false);
                isMoving = true;
                break;
        }
    }

    private void ExitState(AIState state)
    {
        switch (state)
        {
            case AIState.MoveForward:
            case AIState.MoveBackward:
                animator.SetBool("isMoving", false);
                break;
        }
    }

    private void ExecuteCurrentState()
    {
        switch (currentState)
        {
            case AIState.MoveForward:
                characterController.Move(transform.forward * speed * Time.deltaTime);
                break;

            case AIState.MoveBackward:
                characterController.Move(-transform.forward * speed * Time.deltaTime);
                break;
        }
    }

    #endregion

    #region Combat Reactions

    /// <summary>
    /// Called when player initiates an attack. AI may react with block.
    /// </summary>
    public bool OnPlayerAttack(string attackType)
    {
        if (attackType == "Punch" || attackType == "Kick" || attackType == "UpperCut")
        {
            if (!isBlocking && Random.Range(0, 100) < 50)
            {
                StartCoroutine(BlockCoroutine());
            }
            return true;
        }
        return false;
    }

    private IEnumerator BlockCoroutine()
    {
        isBlocking = true;
        TransitionToState(AIState.Block);
        yield return new WaitForSeconds(blockDuration);
        isBlocking = false;
        TransitionToState(AIState.Idle);
    }

    #endregion

    #region Helpers

    private void UpdateGameState()
    {
        DistanceToPlayer = Vector3.Distance(transform.position, player.transform.position);

        Health aiHealthComponent = GetComponent<Health>();
        Health playerHealthComponent = player.GetComponent<Health>();

        if (aiHealthComponent != null)
            AIHealth = aiHealthComponent.currentHealth;

        if (playerHealthComponent != null)
            PlayerHealth = playerHealthComponent.currentHealth;
    }

    private void FacePlayer()
    {
        Vector3 direction = (player.transform.position - transform.position).normalized;
        direction.y = 0;
        Quaternion lookRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * speed);
    }

    private void ResetAnimatorTriggers()
    {
        animator.ResetTrigger("punch");
        animator.ResetTrigger("block");
    }

    private bool IsInAttackAnimation()
    {
        return animator.GetCurrentAnimatorStateInfo(0).IsName("Punch") &&
               animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1.0f;
    }

    private bool ShouldChangeState(AIState nextState)
    {
        return nextState != currentState &&
               Time.time - lastStateChangeTime > stateChangeBuffer;
    }

    private IEnumerator ResetAfterAction(string triggerName, string boolName, float delay)
    {
        yield return new WaitForSeconds(delay);

        animator.ResetTrigger(triggerName);
        animator.SetBool(boolName, false);
        TransitionToState(AIState.Idle);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("PlayerPunch"))
        {
            Health aiHealthComponent = GetComponent<Health>();
            if (aiHealthComponent != null)
            {
                aiHealthComponent.TakeDamage(10);
            }
        }
    }

    #endregion

    /// <summary>
    /// Returns current game state for external systems (UI, analytics).
    /// </summary>
    public GameState GetGameState()
    {
        return new GameState(DistanceToPlayer, AIHealth, PlayerHealth, AIHealth, PlayerHealth);
    }
}

/// <summary>
/// Data container for game state snapshot.
/// Used for AI decision making and potential ML training data.
/// </summary>
public class GameState
{
    public float DistanceToPlayer { get; set; }
    public float AIHealth { get; set; }
    public float PlayerHealth { get; set; }
    public float AIStamina { get; set; }
    public float PlayerStamina { get; set; }

    public GameState(float distanceToPlayer, float aiHealth, float playerHealth, float aiStamina, float playerStamina)
    {
        DistanceToPlayer = distanceToPlayer;
        AIHealth = aiHealth;
        PlayerHealth = playerHealth;
        AIStamina = aiStamina;
        PlayerStamina = playerStamina;
    }
}
