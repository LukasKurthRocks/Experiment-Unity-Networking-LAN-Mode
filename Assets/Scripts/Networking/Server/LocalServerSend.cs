using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

public class LocalServerSend {
    #region Send Data Functions
    private static void SendTCPData(int _toClient, Packet _packet) {
        _packet.WriteLength();
        LocalServer.clients[_toClient].tcp.SendData(_packet);
    }

    private static void SendUDPData(int _toClient, Packet _packet) {
        _packet.WriteLength();
        LocalServer.clients[_toClient].udp.SendData(_packet);
    }

    private static void SendTCPDataToAll(Packet _packet) {
        _packet.WriteLength();
        for (int i = 1; i <= LocalServer.MaxPlayers; i++) {
            LocalServer.clients[i].tcp.SendData(_packet);
        }
    }

    private static void SendTCPDataToAll(int _exceptClient, Packet _packet) {
        _packet.WriteLength();
        for (int i = 1; i <= LocalServer.MaxPlayers; i++) {
            if (i != _exceptClient) {
                LocalServer.clients[i].tcp.SendData(_packet);
            }
        }
    }

    private static void SendUDPDataToAll(Packet _packet) {
        _packet.WriteLength();
        for (int i = 1; i <= LocalServer.MaxPlayers; i++) {
            LocalServer.clients[i].udp.SendData(_packet);
        }
    }

    private static void SendUDPDataToAll(int _exceptClient, Packet _packet) {
        _packet.WriteLength();
        for (int i = 1; i <= LocalServer.MaxPlayers; i++) {
            if (i != _exceptClient) {
                LocalServer.clients[i].udp.SendData(_packet);
            }
        }
    }
    #endregion

    #region LAN Packets
    public static void SendPong() {
        Debug.Log($"LocalServerSend::SendPong(): Sending pong with adress: {LocalPingServer.GetLocalAddress()}");
        using (Packet _returnPacket = new Packet((int)ServerPackets.pong)) {
            _returnPacket.Write("pong");

            // Sending informative data
            _returnPacket.Write(LocalPingServer.GetLocalAddress());
            //_returnPacket.Write(LocalServer.MaxPlayers);
            //_returnPacket.Write(LocalServer.clients.Count);

            _returnPacket.WriteLength();

            // Sending ping data back to the remove endpoint how did SendPing()
            LocalPingServer.SendUDPData(_returnPacket);
        }
    }
    #endregion

    #region Packets
    /// <summary>
    /// Sending the connecting player a welcome paket. TCP because important.
    /// </summary>
    public static void Welcome(int _toClient, string _message) {
        using (Packet _packet = new Packet((int)ServerPackets.welcome)) {
            _packet.Write(_message);
            _packet.Write(_toClient);

            SendTCPData(_toClient, _packet);
        }
    }

    /// <summary>
    /// Spawning the player. Send over TCP, because loosing this paket is not an option.
    /// </summary>
    public static void SpawnPlayer(int _toClient, Player _player) {
        using (Packet _packet = new Packet((int)ServerPackets.spawnPlayer)) {
            _packet.Write(_player.GetPlayerID());
            _packet.Write(_player.GetUsername());
            _packet.Write(_player.transform.position);
            _packet.Write(_player.transform.rotation);

            SendTCPData(_toClient, _packet);
        }
    }

    /// <summary>
    /// Spawning the master client. Send over TCP, because loosing this paket is not an option.
    /// </summary>
    public static void SpawnPlayer(int _toClient, int _id, string _username, Vector3 _position, Quaternion _rotation) {
        using (Packet _packet = new Packet((int)ServerPackets.spawnPlayer)) {
            _packet.Write(_id);
            _packet.Write(_username);
            _packet.Write(_position);
            _packet.Write(_rotation);

            // TCP because important information!!
            SendTCPData(_toClient, _packet);
        }
    }

    /// <summary>
    /// Sending player position data to users. Can afford to loose data, so using UDP.
    /// </summary>
    public static void PlayerPosition(Player _player) {
        using (Packet _packet = new Packet((int)ServerPackets.playerPosition)) {
            _packet.Write(_player.GetPlayerID());
            _packet.Write(_player.transform.position);

            SendUDPDataToAll(_packet);
        }
    }

    /// <summary>
    /// Sending master client position data to users. Can afford to loose data, so using UDP.
    /// </summary>
    public static void PlayerPosition(int _id, Vector3 _position) {
        using (Packet _packet = new Packet((int)ServerPackets.playerPosition)) {
            _packet.Write(_id);
            _packet.Write(_position);

            SendUDPDataToAll(_packet);
        }
    }

    /// <summary>
    /// Sending player rotation data to users. Can afford to loose data, so using UDP.
    /// </summary>
    public static void PlayerRotation(Player _player) {
        using (Packet _packet = new Packet((int)ServerPackets.playerRotation)) {
            _packet.Write(_player.GetPlayerID());
            _packet.Write(_player.transform.rotation);

            SendUDPDataToAll(_exceptClient: _player.GetPlayerID(), _packet);
        }
    }

    /// <summary>
    /// Sending master client rotation data to users. Can afford to loose data, so using UDP.
    /// </summary>
    public static void PlayerRotation(int _id, Quaternion _rotation) {
        using (Packet _packet = new Packet((int)ServerPackets.playerRotation)) {
            _packet.Write(_id);
            _packet.Write(_rotation);

            SendUDPDataToAll(_exceptClient: _id, _packet);
        }
    }
    #endregion
}