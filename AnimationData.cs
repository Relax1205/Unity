using UnityEngine;
using System;

// ✅ Базовые данные анимации (общие)
[CreateAssetMenu(fileName = "NewAnimationData", menuName = "Animation/Animation Data")]
public class AnimationData : ScriptableObject
{
    [Header("Animation Clips")]
    public AnimationClip[] clips;
    
    [Header("State Mappings")]
    public AnimationStateClip[] stateClips;
}

// ✅ Маппинг состояния на клип
[Serializable]
public class AnimationStateClip
{
    public AnimationState state;
    public AnimationClip clip;
}

// ❌ УДАЛЕНО: PlayerAnimationData (игрок без анимации)

// ✅ НОВОЕ: Ghost Animation Data (только для призрака)
[CreateAssetMenu(fileName = "NewGhostAnimation", menuName = "Animation/Ghost Animation")]
public class GhostAnimationData : ScriptableObject
{
    [Header("Procedural Animation Settings")]
    public float floatHeight = 0.3f;
    public float floatSpeed = 0.8f;
    public float patrolRadius = 0.3f;
    
    [Header("Keyframe Animation Clips")]
    public AnimationClip deathClip;      // ✅ КЛЮЧЕВАЯ: Смерть
    public AnimationClip hitClip;        // ✅ КЛЮЧЕВАЯ: Удар
    
    [Header("Optional Clips")]
    public AnimationClip vacuumClip;     // Засасывание пылесосом
    
    public AnimationClip GetClip(AnimationState state)
    {
        switch (state)
        {
            case AnimationState.Death: return deathClip;
            case AnimationState.Hit: return hitClip;
            case AnimationState.Vacuum: return vacuumClip;
            default: return null;
        }
    }
}