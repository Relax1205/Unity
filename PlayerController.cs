using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : CharacterBase
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

        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateHealth(health, 100);
        }
    }

    public override void Move(Vector3 direction)
    {
        if (!isGameActive) return;

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
        if (!isGameActive) return;

        HandleCameraRotation();
        Move(Vector3.zero);

        if (Input.GetKeyDown(KeyCode.Space))
        {
            UseVacuum();
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
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

    void UseVacuum()
    {
        Debug.Log("Пылесос включен.");
        Collider[] hits = Physics.OverlapSphere(transform.position, 5f);
        foreach (Collider hit in hits)
        {
            Ghost ghost = hit.GetComponent<Ghost>();
            if (ghost != null)
            {
                ghost.GetVacuumed(transform);
                Invoke("DestroyGhost", 0.5f);
            }
        }
    }

    void DestroyGhost()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, 5f);
        foreach (Collider hit in hits)
        {
            Ghost ghost = hit.GetComponent<Ghost>();
            if (ghost != null)
            {
                ghost.TakeDamage(100);
            }
        }
    }

    public override void TakeDamage(int damage)
    {
        base.TakeDamage(damage);

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
        Debug.Log("Игрок погиб!");
        GameOver();
    }

    void GameOver()
    {
        isGameActive = false;

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
    }
}