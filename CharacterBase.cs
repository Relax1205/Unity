using UnityEngine;

public abstract class CharacterBase : MonoBehaviour
{
    [Header("Base Stats")]
    public float moveSpeed = 5f;
    public int health = 100;
    public int maxHealth = 100;
    protected Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public virtual void Move(Vector3 direction)
    {
        if (rb != null && !rb.isKinematic)
        {
            rb.MovePosition(transform.position + direction * moveSpeed * Time.deltaTime);
        }
        else
        {
            transform.Translate(direction * moveSpeed * Time.deltaTime);
        }
    }

    public virtual void TakeDamage(int damage)
    {
        // === ЗАЩИТА ОТ ОТРИЦАТЕЛЬНОГО ЗДОРОВЬЯ ===
        health -= damage;
        
        // Не даем здоровью упасть ниже 0
        if (health < 0)
        {
            health = 0;
        }
        
        Debug.Log($"{gameObject.name} получил урон: {damage}. Осталось здоровья: {health}");
        
        // === СМЕРТЬ ПРИ 0 ЗДОРОВЬЯ ===
        if (health <= 0)
        {
            Die();
        }
    }

    public abstract void Die();
}