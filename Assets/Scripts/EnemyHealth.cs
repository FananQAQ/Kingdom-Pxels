using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    public int maxHp = 3;
    public float hurtLockTime = 0.35f;
    public float deathDestroyDelay = 0.8f;

    int hp;
    bool dead;
    bool hurtLocked;

    Rigidbody2D rb;
    Animator animator;
    EnemyPatrol patrol;
    Collider2D body;

    void Start()
    {
        hp = maxHp;
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        patrol = GetComponent<EnemyPatrol>();
        body = GetComponent<Collider2D>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        TryHit(other);
    }

    // 人和怪重叠后再挥刀时，判定框启用不会再触发 Enter，要用 Stay
    void OnTriggerStay2D(Collider2D other)
    {
        TryHit(other);
    }

    void TryHit(Collider2D other)
    {
        if (dead || hurtLocked)
            return;

        if (other.GetComponent<PlayerAttackHitbox>() == null)
            return;

        TakeDamage(1);
    }

    void TakeDamage(int amount)
    {
        hp -= amount;
        if (hp <= 0)
        {
            Die();
            return;
        }

        StartCoroutine(HurtRoutine());
    }

    IEnumerator HurtRoutine()
    {
        hurtLocked = true;
        if (patrol != null)
            patrol.enabled = false;

        if (rb != null)
            rb.velocity = new Vector2(0f, rb.velocity.y);

        if (animator != null)
        {
            animator.SetBool("IsWalking", false);
            animator.SetTrigger("Hurt");
        }

        yield return new WaitForSeconds(hurtLockTime);

        if (!dead && patrol != null)
            patrol.enabled = true;

        hurtLocked = false;
    }

    void Die()
    {
        dead = true;
        hurtLocked = true;

        if (patrol != null)
            patrol.enabled = false;

        // 定住刚体，不要关碰撞，否则会穿地坠落
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.gravityScale = 0f;
            rb.bodyType = RigidbodyType2D.Kinematic;
        }

        if (body != null)
            body.isTrigger = true;

        if (animator != null)
        {
            animator.SetBool("IsWalking", false);
            animator.SetTrigger("Death");
        }

        Destroy(gameObject, deathDestroyDelay);
    }
}
