using UnityEngine;
using System;

/// <summary>
/// Структура пользовательского пакета данных (PTS)
/// Используется для синхронизации между клиентами
/// </summary>
[Serializable]
public struct UserDataPacket
{
    // ✅ Основные данные игрока
    public Vector3 position;
    public Quaternion rotation;
    public int health;
    public int score;
    
    // ✅ Состояния
    public bool isVacuumActive;
    public bool isGrounded;
    public int currentJumps;
    
    // ✅ Временная метка (для интерполяции)
    public float timestamp;
    
    // ✅ Конструктор
    public UserDataPacket(Vector3 pos, Quaternion rot, int hp, int sc, 
                         bool vacuum, bool grounded, int jumps)
    {
        position = pos;
        rotation = rot;
        health = hp;
        score = sc;
        isVacuumActive = vacuum;
        isGrounded = grounded;
        currentJumps = jumps;
        timestamp = Time.time;
    }
    
    // ✅ Метод для логирования (отладка)
    public void DebugLog(string prefix = "PTS")
    {
        Debug.Log($"[{prefix}] POS:{position} | HP:{health} | Score:{score} | Vacuum:{isVacuumActive}");
    }
}