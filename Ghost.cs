using UnityEngine;
using System.Collections;
using Photon.Pun; 

public class Ghost : CharacterBase
{
    [Header("Ghost Settings")]
    public float floatHeight = 0.5f;
    public float floatSpeed = 1f;
    public float detectRange = 5f;
    public float attackRange = 1.5f;

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
        Debug.Log("Призрак поглощён!");
        isVacuumed = true;
        
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayGhostDieSound();
        }
        
        if (PhotonNetwork.IsConnectedAndReady && GameManager.Instance != null)
        {
            GameManager.Instance.OnGhostCaught();
        }
        else if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGhostCaught();
        }
        
        StartCoroutine(DeathAnimationRoutine());
    }

    IEnumerator DeathAnimationRoutine()
    {
        float duration = 0.5f;
        float timer = 0f;
        Vector3 startScale = transform.localScale;
        Vector3 endScale = Vector3.zero;

        if (GetComponent<Collider>() != null)
            GetComponent<Collider>().enabled = false;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = timer / duration;
            transform.localScale = Vector3.Lerp(startScale, endScale, t);
            yield return null;
        }

        Destroy(gameObject);
    }

    void Update()
    {
        if (isVacuumed || player == null) return;

        float newY = startPos.y + Mathf.Sin(Time.time * floatSpeed) * floatHeight;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);

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