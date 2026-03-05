using UnityEngine;
using Photon.Pun;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : CharacterBase, IPunObservable
{
    [Header("Camera Settings")]
    public Transform cameraTransform;
    public float mouseSensitivity = 2f;

    [Header("Physics")]
    private float xRotation = 0f;
    private bool isGameActive = true;

    [Header("Movement Settings")]
    public float sprintMultiplier = 2f;
    private bool isSprinting = false;

    [Header("Vacuum")]
    private bool isVacuumActive = false;
    private bool destroyScheduled = false;

    [Header("Jump Settings")]
    public float jumpForce = 5f;
    public float sprintJumpForce = 8f;
    public float groundCheckDistance = 0.2f;
    public int maxJumps = 2;
    private int currentJumps = 0;
    private bool isGrounded = false;
    private LayerMask groundLayer;

    [Header("VFX Settings")]
    public ParticleSystem hitParticlePrefab;
    public ParticleSystem jumpParticlePrefab;
    public GameObject vacuumParticlePrefab;
    private ParticleSystem activeVacuumVFX;

    [Header("Animator System")]
    public Animator animator;

    // ✅ СЕТЕВЫЕ КОМПОНЕНТЫ PHOTON PUN
    private PhotonView photonView;
    private bool isLocalPlayer = false;
    
    // ✅ PTS: Пользовательский пакет данных
    private UserDataPacket currentPTS;
    private UserDataPacket lastReceivedPTS;
    private float lastPTSUpdateTime = 0f;
    
    // ✅ Счёт игрока (для PTS)
    private int playerScore = 0;

    void Start()
    {
        
        rb.freezeRotation = true;
        moveSpeed = 8f;

        // ✅ ПОЛУЧАЕМ PhotonView компонент
        photonView = GetComponent<PhotonView>();
        
        // ✅ ПРОВЕРЯЕМ: является ли этот игрок локальным
        isLocalPlayer = photonView.IsMine;

        // ✅ ИНИЦИАЛИЗАЦИЯ ANIMATOR
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }

        if (cameraTransform == null)
        {
            cameraTransform = Camera.main.transform;
        }

        // ✅ КАМЕРУ прикрепляем ТОЛЬКО к локальному игроку
        if (isLocalPlayer)
        {
            cameraTransform.SetParent(transform);
            cameraTransform.localPosition = new Vector3(0, 1f, 0);
            transform.rotation = Quaternion.identity;
            cameraTransform.localRotation = Quaternion.identity;
            xRotation = 0f;
            
        }
        else
        {
            // ✅ Для удалённых игроков: отключаем камеру и освобождаем курсор
            if (Camera.main != null)
            {
                Camera.main.enabled = false;
            }
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            
            // ✅ Удалённые игроки не управляют физикой напрямую
            if (rb != null)
            {
                rb.isKinematic = true;
            }
        }

        groundLayer = LayerMask.GetMask("Ground", "Default");

        // ✅ UI обновляем только для локального игрока
        if (UIManager.Instance != null && isLocalPlayer)
        {
            UIManager.Instance.UpdateHealth(health, 100);
        }
        
        // ✅ ИНИЦИАЛИЗАЦИЯ PTS ПАКЕТА
        InitializePTS();
    }

    // ✅ PTS: Инициализация пакета
    void InitializePTS()
    {
        currentPTS = new UserDataPacket(
            transform.position,
            transform.rotation,
            health,
            playerScore,
            isVacuumActive,
            isGrounded,
            currentJumps
        );
        Debug.Log("📦 PTS: Пакет инициализирован");
    }

    // ✅ PTS: Обновление данных пакета
    void UpdatePTS()
    {
        currentPTS = new UserDataPacket(
            transform.position,
            transform.rotation,
            health,
            playerScore,
            isVacuumActive,
            isGrounded,
            currentJumps
        );
    }

    // ✅ СИНХРОНИЗАЦИЯ ДАННЫХ МЕЖДУ КЛИЕНТАМИ (IPunObservable)
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            // 📤 ОТПРАВКА данных (только для локального игрока)
            // ✅ PTS: Формируем пакет для отправки
            UpdatePTS();
            
            stream.SendNext(currentPTS.position);
            stream.SendNext(currentPTS.rotation);
            stream.SendNext(currentPTS.health);
            stream.SendNext(currentPTS.score);
            stream.SendNext(currentPTS.isVacuumActive);
            stream.SendNext(currentPTS.isGrounded);
            stream.SendNext(currentPTS.currentJumps);
            stream.SendNext(currentPTS.timestamp);
            
            // ✅ Логирование отправки (для отладки)
            if (Time.frameCount % 60 == 0) // Раз в секунду
            {
                Debug.Log("📤 PTS: Отправка пакета " + currentPTS.position);
            }
        }
        else
        {
            // 📥 ПОЛУЧЕНИЕ данных (только для удалённых игроков)
            // ✅ PTS: Получаем и обрабатываем пакет
            Vector3 pos = (Vector3)stream.ReceiveNext();
            Quaternion rot = (Quaternion)stream.ReceiveNext();
            int hp = (int)stream.ReceiveNext();
            int sc = (int)stream.ReceiveNext();
            bool vacuum = (bool)stream.ReceiveNext();
            bool grounded = (bool)stream.ReceiveNext();
            int jumps = (int)stream.ReceiveNext();
            float ts = (float)stream.ReceiveNext();
            
            // ✅ Создаём полученный PTS пакет
            lastReceivedPTS = new UserDataPacket(pos, rot, hp, sc, vacuum, grounded, jumps);
            lastReceivedPTS.timestamp = ts;
            lastPTSUpdateTime = Time.time;
            
            // ✅ Интерполяция для плавности (из лекции стр. 58-59)
            if (!isLocalPlayer)
            {
                // Плавное перемещение к полученной позиции
                transform.position = Vector3.Lerp(transform.position, pos, Time.deltaTime * 10);
                transform.rotation = Quaternion.Lerp(transform.rotation, rot, Time.deltaTime * 10);
                health = hp;
                playerScore = sc;
                isVacuumActive = vacuum;
                isGrounded = grounded;
                currentJumps = jumps;
                
                // ✅ Логирование получения (для отладки)
                if (Time.frameCount % 60 == 0)
                {
                    Debug.Log("📥 PTS: Получен пакет от " + info.Sender + " | HP:" + hp);
                }
            }
        }
    }

    // ✅ ДВИЖЕНИЕ ТОЛЬКО ДЛЯ ЛОКАЛЬНОГО ИГРОКА
    public override void Move(Vector3 direction)
    {
        if (!isGameActive || !isLocalPlayer) return;

        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        isSprinting = Input.GetKey(KeyCode.LeftShift);

        Vector3 forward = cameraTransform.forward;
        Vector3 right = cameraTransform.right;
        forward.y = 0f;
        right.y = 0f;
        forward.Normalize();
        right.Normalize();

        Vector3 moveDirection = (forward * v + right * h).normalized;
        float currentSpeed = isSprinting ? moveSpeed * sprintMultiplier : moveSpeed;

        // ✅ АНИМАЦИЯ ANIMATOR
        if (animator != null)
        {
            animator.SetFloat("Speed", moveDirection.magnitude * currentSpeed);
        }

        if (rb != null)
        {
            rb.MovePosition(transform.position + moveDirection * currentSpeed * Time.deltaTime);
        }
        else
        {
            transform.Translate(moveDirection * currentSpeed * Time.deltaTime);
        }
    }

    void Update()
    {
        if (!isGameActive || !isLocalPlayer) return;

        HandleCameraRotation();
        Move(Vector3.zero);
        CheckGround();

        if (Input.GetKeyDown(KeyCode.Space) && currentJumps < maxJumps)
        {
            Jump();
            currentJumps++;
        }

        if (Input.GetKey(KeyCode.Mouse0))
        {
            if (!isVacuumActive)
            {
                isVacuumActive = true;
                destroyScheduled = false;
                StartVacuum();
            }
            UseVacuum();
        }
        else if (isVacuumActive)
        {
            isVacuumActive = false;
            StopVacuum();
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }

    void CheckGround()
    {
        bool wasGrounded = isGrounded;
        isGrounded = Physics.Raycast(transform.position, Vector3.down, groundCheckDistance, groundLayer);
        if (isGrounded && !wasGrounded)
        {
            currentJumps = 0;
        }
    }

    void Jump()
    {
        float force = isSprinting ? sprintJumpForce : jumpForce;
        rb.AddForce(Vector3.up * force, ForceMode.Impulse);
        if (jumpParticlePrefab != null)
        {
            Instantiate(jumpParticlePrefab, transform.position, Quaternion.identity);
        }
    }

    void HandleCameraRotation()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        transform.Rotate(Vector3.up * mouseX);
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        cameraTransform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
    }

    void StartVacuum()
    {
        if (animator != null)
        {
            animator.SetBool("IsVacuuming", true);
        }
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StartVacuumSound();
        }
        if (vacuumParticlePrefab != null)
        {
            GameObject vfxObj = Instantiate(vacuumParticlePrefab, transform.position, transform.rotation);
            vfxObj.transform.SetParent(transform);
            activeVacuumVFX = vfxObj.GetComponent<ParticleSystem>();
        }
        if (!destroyScheduled)
        {
            destroyScheduled = true;
            Invoke("DestroyGhost", 0.5f);
        }
    }

    void StopVacuum()
    {
        if (animator != null)
        {
            animator.SetBool("IsVacuuming", false);
        }
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StopVacuumSound();
        }
        if (activeVacuumVFX != null)
        {
            activeVacuumVFX.Stop();
            Destroy(activeVacuumVFX.gameObject, 2f);
            activeVacuumVFX = null;
        }
        CancelInvoke("DestroyGhost");
        destroyScheduled = false;
    }

    void UseVacuum()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, 5f);
        foreach (Collider hit in hits)
        {
            Ghost ghost = hit.GetComponent<Ghost>();
            if (ghost != null)
            {
                ghost.GetVacuumed(transform);
            }
        }
    }

    void DestroyGhost()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayGhostDieSound();
        }
        Collider[] hits = Physics.OverlapSphere(transform.position, 5f);
        foreach (Collider hit in hits)
        {
            Ghost ghost = hit.GetComponent<Ghost>();
            if (ghost != null)
            {
                ghost.TakeDamage(100);
            }
        }
        destroyScheduled = false;
    }

    // ✅ PTS: СЕТЕВОЙ ВЫЗОВ ДЛЯ ПОЛУЧЕНИЯ УРОНА (RPC)
    [PunRPC]
    public void TakeDamageRPC(int damage, float timestamp)
    {
        Debug.Log($"📦 PTS RPC: Получен урон {damage} в {timestamp}");
        TakeDamage(damage);
    }

    // ✅ PTS: TAKEDEAMAGE С СЕТЕВЫМ ВЫЗОВОМ
    public override void TakeDamage(int damage)
    {
        if (photonView.IsMine)
        {
            // ✅ Локальный игрок получает урон напрямую
            base.TakeDamage(damage);
            
            // ✅ ОТПРАВКА PTS ПАКЕТА ОБ УРОНЕ
            SendDamagePTS(damage);

            if (hitParticlePrefab != null)
            {
                Instantiate(hitParticlePrefab, transform.position, Quaternion.identity);
            }
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayHitSound();
            }
            if (UIManager.Instance != null)
            {
                UIManager.Instance.UpdateHealth(health, 100);
            }
            if (health <= 0)
            {
                GameOver();
            }
        }
        else
        {
            // ✅ Удалённый игрок - вызываем RPC для всех клиентов
            photonView.RPC("TakeDamageRPC", RpcTarget.All, damage, Time.time);
        }
    }
    
    // ✅ PTS: Отправка пакета урона
    void SendDamagePTS(int damage)
    {
        UserDataPacket damagePTS = new UserDataPacket(
            transform.position,
            transform.rotation,
            health,
            playerScore,
            isVacuumActive,
            isGrounded,
            currentJumps
        );
        damagePTS.DebugLog("📤 PTS DAMAGE");
    }

    // ✅ PTS: Обновление счёта через RPC
    [PunRPC]
    public void UpdateScoreRPC(int newScore)
    {
        Debug.Log($"📦 PTS RPC: Счёт обновлён {playerScore} -> {newScore}");
        playerScore = newScore;
        if (UIManager.Instance != null && isLocalPlayer)
        {
            UIManager.Instance.UpdateScore(playerScore);
        }
    }
    
    // ✅ PTS: Публичный метод для обновления счёта
    public void UpdateScore(int newScore)
    {
        playerScore = newScore;
        if (photonView.IsMine)
        {
            // ✅ Отправляем обновление счёта всем клиентам
            photonView.RPC("UpdateScoreRPC", RpcTarget.All, newScore);
        }
    }

    public override void Die()
    {
        Debug.Log("💀 Игрок погиб!");
        GameOver();
    }

    void GameOver()
    {
        isGameActive = false;
        if (isVacuumActive)
        {
            StopVacuum();
            isVacuumActive = false;
        }
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowGameOver();
        }
    }

    void TogglePause()
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.TogglePause();
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, 5f);
        Gizmos.color = isGrounded ? Color.green : Color.red;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.down * groundCheckDistance);
    }
}