using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

public class ClientReceive : MonoBehaviour {
    #region Packets
    public static void Welcome(Packet _packet) {
        string _message = _packet.ReadString();
        int _myId = _packet.ReadInt();

        Debug.Log($"ClientReceive::Welcome(): Message from server: {_message}");
        ClientConnector.Instance.myId = _myId;

        // Send welcome received packet
        ClientSend.WelcomeReceived();

        ClientConnector.Instance.udp.Connect(((IPEndPoint)ClientConnector.Instance.tcp.socket.Client.LocalEndPoint).Port);
    }

    public static void SpawnPlayer(Packet _packet) {
        int _id = _packet.ReadInt();
        string _username = _packet.ReadString();
        Vector3 _position = _packet.ReadVector3();
        Quaternion _rotation = _packet.ReadQuarternion();

        ClientManager.Instance.SpawnPlayer(_id, _username, _position, _rotation);
    }

    public static void PlayerPosition(Packet _packet) {
        int _id = _packet.ReadInt();
        Vector3 _position = _packet.ReadVector3();

        // For debugging purposes.
        // Debug.Log($"ClientReceive::PlayerPosition::() Changing transform of player with id: {_id}");

        ClientManager.players[_id].transform.position = _position;
    }

    public static void PlayerRotation(Packet _packet) {
        int _id = _packet.ReadInt();
        Quaternion _rotation = _packet.ReadQuarternion();

        ClientManager.players[_id].transform.rotation = _rotation;
    }

    public static void Pong(Packet _packet) {
        Debug.Log("Pong received ...");

        string _message = _packet.ReadString();
        string _serverAddress = _packet.ReadString();
        //int _maximalPlayers = _packet.ReadInt();

        // might not be actual current...
        // without relay server I would have to constantly ping the server
        // or have a udp stream constantly checking data...
        //int _currentPlayers = _packet.ReadInt();

        //string address = _remoteEndPoint.ToString().Split(':')[0];
        Debug.Log("Message was: " + _message + ", serverAddress: " + _serverAddress);

        // Adding receive Pong to address...
        //LanManager.Instance.AddAddress(_serverAddress);
        ClientLANHelper.Instance.AddAddress(_serverAddress);
    }
    #endregion
}