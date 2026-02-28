using UnityEngine;
public class PlayerController : CharacterBase
{
    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;
        mainCamera.transform.position = transform.position + new Vector3(0, 5, -5);
        mainCamera.transform.LookAt(transform.position);
    }

    public override void Move(Vector3 direction)
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        Vector3 move = new Vector3(h, 0, v);
        base.Move(move);
    }

    public override void Die()
    {
        Debug.Log("Игрок погиб!");
    }

    void Update()
    {
        Move(Vector3.zero);

        if (Input.GetKeyDown(KeyCode.Space))
        {
            UseVacuum();
        }
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
}