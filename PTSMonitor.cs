using UnityEngine;
using Photon.Pun;

public class PTSMonitor : MonoBehaviour
{
    [Header("PTS Statistics")]
    public int packetsSent = 0;
    public int packetsReceived = 0;
    public float lastPacketTime = 0f;
    
    [Header("UI")]
    public GameObject debugPanel;
    public UnityEngine.UI.Text debugText;
    
    void Start()
    {
        InvokeRepeating("UpdatePTSStats", 1f, 1f);
    }
    
    void UpdatePTSStats()
    {
        if (PhotonNetwork.IsConnectedAndReady)
        {
            Debug.Log($"PTS Monitor: Sent={packetsSent} | Received={packetsReceived} | Ping={PhotonNetwork.GetPing()}ms");
        }
    }
    
    public void RecordPacketSent()
    {
        packetsSent++;
        lastPacketTime = Time.time;
    }
    
    public void RecordPacketReceived()
    {
        packetsReceived++;
        lastPacketTime = Time.time;
    }
}