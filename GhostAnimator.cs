using UnityEngine;

public class GhostAnimator : AnimatorSystem
{
    [Header("Ghost Procedural Animation")]
    public float floatHeight = 0.3f;
    public float floatSpeed = 0.8f;
    
    [Header("Ghost Keyframe Animation")]
    public AnimationClip deathClip;
    
    private Vector3 startPos;
    private float deathTimer = 0f;
    private bool isDying = false;
    
    public override void Initialize()
    {
        base.Initialize();
        startPos = transform.position;
    }
    
    // ✅ ПРОЦЕДУРНАЯ АНИМАЦИЯ: Парение
    protected override void ApplyFloatAnimation()
    {
        if (modelTransform == null) return;
        
        float newY = startPos.y + Mathf.Sin(Time.time * floatSpeed) * floatHeight;
        transform.position = new Vector3(
            transform.position.x,
            newY,
            transform.position.z
        );
        
        float tilt = Mathf.Sin(Time.time * floatSpeed * 0.5f) * 3f;
        modelTransform.localRotation = Quaternion.Euler(tilt, 0, tilt);
    }
    
    // ✅ КЛЮЧЕВАЯ АНИМАЦИЯ: Смерть
    protected override void ApplyDeathAnimation()
    {
        if (modelTransform == null) return;
        
        if (!isDying)
        {
            isDying = true;
            deathTimer = 0f;
            
            // Запуск ключевой анимации если есть клип
            if (deathClip != null)
            {
                PlayKeyframeAnimation(deathClip);
            }
        }
        
        deathTimer += Time.deltaTime;
        float progress = Mathf.Clamp01(deathTimer / 1.5f);
        
        modelTransform.localScale = Vector3.one * (1f - progress);
        transform.Rotate(Vector3.up, 90f * Time.deltaTime);
        
        float riseY = Mathf.Sin(progress * Mathf.PI) * 0.3f;
        transform.position = new Vector3(
            transform.position.x,
            startPos.y + riseY,
            transform.position.z
        );
    }
    
    protected override void OnStateChange(AnimationState to)
    {
        base.OnStateChange(to);
        
        if (to != AnimationState.Death)
        {
            isDying = false;
            deathTimer = 0f;
        }
    }
    
    public void SetFloatParams(float height, float speed)
    {
        floatHeight = height;
        floatSpeed = speed;
    }
}