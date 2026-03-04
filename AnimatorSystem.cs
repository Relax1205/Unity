using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class AnimationClip
{
    public string name;
    public float duration;
    public AnimationCurve[] curves;
    public bool loop;
}

public class AnimatorSystem : MonoBehaviour
{
    [Header("Animation Settings")]
    public float animationSpeed = 1f;
    
    [Header("Procedural Settings")]
    public bool useProceduralAnimation = true;
    
    [Header("Current State")]
    public AnimationState currentState = AnimationState.Idle;
    
    private List<AnimationState> allStates = new List<AnimationState>();
    private Dictionary<AnimationState, float> stateTimers = new Dictionary<AnimationState, float>();
    
    protected Transform modelTransform;
    protected Vector3 lastPosition;
    
    public virtual void Initialize()
    {
        modelTransform = transform.Find("Model");
        if (modelTransform == null)
        {
            modelTransform = transform;
        }
        lastPosition = transform.position;
        
        allStates.Clear();
        stateTimers.Clear();
        
        foreach (AnimationState state in System.Enum.GetValues(typeof(AnimationState)))
        {
            allStates.Add(state);
            stateTimers[state] = 0f;
        }
    }
    
    public virtual void UpdateAnimation()
    {
        if (!useProceduralAnimation) return;
        
        UpdateStateTimers();
        ApplyProceduralAnimation();
    }
    
    protected virtual void UpdateStateTimers()
    {
        for (int i = 0; i < allStates.Count; i++)
        {
            AnimationState state = allStates[i];
            
            if (state == currentState)
            {
                stateTimers[state] += Time.deltaTime * animationSpeed;
            }
            else
            {
                stateTimers[state] = 0f;
            }
        }
    }
    
    protected virtual void ApplyProceduralAnimation()
    {
        switch (currentState)
        {
            case AnimationState.Float:
                ApplyFloatAnimation();
                break;
            case AnimationState.Death:
                ApplyDeathAnimation();
                break;
            default:
                ApplyIdleAnimation();
                break;
        }
    }
    
    protected virtual void ApplyIdleAnimation()
    {
        // Пустая базовая реализация
    }
    
    protected virtual void ApplyFloatAnimation()
    {
        // Переопределяется в GhostAnimator
    }
    
    protected virtual void ApplyDeathAnimation()
    {
        // Переопределяется в GhostAnimator
    }
    
    public void SetState(AnimationState newState)
    {
        if (currentState != newState)
        {
            currentState = newState;
            
            if (stateTimers.ContainsKey(newState))
            {
                stateTimers[newState] = 0f;
            }
            
            OnStateChange(newState);
        }
    }
    
    protected virtual void OnStateChange(AnimationState to)
    {
        Debug.Log($"🎬 Анимация: {to}");
    }
    
    // ✅ КЛЮЧЕВАЯ АНИМАЦИЯ
    public void PlayKeyframeAnimation(AnimationClip clip)
    {
        if (clip == null) return;
        
        StartCoroutine(PlayKeyframeCoroutine(clip));
    }
    
    private System.Collections.IEnumerator PlayKeyframeCoroutine(AnimationClip clip)
    {
        float timer = 0f;
        
        while (timer < clip.duration)
        {
            timer += Time.deltaTime * animationSpeed;
            
            if (timer >= clip.duration)
            {
                break;
            }
            
            float normalizedTime = timer / clip.duration;
            
            for (int i = 0; i < clip.curves.Length; i++)
            {
                if (clip.curves[i] != null)
                {
                    float value = clip.curves[i].Evaluate(normalizedTime);
                    ApplyCurveValue(i, value);
                }
            }
            
            yield return null;
        }
    }
    
    protected virtual void ApplyCurveValue(int curveIndex, float value)
    {
        // Переопределяется в наследниках
    }
    
    protected float GetStateTimer(AnimationState state)
    {
        if (stateTimers.ContainsKey(state))
        {
            return stateTimers[state];
        }
        return 0f;
    }
}