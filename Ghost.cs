using UnityEngine;

public class Ghost : CharacterBase
{
    public float floatHeight = 0.5f;
    public float floatSpeed = 2f;
    
    private Vector3 startPos;
    private bool isVacuumed = false;

    void Start()
    {
        startPos = transform.position;
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
        transform.Translate(dir * (moveSpeed * 2) * Time.deltaTime);
    }

    public override void Die()
    {
        Debug.Log("Призрак пойман!");
        Destroy(gameObject);
    }

    void Update()
    {
        if (!isVacuumed)
        {
            Move(new Vector3(Mathf.Sin(Time.time), 0, Mathf.Cos(Time.time)) * 0.5f);
        }
    }
}