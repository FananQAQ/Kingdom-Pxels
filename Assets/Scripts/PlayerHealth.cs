using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class PlayerHealth : MonoBehaviour
{
    public int maxHp = 3;
    public float hurtInvincibleTime = 0.8f;
    public bool allowTestDamageKey = true;

    public UnityEvent<int, int> onHealthChanged;
    public UnityEvent onDied;

    int hp;
    bool dead;
    bool invincible;

    void Start()
    {
        hp = maxHp;
        NotifyHealthChanged();
    }

    void Update()
    {
        // 临时：按 H 扣 1 心，方便还没接怪攻击时测试 UI
        if (allowTestDamageKey && Input.GetKeyDown(KeyCode.H))
            TakeDamage(1);
    }

    public void TakeDamage(int amount)
    {
        if (dead || invincible || amount <= 0)
            return;

        hp = Mathf.Max(0, hp - amount);
        NotifyHealthChanged();

        if (hp <= 0)
        {
            Die();
            return;
        }

        StartCoroutine(InvincibleRoutine());
    }

    public void Heal(int amount)
    {
        if (dead || amount <= 0)
            return;

        hp = Mathf.Min(maxHp, hp + amount);
        NotifyHealthChanged();
    }

    IEnumerator InvincibleRoutine()
    {
        invincible = true;
        yield return new WaitForSeconds(hurtInvincibleTime);
        invincible = false;
    }

    void Die()
    {
        dead = true;
        invincible = true;
        onDied?.Invoke();
    }

    void NotifyHealthChanged()
    {
        onHealthChanged?.Invoke(hp, maxHp);
    }

    public int CurrentHp => hp;
    public int MaxHp => maxHp;
    public bool IsDead => dead;
}
