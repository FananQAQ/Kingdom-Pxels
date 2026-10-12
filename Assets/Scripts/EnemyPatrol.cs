using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyPatrol : MonoBehaviour
{
    public float moveSpeed = 1.5f;
    public float leftX;
    public float rightX;
    public bool startFacingRight = true;

    Rigidbody2D rb;
    Animator animator;
    SpriteRenderer spriteRenderer;

    float direction = 1f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        direction = startFacingRight ? 1f : -1f;
        ApplyFacing();
        IgnorePlayerBodyCollision();
    }

    void IgnorePlayerBodyCollision()
    {
        // 只忽略「玩家身体」和「怪身体」，AttackHitbox 仍可 Trigger 打到怪
        PlayerController player = FindObjectOfType<PlayerController>();
        if (player == null)
            return;

        Collider2D playerBody = player.GetComponent<Collider2D>();
        Collider2D enemyBody = GetComponent<Collider2D>();
        if (playerBody != null && enemyBody != null)
            Physics2D.IgnoreCollision(playerBody, enemyBody);
    }

    void Update()
    {
        float x = rb.position.x;
        if (direction > 0f && x >= rightX)
            direction = -1f;
        else if (direction < 0f && x <= leftX)
            direction = 1f;

        ApplyFacing();
        rb.velocity = new Vector2(direction * moveSpeed, rb.velocity.y);

        if (animator != null)
            animator.SetBool("IsWalking", true);
    }

    void ApplyFacing()
    {
        if (spriteRenderer == null)
            return;

        // 默认精灵朝右：向左走时翻转
        spriteRenderer.flipX = direction < 0f;
    }
}
