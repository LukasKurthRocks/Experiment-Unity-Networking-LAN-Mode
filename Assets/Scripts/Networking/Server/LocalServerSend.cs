using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

public class LocalServerSend : MonoBehaviour {

    // TODO: Remove Socket and IPEndPoint.
    public static void SendPong(ref Socket _socketServer, ref EndPoint _remoteEndPoint) {
        using (Packet _returnPacket = new Packet((int)ServerPackets.pong)) {
            _returnPacket.Write("pong");
            _returnPacket.WriteLength();

            //_socketServer.BeginSend()
            //_socketServer.SendTo();
            //_socketServer.SendTo(_returnPacket.ToArray(), _returnPacket.Length(), SocketFlags.None, _remoteEndPoint);
            LocalServer.SendUDPData(_remoteEndPoint, _returnPacket);
        }
    }
}