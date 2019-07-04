using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class NetworkingManager : NetworkManager {

    public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId)
    {
        GameObject player;

        player = (GameObject)Object.Instantiate(this.playerPrefab, Vector3.zero, Quaternion.identity);
        NetworkServer.AddPlayerForConnection(conn, player, playerControllerId);

    }

    public override void OnClientSceneChanged(NetworkConnection conn)
    {
        ClientScene.AddPlayer(conn, 0);
    }
}
