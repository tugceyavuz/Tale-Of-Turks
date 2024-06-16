using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class EnemyHealth : MonoBehaviour
{
    public int maxHealth = 100;
    public int currentHealth;
    public Animator animator; // Reference to the Animator component
    private bool isDead = false; // Flag to indicate if the enemy is dead
    private Image healthBar;
    void Start()
    {
        currentHealth = maxHealth;
        if (animator == null)
        {
            animator = GetComponent<Animator>(); // Get the Animator component if not set
        }
        if (gameObject.name == "FinalBoss(Clone)" || gameObject.name == "Skelaton(Clone)")
        {
            healthBar = GameObject.Find("BossHealthBar").GetComponent<Image>();
            if (healthBar == null)
            {
                Debug.LogError("HealthBar not found!");
            }
        }
    }

    void Update()
    {
        if (healthBar != null)
        {
            healthBar.fillAmount = Mathf.Clamp((float)currentHealth / maxHealth, 0, 1);
        }
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return; // Do nothing if the enemy is dead

        animator.SetTrigger("Hit");

        currentHealth -= damage;
        Debug.Log("Enemy took damage. Current health: " + currentHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        isDead = true; // Set the flag to indicate the enemy is dead

        // Play death animation
        if (animator != null)
        {
            animator.SetTrigger("Die"); // Assumes "Die" is the trigger for the death animation
        }

        // Start coroutine to destroy object after delay
        StartCoroutine(DestroyAfterDelay(3f));
    }

    IEnumerator DestroyAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        Destroy(gameObject);
    }
}

