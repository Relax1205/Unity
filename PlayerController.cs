using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : CharacterBase
{
    [Header("Camera Settings")]
    public Transform cameraTransform;
    public float mouseSensitivity = 2f;
    
    [Header("Physics")]
    private Rigidbody rb;
    private float xRotation = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        
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
    }

    public override void Move(Vector3 direction)
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        Vector3 move = new Vector3(h, 0, v);
        
        if (rb != null)
        {
            rb.MovePosition(transform.position + move * moveSpeed * Time.deltaTime);
        }
        else
        {
            base.Move(move);
        }
    }

    void Update()
    {
        HandleCameraRotation();
        Move(Vector3.zero);
        
        if (Input.GetKeyDown(KeyCode.Space))
        {
            UseVacuum();
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

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, 5f);
    }
    
    public override void Die()
    {
        Debug.Log("Игрок погиб!");
    }
}