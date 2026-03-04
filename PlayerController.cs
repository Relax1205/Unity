using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : CharacterBase
{
    [Header("Camera Settings")]
    public Transform cameraTransform;
    public float mouseSensitivity = 2f;

    [Header("Physics")]
    private float xRotation = 0f;
    private bool isGameActive = true;  // ✅ ДОБАВЛЕНО: Поле было удалено

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
    public int maxJumps = 2; // ✅ НОВОЕ: Максимальное количество прыжков
    private int currentJumps = 0; // ✅ НОВОЕ: Текущее количество прыжков
    private bool isGrounded = false;
    private LayerMask groundLayer;

    [Header("VFX Settings")]
    public ParticleSystem hitParticlePrefab;
    public ParticleSystem jumpParticlePrefab;
    public GameObject vacuumParticlePrefab;
    private ParticleSystem activeVacuumVFX;

    // ❌ УДАЛЕНО: Всё связанное с AnimatorSystem

    void Start()
    {
        rb.freezeRotation = true;
        moveSpeed = 8f;
        if (cameraTransform == null)
        {
            cameraTransform = Camera.main.transform;
        }
        cameraTransform.SetParent(transform);
        cameraTransform.localPosition = new Vector3(0, 1f, 0);
        transform.rotation = Quaternion.identity;
        cameraTransform.localRotation = Quaternion.identity;
        xRotation = 0f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        groundLayer = LayerMask.GetMask("Ground", "Default");
        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateHealth(health, 100);
        }
    }

    public override void Move(Vector3 direction)
    {
        if (!isGameActive) return;  // ✅ Теперь работает
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
        if (!isGameActive) return;  // ✅ Теперь работает

        HandleCameraRotation();
        Move(Vector3.zero);
        CheckGround();

        // ✅ ИЗМЕНЕНО: Проверка на двойной прыжок
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

        // ❌ УДАЛЕНО: Обновление анимации
    }

    // ✅ ИЗМЕНЕНО: Отслеживание приземления для сброса прыжков
    void CheckGround()
    {
        bool wasGrounded = isGrounded; // Запоминаем состояние с прошлого кадра
        isGrounded = Physics.Raycast(transform.position, Vector3.down, groundCheckDistance, groundLayer);

        // Если только что приземлились, сбрасываем счетчик прыжков
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

    public override void TakeDamage(int damage)
    {
        base.TakeDamage(damage);
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

    public override void Die()
    {
        Debug.Log("💀 Игрок погиб!");
        GameOver();
    }

    void GameOver()
    {
        isGameActive = false;  // ✅ Теперь работает
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