using UnityEngine;
using UnityEngine.AI;

public class Ghost : CharacterBase
{
    [Header("Ghost Settings")]
    public float floatHeight = 0.3f;
    public float floatSpeed = 0.8f;
    public float patrolRadius = 0.3f;
    public float detectRange = 5f;
    public float attackRange = 1.5f;
    public float fleeSpeedMultiplier = 0.7f;
    public float attackSpeedMultiplier = 2.5f;
    
    [Header("VFX Settings")]
    public ParticleSystem deathParticlePrefab;
    
    [Header("Animation")]
    public GhostAnimator ghostAnimator;
    
    private Vector3 startPos;
    private bool isVacuumed = false;
    private Transform player;
    private NavMeshAgent navAgent;
    private float patrolTimer = 0f;
    private Vector3 patrolTarget;
    
    void Start()
    {
        navAgent = GetComponent<NavMeshAgent>();
        if (navAgent != null)
        {
            navAgent.enabled = false;
        }
        
        startPos = transform.position;
        
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
        
        // 🎬 Инициализация аниматора
        if (ghostAnimator == null)
        {
            ghostAnimator = gameObject.AddComponent<GhostAnimator>();
        }
        
        ghostAnimator.Initialize();
        ghostAnimator.SetFloatParams(floatHeight, floatSpeed);
        ghostAnimator.SetState(AnimationState.Float);
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
        
        // 🎬 Обновление процедурной анимации (только парение)
        if (!isVacuumed && ghostAnimator != null && ghostAnimator.currentState != AnimationState.Death)
        {
            ghostAnimator.UpdateAnimation();
        }
    }
    
    public void GetVacuumed(Transform playerPos)
    {
        isVacuumed = true;
        Vector3 dir = (playerPos.position - transform.position).normalized;
        transform.Translate(dir * (moveSpeed * attackSpeedMultiplier) * Time.deltaTime);
    }
    
    public override void Die()
    {
        Debug.Log("👻 Призрак пойман!");
        
        // 🎬 КЛЮЧЕВАЯ АНИМАЦИЯ: Смерть
        if (ghostAnimator != null)
        {
            ghostAnimator.SetState(AnimationState.Death);
        }
        
        // 🎆 VFX
        if (deathParticlePrefab != null)
        {
            Instantiate(deathParticlePrefab, transform.position, Quaternion.identity);
        }
        
        // 🔊 Звук
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayGhostDieSound();
        }
        
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGhostCaught();
        }
        
        Destroy(gameObject, 1.5f);
    }
    
    void Update()
    {
        if (isVacuumed || player == null)
        {
            return;
        }
        
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        
        // 🎬 Только 2 состояния: Float или Death
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
        patrolTimer += Time.deltaTime;
        
        if (patrolTimer >= 3f || Vector3.Distance(transform.position, patrolTarget) < 0.5f)
        {
            patrolTimer = 0f;
            patrolTarget = startPos + new Vector3(
                Random.Range(-patrolRadius, patrolRadius),
                0,
                Random.Range(-patrolRadius, patrolRadius)
            );
        }
        
        Vector3 direction = (patrolTarget - transform.position).normalized;
        Move(direction);
        
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
        }
    }
    
    void FleeFromPlayer()
    {
        Vector3 direction = (transform.position - player.position).normalized;
        Move(direction * fleeSpeedMultiplier);
        
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
        }
    }
    
    void AttackPlayer()
    {
        Vector3 direction = (player.position - transform.position).normalized;
        Move(direction * attackSpeedMultiplier);
        
        PlayerController playerCtrl = player.GetComponent<PlayerController>();
        if (playerCtrl != null && Time.time % 1f < 0.05f)
        {
            playerCtrl.TakeDamage(10);
        }
        
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
        }
    }
}