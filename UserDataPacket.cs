using UnityEngine;
using System;


[Serializable]
public struct UserDataPacket
{
    public Vector3 position;
    public Quaternion rotation;
    public int health;
    public int score;
    
    public bool isVacuumActive;
    public bool isGrounded;
    public int currentJumps;
    
    public float timestamp;
    
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
    
    public void DebugLog(string prefix = "PTS")
    {
        Debug.Log($"[{prefix}] POS:{position} | HP:{health} | Score:{score} | Vacuum:{isVacuumActive}");
    }
}