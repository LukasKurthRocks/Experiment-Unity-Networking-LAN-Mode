using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class LocalServerReceive {
    #region Packets
    // TODO: Remove or Keep?
    public static void Ping(ref EndPoint _remoteEndPoint, ref Socket _socketServer,  Packet _packet) {
        Debug.Log($"LocalServerReceive::Ping(): Ping from _remoteEndPoint: {_remoteEndPoint}, localEndPoint: {_socketServer.LocalEndPoint}");

        if (_packet == null)
            Debug.Log("Received empty ping packet.");

        //id = _packet.ReadInt();
        //username = _packet.ReadString();

        LocalServerSend.SendPong(ref _socketServer, ref _remoteEndPoint);
    }

    // TODO: Remove or Keep?
    public static void Ping(string _remoteEndPoint, Packet _packet) {
        Debug.Log($"LocalServerReceive::Ping(): Ping received from '{_remoteEndPoint}'. Sending pong ...");

        if (_packet == null)
            Debug.Log("Received empty ping packet.");

        LocalServerSend.SendPong();
    }

    // TODO: Remove or Keep?
    public static void Ping(int _fromClient, Packet _packet) {
        Debug.Log($"LocalServerReceive::Ping(): Ping received. Sending pong ...");

        if (_packet == null)
            Debug.Log("Received empty ping packet.");

        LocalServerSend.SendPong();
    }

    // TODO: Remove or Keep?
    public static void Ping(Packet _packet) {
        Debug.Log($"LocalServerReceive::Ping(): Ping received. Sending pong ...");

        if (_packet == null)
            Debug.Log("Received empty ping packet.");

        LocalServerSend.SendPong();
    }
    #endregion

    #region Standard Packets
    public static void WelcomeReceived(int _fromClient, Packet _packet) {
        // Just checking for empty packets. Should not happen, happened once!
        if (_packet == null) {
            Debug.LogError("ServerReceive::WelcomeReceived(): Welcome receive packet is null.");
            return;
        }

        int _clientIdCheck = _packet.ReadInt();
        string _username = _packet.ReadString();

        Debug.Log($"ServerReceive::WelcomeReceived(): {LocalServer.clients[_fromClient].tcp.socket.Client.RemoteEndPoint} connected successfully and is now player {_fromClient}.");
        if (_fromClient != _clientIdCheck) {
            Debug.Log($"ServerReceive::WelcomeReceived(): Player \"{_username}\" (ID: {_fromClient}) has assumed the wrong client ID ({_clientIdCheck})!");
        }

        // Send player into game
        if (LocalServer.clients[_fromClient] == null) {
            Debug.LogError("ServerReceive::WelcomeReceived(): Trying to send player into game. Client itself is null though...");
        }
        LocalServer.clients[_fromClient].SendIntoGame(_username);
    }

    // TODO: Should I have to so this via playerInput?
    // Local Server should just receive Client input, not calculating it...
    public static void PlayerMovement(int _fromClient, Packet _packet) {
        bool[] _inputs = new bool[_packet.ReadInt()];
        for (int i = 0; i < _inputs.Length; i++) {
            _inputs[i] = _packet.ReadBool();
        }

        Quaternion _rotation = _packet.ReadQuarternion();

        Debug.Log("TODO: Set player input. Maybe set an interface here?");
        // Setting player input when received, so server side player clone moves along.
        //LocalServer.clients[_fromClient].player.GetComponent<PlayerController>().SetPlayerInput(_inputs, _rotation);

        // TODO: Test if this can be used!!
        //LocalServer.clients[_fromClient].player.GetComponent<IPlayerController>().SetPlayerInput(_inputs, _rotation);
    }
    #endregion
}