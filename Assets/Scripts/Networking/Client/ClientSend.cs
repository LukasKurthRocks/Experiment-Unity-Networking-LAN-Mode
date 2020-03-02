using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

public class ClientSend : MonoBehaviour {
    #region Send Data Functions
    private static void SendTCPData(Packet _packet) {
        _packet.WriteLength();
        ClientConnector.Instance.tcp.SendData(_packet);
    }

    private static void SendUDPData(Packet _packet) {
        _packet.WriteLength();
        ClientConnector.Instance.udp.SendData(_packet);
    }
    #endregion

    #region Packets
    public static void WelcomeReceived() {
        using (Packet _packet = new Packet((int)ClientPackets.welcomeReceived)) {
            _packet.Write(ClientConnector.Instance.myId);

            // Not found a better way of getting the username field text yet.
            _packet.Write(UIManager.Instance.GetUsernameText());

            SendTCPData(_packet);
        }
    }

    public static void PlayerMovement(bool[] _inputs) {
        using (Packet _packet = new Packet((int)ClientPackets.playerMovement)) {
            _packet.Write(_inputs.Length);
            foreach (bool _input in _inputs) {
                _packet.Write(_input);
            }

            _packet.Write(ClientManager.players[ClientConnector.Instance.myId].transform.rotation);

            // UDP "can afford" to loose some pakets. Just movement in here.
            // PLUS: UDP is faster than TCP.
            SendUDPData(_packet);
        }
    }

    // INFO: Only in LAN: Sending PlayerMovement directly to server.
    // This is for having the server to relay it directly to the clients.
    public static void PlayerMovement(Vector3 position, Quaternion rotation) {
        using (Packet _packet = new Packet((int)ClientPackets.playerMovement)) {
            _packet.Write(position);
            _packet.Write(rotation);

            // UDP "can afford" to loose some pakets. Just movement in here.
            // PLUS: UDP is faster than TCP.
            SendUDPData(_packet);
        }
    }
    #endregion
}