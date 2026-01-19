using UnityEngine;

/// <summary>
/// Handles collision-based hit detection for combat system.
/// Attach to hitbox colliders on character limbs (hands, feet).
/// Determines damage based on current attack animation and applies block reduction.
/// </summary>
public class HitDetection : MonoBehaviour
{
    [Header("References")]
    public HealthManager healthManager;
    public string characterTag; // "Player" or "AI"

    private Animator animator;
    private bool damageApplied = false;

    #region Damage Values

    private const float PUNCH_DAMAGE = 10f;
    private const float KICK_DAMAGE = 5f;
    private const float UPPERCUT_DAMAGE = 15f;
    private const float BLOCK_REDUCTION = 0.25f;

    #endregion

    private void Start()
    {
        animator = GetComponentInParent<Animator>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        HandleCollision(collision.gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        HandleCollision(other.gameObject);
    }

    /// <summary>
    /// Process collision with another character.
    /// Validates attack state, applies damage, and triggers hit reaction.
    /// </summary>
    private void HandleCollision(GameObject other)
    {
        // Only process hits on valid targets during attacks
        bool isValidTarget = (other.CompareTag("Player") || other.CompareTag("AI")) && 
                             other.tag != characterTag;

        if (isValidTarget && IsAttacking() && CanApplyDamage())
        {
            float damageAmount = GetDamageAmount();

            // Reduce damage if target is blocking
            if (IsBlocking(other))
            {
                damageAmount *= BLOCK_REDUCTION;
            }

            healthManager.TakeDamage(other.tag, damageAmount);

            // Trigger hit reaction animation on target
            Animator otherAnimator = other.GetComponentInParent<Animator>();
            if (otherAnimator != null)
            {
                otherAnimator.SetTrigger("hit");
            }

            damageApplied = true;
        }
    }

    /// <summary>
    /// Check if character is currently in an attack animation.
    /// </summary>
    private bool IsAttacking()
    {
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        return stateInfo.IsName("kick") ||
               stateInfo.IsName("upperCut") ||
               stateInfo.IsName("punch");
    }

    /// <summary>
    /// Prevent multiple damage applications per attack.
    /// Only applies damage in first half of animation.
    /// </summary>
    private bool CanApplyDamage()
    {
        float normalizedTime = animator.GetCurrentAnimatorStateInfo(0).normalizedTime % 1;
        return normalizedTime < 0.5f && !damageApplied;
    }

    /// <summary>
    /// Get damage value based on current attack type.
    /// </summary>
    private float GetDamageAmount()
    {
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);

        if (stateInfo.IsName("kick"))
            return KICK_DAMAGE;
        else if (stateInfo.IsName("upperCut"))
            return UPPERCUT_DAMAGE;
        else if (stateInfo.IsName("punch"))
            return PUNCH_DAMAGE;

        return 0f;
    }

    /// <summary>
    /// Check if target character is currently blocking.
    /// </summary>
    private bool IsBlocking(GameObject other)
    {
        Animator otherAnimator = other.GetComponentInParent<Animator>();
        if (otherAnimator != null)
        {
            return otherAnimator.GetCurrentAnimatorStateInfo(0).IsName("block");
        }
        return false;
    }

    private void Update()
    {
        // Reset damage flag when attack animation completes or restarts
        if (IsAttacking())
        {
            float normalizedTime = animator.GetCurrentAnimatorStateInfo(0).normalizedTime % 1;

            if (normalizedTime < 0.5f && damageApplied)
            {
                damageApplied = false;
            }
            else if (normalizedTime > 2f)
            {
                damageApplied = false;
            }
        }
        else
        {
            damageApplied = false;
        }
    }
}
