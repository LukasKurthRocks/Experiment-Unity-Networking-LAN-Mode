using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;


public class ServerReceive {
    #region Packets
    public static void Ping(int _fromClient, Packet _packet) {


        // Send a pong to the remote (client)
        //byte[] str = Encoding.ASCII.GetBytes("pong");
        //_socketServer.SendTo(str, _remoteEndPoint);
    }
    #endregion

    #region OldPackets
    /*
    public static void WelcomeReceived(int _fromClient, Packet _packet) {
        // Just checking for empty packets. Should not happen, happened once!
        if (_packet == null) {
            Debug.LogError("ServerReceive::WelcomeReceived(): Welcome receive packet is null.");
            return;
        }

        int _clientIdCheck = _packet.ReadInt();
        string _username = _packet.ReadString();

        Debug.Log($"ServerReceive::WelcomeReceived(): {Server.clients[_fromClient].tcp.socket.Client.RemoteEndPoint} connected successfully and is now player {_fromClient}.");
        if (_fromClient != _clientIdCheck) {
            Debug.Log($"ServerReceive::WelcomeReceived(): Player \"{_username}\" (ID: {_fromClient}) has assumed the wrong client ID ({_clientIdCheck})!");
        }

        // Send player into game
        if (Server.clients[_fromClient] == null) {
            Debug.LogError("ServerReceive::WelcomeReceived(): Trying to send player into game. Client itself is null though...");
        }
        Server.clients[_fromClient].SendIntoGame(_username);
    }

    public static void PlayerMovement(int _fromClient, Packet _packet) {
        bool[] _inputs = new bool[_packet.ReadInt()];
        for (int i = 0; i < _inputs.Length; i++) {
            _inputs[i] = _packet.ReadBool();
        }

        Quaternion _rotation = _packet.ReadQuarternion();

        // Setting player input when received, so server side player clone moves along.
        Server.clients[_fromClient].player.GetComponent<PlayerController>().SetPlayerInput(_inputs, _rotation);
    }
    */
    #endregion
}
