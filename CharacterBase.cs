using UnityEngine;


public abstract class CharacterBase : MonoBehaviour
{
    [Header("Base Stats")]
    public float moveSpeed = 5f;
    public int health = 100;

    public virtual void Move(Vector3 direction)
    {
        transform.Translate(direction * moveSpeed * Time.deltaTime);
    }

    public void TakeDamage(int damage)
    {
        health -= damage;
        if (health <= 0)
        {
            Die();
        }
    }

    public abstract void Die();
}