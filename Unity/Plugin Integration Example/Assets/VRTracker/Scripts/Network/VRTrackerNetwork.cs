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


    // called when connected to a server
    /*public override void OnClientConnect(NetworkConnection conn)
    {
        Debug.LogWarning(" New Client connected " + conn.address);
        //mNetworkConn = conn;
        //connectionAsClientEstablished = true;
        //WARNING don't call base function, it tells directly that the client is ready
        base.OnClientConnect (conn);

    }*/

    /*public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId)
    {
        Debug.LogWarning(" Add player from server");
        base.OnServerAddPlayer(conn, playerControllerId);
    }*/

    private IEnumerator waitForServerIPTimeout()
    {
        isWaitingForIP = true;
        yield return new WaitForSeconds(3f);
        if (!serverIpReceived)
        {
            Debug.Log("Looking for server timeout : starting Host");
            server = true;
            NetworkManager.singleton.serverBindToIP = true;
            NetworkManager.singleton.serverBindAddress = Network.player.ipAddress;
            client = NetworkManager.singleton.StartHost();
        }
        else
        {
            Debug.Log("Server Ip received");
        }
        isWaitingForIP = false;

    }

    public void receiveServerIP(string ip)
    {
        if (!serverIpReceived)
        {
            Debug.Log("Receiving first IP: " + ip);
            NetworkManager.singleton.networkAddress = ip;
            serverIpReceived = true;
        }
        // The server sent its IP through VR Tracker Gateway, we can start the client connection now
        if (isWaitingForIP && !NetworkManager.singleton.IsClientConnected())
        {
            Debug.Log("IP received : starting Client");
            isWaitingForIP = false;
            if (!NetworkManager.singleton.isNetworkActive)
            {
                Debug.Log("Client going to start after assignation");
                DoOnMainThread.ExecuteOnMainThread.Enqueue(() => NetworkManager.singleton.StartClient());

            }
        }
    }

    public void receiveAskServerIP()
    {
        if (server)
            DoOnMainThread.ExecuteOnMainThread.Enqueue(() => VRTracker.instance.sendServerIP(Network.player.ipAddress));
    }

    void OnApplicationQuit()
    {
        if (server)
        {
            Debug.Log("Cleanup NetworkManager Host");
            NetworkManager.singleton.StopHost();
        }
    }

    public void connectClient()
    {
        client = NetworkManager.singleton.StartClient();
        mclient = client;
        mNetworkConn = client.connection;

    }

    // called when a client connects
    public override void OnServerConnect(NetworkConnection conn)
    {
        Debug.LogWarning("Receive client " + conn.address);
        Debug.LogWarning("Receive client state " + conn.isReady);

        base.OnServerConnect(conn);
    }

}
