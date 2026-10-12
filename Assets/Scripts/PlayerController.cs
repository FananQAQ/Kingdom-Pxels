using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 3f;
    public float jumpForce = 7f;
    [Range(0f, 1f)]
    public float jumpCutMultiplier = 0.4f;
    public float fallGravityMultiplier = 1.8f;
    public float rollSpeed = 6f;
    public Transform groundCheck;
    public float groundCheckRadius = 0.08f;
    public LayerMask groundLayer;
    public Collider2D moveBounds;
    public Collider2D attackHitbox;

    Rigidbody2D rb;
    Animator animator;
    SpriteRenderer spriteRenderer;
    Collider2D body;

    bool isGrounded;
    bool isAttacking;
    bool isRolling;
    float baseGravityScale;
    float rollDirection = 1f;
    Vector2 attackHitboxLocalPos;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        body = GetComponent<Collider2D>();
        baseGravityScale = rb.gravityScale;
        if (attackHitbox != null)
        {
            attackHitboxLocalPos = attackHitbox.transform.localPosition;
            attackHitbox.enabled = false;
        }
    }

    void Update()
    {
        float input = Input.GetAxisRaw("Horizontal");
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        HandleAttack();
        HandleRoll(input);
        HandleMove(input);
        HandleJump();
        HandleGravity();
        HandleAnimator(input);
        HandleFacing(input);
        UpdateAttackHitboxFacing();

        // 攻击被打断或结束时，兜底关掉判定（防止 Animation Event 没跑到 Disable）
        if (!isAttacking)
            DisableAttackHitbox();

        ClampToMoveBounds();
    }

    // Animation Event：刀光出现时调用
    public void EnableAttackHitbox()
    {
        if (attackHitbox != null && isAttacking && !isRolling)
            attackHitbox.enabled = true;
    }

    // Animation Event：刀光结束时调用
    public void DisableAttackHitbox()
    {
        if (attackHitbox != null)
            attackHitbox.enabled = false;
    }

    void HandleAttack()
    {
        AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);
        bool inAttack1 = state.IsName("Attack1");
        bool inAttack2 = state.IsName("Attack2");
        bool inAttack3 = state.IsName("Attack3");
        isAttacking = inAttack1 || inAttack2 || inAttack3;

        // 地上起手，或连招中再按 J；第三刀 / 翻滚期间不再起手
        if (Input.GetKeyDown(KeyCode.J) && isGrounded && !inAttack3 && !isRolling)
            animator.SetTrigger("Attack");
    }

    void HandleRoll(float input)
    {
        isRolling = animator.GetCurrentAnimatorStateInfo(0).IsName("Roll");

        // 地上可翻滚；攻击中也可打断进翻滚，翻滚中不能再按 L
        if (Input.GetKeyDown(KeyCode.L) && isGrounded && !isRolling)
        {
            if (input != 0f)
                rollDirection = input > 0f ? 1f : -1f;
            else
                rollDirection = spriteRenderer.flipX ? -1f : 1f;

            animator.ResetTrigger("Attack");
            animator.SetTrigger("Roll");
            isAttacking = false;
            isRolling = true;
            DisableAttackHitbox();
        }
    }

    void HandleMove(float input)
    {
        float move = 0f;
        if (isRolling)
            move = FilterMoveAgainstBounds(rollDirection);
        else if (!isAttacking)
            move = FilterMoveAgainstBounds(input);

        float speed = isRolling ? rollSpeed : moveSpeed;
        rb.velocity = new Vector2(move * speed, rb.velocity.y);
    }

    void HandleJump()
    {
        // 按下跳跃：同一帧起跳并触发 Jump 动画（前摇与离地同步）
        if (Input.GetButtonDown("Jump") && isGrounded && !isAttacking && !isRolling)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            animator.SetTrigger("Jump");
        }

        // 松开跳跃：如果还在上升，立刻削弱向上速度（短跳）
        if (Input.GetButtonUp("Jump") && rb.velocity.y > 0f)
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * jumpCutMultiplier);
    }

    void HandleGravity()
    {
        if (rb.velocity.y < 0f)
            rb.gravityScale = baseGravityScale * fallGravityMultiplier;
        else
            rb.gravityScale = baseGravityScale;
    }

    void HandleAnimator(float input)
    {
        animator.SetBool("IsWalking", input != 0f && isGrounded && !isAttacking && !isRolling);
        animator.SetBool("IsGrounded", isGrounded);
        animator.SetFloat("VelocityY", rb.velocity.y);
    }

    void HandleFacing(float input)
    {
        if (isRolling)
        {
            spriteRenderer.flipX = rollDirection < 0f;
            return;
        }

        if (!isAttacking && input != 0f)
            spriteRenderer.flipX = input < 0f;
    }

    void UpdateAttackHitboxFacing()
    {
        if (attackHitbox == null)
            return;

        Vector2 p = attackHitboxLocalPos;
        if (spriteRenderer.flipX)
            p.x = -p.x;
        attackHitbox.transform.localPosition = p;
    }

    float FilterMoveAgainstBounds(float move)
    {
        if (moveBounds == null || move == 0f)
            return move;

        Bounds b = moveBounds.bounds;
        float half = body != null ? body.bounds.extents.x : 0.1f;
        float x = rb.position.x;

        if (move < 0f && x <= b.min.x + half)
            return 0f;
        if (move > 0f && x >= b.max.x - half)
            return 0f;

        return move;
    }

    void ClampToMoveBounds()
    {
        if (moveBounds == null)
            return;

        Bounds b = moveBounds.bounds;
        float half = body != null ? body.bounds.extents.x : 0.1f;
        Vector2 p = rb.position;
        float minX = b.min.x + half;
        float maxX = b.max.x - half;

        if (p.x < minX)
        {
            p.x = minX;
            rb.velocity = new Vector2(0f, rb.velocity.y);
        }
        else if (p.x > maxX)
        {
            p.x = maxX;
            rb.velocity = new Vector2(0f, rb.velocity.y);
        }

        rb.position = p;
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck == null)
            return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}
