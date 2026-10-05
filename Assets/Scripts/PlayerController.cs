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

    Rigidbody2D rb;
    Animator animator;
    SpriteRenderer spriteRenderer;
    Collider2D body;

    bool isGrounded;
    float baseGravityScale;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        body = GetComponent<Collider2D>();
        baseGravityScale = rb.gravityScale;
    }

    void Update()
    {
        float input = Input.GetAxisRaw("Horizontal");
        float move = FilterMoveAgainstBounds(input);

        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        rb.velocity = new Vector2(move * moveSpeed, rb.velocity.y);

        // 按下跳跃：同一帧起跳并触发 Jump 动画（前摇与离地同步）
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            animator.SetTrigger("Jump");
        }

        // 松开跳跃：如果还在上升，立刻削弱向上速度（短跳）
        if (Input.GetButtonUp("Jump") && rb.velocity.y > 0f)
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * jumpCutMultiplier);

        // 下落时加重重力，手感更接近空洞骑士
        if (rb.velocity.y < 0f)
            rb.gravityScale = baseGravityScale * fallGravityMultiplier;
        else
            rb.gravityScale = baseGravityScale;

        animator.SetBool("IsWalking", input != 0f && isGrounded);
        animator.SetBool("IsGrounded", isGrounded);
        animator.SetFloat("VelocityY", rb.velocity.y);

        if (input != 0f)
            spriteRenderer.flipX = input < 0f;

        ClampToMoveBounds();
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
