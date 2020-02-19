using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;

public class NetworkManager : Singleton<NetworkManager> {
    private Socket _socketServer;
    private EndPoint _remoteEndPoint;

    void StartServer(int port) {
        if (_socketServer == null) {
            try {
                _socketServer = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

                if (_socketServer == null) {
                    Debug.LogWarning("SocketServer creation failed");
                    return;
                }

                // Check if we received pings
                _socketServer.Bind(new IPEndPoint(IPAddress.Any, port));

                _remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);

                _socketServer.BeginReceiveFrom(new byte[1024], 0, 1024, SocketFlags.None, ref _remoteEndPoint, new AsyncCallback(OnReceive), null);
            } catch (Exception ex) {
                Debug.Log(ex.Message);
            }

            Debug.Log($"Started server on port {port}");
        }
    }

    void OnReceive(IAsyncResult _asyncResult) {

    }
}