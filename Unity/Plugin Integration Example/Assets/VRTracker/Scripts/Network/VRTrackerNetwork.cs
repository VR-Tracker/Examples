using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class VRTrackerNetwork : NetworkManager
{
    public static VRTrackerNetwork instance;
    //public static VRTrackerNetwork singleton;
    private bool connectionAsClientEstablished = false;
    public NetworkClient mclient;
    public NetworkConnection mNetworkConn;
    public bool server;

    private bool serverIpReceived = false;
    private bool isWaitingForIP = false;

    public bool playerSpawned = false;

    public List<GameObject> players;

    void Start()
    {
        if (instance != null)
        {
            Debug.LogError("More than one VRTracker Network in the scene");
        }
        else
        {
            instance = this;
        }
    }

    public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId)
    {
        if (Network.isServer)
        {
            base.OnServerAddPlayer(conn, playerControllerId);

            var newPlayer = conn.playerControllers[0].gameObject;

            players.Add(newPlayer);
        }

    }

    public override void OnServerRemovePlayer(NetworkConnection conn, PlayerController player)
    {
        if (Network.isServer)
        {
            players.Remove(player.gameObject);

            base.OnServerRemovePlayer(conn, player);
        }


    }

    public override void OnServerDisconnect(NetworkConnection conn)
    {
        if (Network.isServer)
        {
            foreach (var p in conn.playerControllers)
            {
                if (p != null && p.gameObject != null)
                {
                    players.Remove(p.gameObject);
                }
            }
            base.OnServerDisconnect(conn);
        }

           
    }
    // called when a client connects
    public override void OnServerConnect(NetworkConnection conn)
    {
        Debug.LogWarning("Receive client " + conn.address);
        Debug.LogWarning("Receive client state " + conn.isReady);

        base.OnServerConnect(conn);
    }

}
