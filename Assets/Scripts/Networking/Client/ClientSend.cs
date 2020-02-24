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

            // TODO: Implement Username
            // Not found a better way of getting the username field text.
            //_packet.Write(UIManager.Instance.GetUsernameText());
            _packet.Write("REPLACEMENT USER NAME");

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
    #endregion
}