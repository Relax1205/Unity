using UnityEngine;
using UnityEngine.AI; // Для NavMeshAgent

public class Ghost : CharacterBase
{
    [Header("Ghost Settings")]
    public float floatHeight = 0.5f;
    public float floatSpeed = 2f;
    public float patrolRadius = 0.5f;
    public float detectRange = 5f;
    public float attackRange = 1.5f;
    public float fleeSpeedMultiplier = 0.7f;
    public float attackSpeedMultiplier = 2.5f;

    private Vector3 startPos;
    private bool isVacuumed = false;
    private Transform player;
    private NavMeshAgent navAgent; // Ссылка на агент

    void Start()
    {
        // === ДОБАВЛЕНО: Отключаем NavMeshAgent ===
        navAgent = GetComponent<NavMeshAgent>();
        if (navAgent != null)
        {
            navAgent.enabled = false; // Отключаем чтобы не было ошибки
        }
        // ==========================================

        startPos = transform.position;
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
    }

    public override void Move(Vector3 direction)
    {
        base.Move(direction);
        float newY = startPos.y + Mathf.Sin(Time.time * floatSpeed) * floatHeight;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }

    public void GetVacuumed(Transform playerPos)
    {
        isVacuumed = true;
        Vector3 dir = (playerPos.position - transform.position).normalized;
        transform.Translate(dir * (moveSpeed * attackSpeedMultiplier) * Time.deltaTime);
    }

    public override void Die()
    {
        Debug.Log("Призрак пойман!");
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGhostCaught();
        }
        Destroy(gameObject);
    }

    void Update()
    {
        if (isVacuumed || player == null)
        {
            return;
        }

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
        float x = Mathf.Sin(Time.time * floatSpeed) * patrolRadius;
        float z = Mathf.Cos(Time.time * floatSpeed) * patrolRadius;
        Vector3 patrolMove = new Vector3(x, 0, z);
        Move(patrolMove);
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