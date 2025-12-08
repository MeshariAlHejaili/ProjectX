using UnityEngine;

// This acts as a contract. Any script that uses this MUST have a TakeDamage function.
public interface IDamageable
{
    void TakeDamage(int amount);
}