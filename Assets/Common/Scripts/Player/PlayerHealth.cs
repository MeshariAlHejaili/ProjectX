using UnityEngine;
using UnityEngine.SceneManagement; 
using UnityEngine.UI; // <-- This is essential!

public class PlayerHealth : MonoBehaviour, IDamageable
{
    [SerializeField] private int maxHealth = 100;
    private int currentHealth;

    [SerializeField] private Slider healthSlider; // <-- Reference to your Slider

    private void Start()
    {
        currentHealth = maxHealth;
        
        if (healthSlider != null)
        {
            healthSlider.minValue = 0;
            healthSlider.maxValue = maxHealth;
            UpdateHealthBar(); // Set the initial value
        }
    }

    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        currentHealth = Mathf.Max(currentHealth, 0); 
        
        Debug.Log($"Player HP: {currentHealth}");

        UpdateHealthBar(); // <-- Updates the UI every time damage is taken

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    // Helper function to update the Slider UI
    private void UpdateHealthBar()
    {
        if (healthSlider != null)
        {
            healthSlider.value = currentHealth;
        }
    }

    private void Die()
    {
        Debug.Log("Player Died! Restarting...");
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}