using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthUI : MonoBehaviour
{
    public PlayerHealth playerHealth;
    public Image[] hearts;
    public Sprite heartFull;
    public Sprite heartEmpty;

    void OnEnable()
    {
        if (playerHealth != null)
            playerHealth.onHealthChanged.AddListener(Refresh);
    }

    void OnDisable()
    {
        if (playerHealth != null)
            playerHealth.onHealthChanged.RemoveListener(Refresh);
    }

    void Start()
    {
        if (playerHealth != null)
            Refresh(playerHealth.CurrentHp, playerHealth.MaxHp);
    }

    void Refresh(int current, int max)
    {
        if (hearts == null)
            return;

        for (int i = 0; i < hearts.Length; i++)
        {
            if (hearts[i] == null)
                continue;

            hearts[i].gameObject.SetActive(i < max);
            if (i < max)
                hearts[i].sprite = i < current ? heartFull : heartEmpty;
        }
    }
}
