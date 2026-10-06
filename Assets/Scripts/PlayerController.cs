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
    public Transform groundCheck;
    public float groundCheckRadius = 0.08f;
    public LayerMask groundLayer;
    public Collider2D moveBounds;
    public Collider2D attackHitbox;
    [Range(0f, 1f)]
    public float attackHitStart = 0.33f;
    [Range(0f, 1f)]
    public float attackHitEnd = 0.75f;

    Rigidbody2D rb;
    Animator animator;
    SpriteRenderer spriteRenderer;
    Collider2D body;

    bool isGrounded;
    bool isAttacking;
    float baseGravityScale;
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

        HandleAttack();         //攻击
        HandleMove(input);      //移动
        HandleJump();           //跳跃
        HandleGravity();        //重力
        HandleAnimator(input);  //动画
        HandleFacing(input);    //面向

        ClampToMoveBounds();    //限制移动范围
    }

    void HandleAttack()
    {
        AnimatorStateInfo attackState = animator.GetCurrentAnimatorStateInfo(0);
        bool inAttack1 = attackState.IsName("Attack1");
        bool inAttack2 = attackState.IsName("Attack2");
        bool inAttack3 = attackState.IsName("Attack3");
        isAttacking = inAttack1 || inAttack2 || inAttack3;

        // 地上起手，或连招中再按 J；第三刀期间不再起手
        if (Input.GetKeyDown(KeyCode.J) && isGrounded && !inAttack3)
            animator.SetTrigger("Attack");

        UpdateAttackHitbox();
    }

    void HandleMove(float input)
    {
        float move = 0f;
        if (!isAttacking)
            move = FilterMoveAgainstBounds(input);

        rb.velocity = new Vector2(move * moveSpeed, rb.velocity.y);
    }

    void HandleJump()
    {
        // 按下跳跃：同一帧起跳并触发 Jump 动画（前摇与离地同步）
        if (Input.GetButtonDown("Jump") && isGrounded && !isAttacking)
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
        animator.SetBool("IsWalking", input != 0f && isGrounded && !isAttacking);
        animator.SetBool("IsGrounded", isGrounded);
        animator.SetFloat("VelocityY", rb.velocity.y);
    }

    void HandleFacing(float input)
    {
        if (!isAttacking && input != 0f)
            spriteRenderer.flipX = input < 0f;
    }

    void UpdateAttackHitbox()
    {
        if (attackHitbox == null)
            return;

        float t = animator.GetCurrentAnimatorStateInfo(0).normalizedTime % 1f;
        attackHitbox.enabled = isAttacking && t >= attackHitStart && t <= attackHitEnd;

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
