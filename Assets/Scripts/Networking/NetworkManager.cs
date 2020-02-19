using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using UnityEngine;

public class NetworkManager : Singleton<NetworkManager> {
    public delegate void PacketHandler(int _fromClient, Packet _packet);
    public static Dictionary<int, PacketHandler> packetHandlers;

    private Socket _socketServer;
    private EndPoint _remoteEndPoint;
    private Packet _receivedData; // Handling data
    private byte[] _receiveBuffer;

    private void Start() {
        InitializeServerData();
    }

    public void StartServer(int port) {
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

    public void CloseServer() {
        if (_socketServer != null) {
            _socketServer.Close();
            _socketServer = null;

            Debug.Log("Closed socket on server.");
        }
    }

    // NOT WORKING. COMPARE OTHER SOLUTION
    void OnReceive(IAsyncResult _asyncResult) {
        if (_socketServer != null) {
            try {
                int size = _socketServer.EndReceiveFrom(_asyncResult, ref _remoteEndPoint);
                _socketServer.BeginReceiveFrom(new byte[1024], 0, 1024, SocketFlags.None, ref _remoteEndPoint, new AsyncCallback(OnReceive), null);

                if (size <= 0) {
                    // Disconnecting client/player
                    return;
                }

                // Having data, copying bytes into array
                byte[] _data = new byte[size];
                Array.Copy(_receiveBuffer, _data, size);

                
                // Handle data
                _receivedData.Reset(HandleData(_data));

                /*
                int _packetId = _packet.ReadInt();
                _packetHandlers[_packetId](_packet);
                */

                byte[] str = Encoding.ASCII.GetBytes("pong");

                // Send a pong to the remote (client)
                _socketServer.SendTo(str, _remoteEndPoint);

            } catch (Exception ex) {
                Debug.Log(ex.ToString());
            }
        }
    }

    private bool HandleData(byte[] _data) {


        return false;
    }

    private static void InitializeServerData() {
        // TODO: Initialize Client Array

        packetHandlers = new Dictionary<int, PacketHandler>() {
            { (int)ClientPackets.ping, ServerReceive.Ping }
        };
        Debug.Log("Server::InitializeServerData(): Initialized packets.");
    }
}