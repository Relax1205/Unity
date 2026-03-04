using UnityEngine;

public class Ghost : CharacterBase
{
    [Header("Ghost Settings")]
    public float floatHeight = 0.3f;
    public float floatSpeed = 0.8f;
    public float detectRange = 5f;
    public float attackRange = 1.5f;
    
    [Header("VFX")]
    public ParticleSystem deathParticlePrefab;
    
    private Vector3 startPos;
    private bool isVacuumed = false;
    private Transform player;
    
    void Start()
    {
        startPos = transform.position;
        
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
    }
    
    public override void Move(Vector3 direction)
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
    
    public void GetVacuumed(Transform playerPos)
    {
        isVacuumed = true;
        Vector3 dir = (playerPos.position - transform.position).normalized;
        transform.Translate(dir * moveSpeed * Time.deltaTime);
    }
    
    public override void Die()
    {
        Debug.Log("👻 Призрак поглощён!");
        
        isVacuumed = true;
        
        // Частицы
        if (deathParticlePrefab != null)
        {
            Instantiate(deathParticlePrefab, transform.position, Quaternion.identity);
        }
        
        // Звук
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayGhostDieSound();
        }
        
        // Счёт
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGhostCaught();
        }
        
        // Удаляем объект
        Destroy(gameObject, 1.5f);
    }
    
    void Update()
    {
        if (isVacuumed || player == null) return;
        
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        
        if (distanceToPlayer <= attackRange)
        {
            AttackPlayer();
        }
        else if (distanceToPlayer <= detectRange)
        {
            FleeFromPlayer();
        }
        else
        {
            Patrol();
        }
    }
    
    void Patrol()
    {
        float x = Mathf.Sin(Time.time * floatSpeed) * 0.3f;
        float z = Mathf.Cos(Time.time * floatSpeed) * 0.3f;
        Move(new Vector3(x, 0, z));
    }
    
    void FleeFromPlayer()
    {
        Vector3 direction = (transform.position - player.position).normalized;
        Move(direction * 0.7f);
    }
    
    void AttackPlayer()
    {
        Vector3 direction = (player.position - transform.position).normalized;
        Move(direction * 2.5f);
        
        PlayerController playerCtrl = player.GetComponent<PlayerController>();
        if (playerCtrl != null && Time.time % 1f < 0.05f)
        {
            playerCtrl.TakeDamage(10);
        }
    }
}