using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using UnityEngine;

[Obsolete("The OLD network manager is obsolete. Please replace it.", true)]
public class NetworkManager : Singleton<NetworkManager> {
    public static int dataBufferSize = 4096;

    public delegate void PacketHandler(ref EndPoint _remoteEndPoint, ref Socket _socketServer, Packet _packet);
    public static Dictionary<int, PacketHandler> packetHandlers;

    private Socket _socketServer;
    private EndPoint _remoteEndPoint;
    private Packet _receivedData; // Handling data
    private byte[] _receiveBuffer;

    private void Start() {
        // DoStart();
    }

    // TODO: MONO BEHAVIOUR! ( No Singleton ... )
    public void DoStart() {
        // Handling data
        _receivedData = new Packet();
        _receiveBuffer = new byte[dataBufferSize];

        InitializeServerData();

        // TODO: Later just do this via button or headless...
        StartServer(26950);
    }

    private void OnApplicationQuit() {
        CloseServer();
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

                //_socketServer.BeginReceiveFrom(new byte[1024], 0, 1024, SocketFlags.None, ref _remoteEndPoint, new AsyncCallback(OnReceive), null);
                _socketServer.BeginReceiveFrom(_receiveBuffer, 0, 1024, SocketFlags.None, ref _remoteEndPoint, new AsyncCallback(OnReceive), null);
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
        Debug.Log("OnReceive()...");
        if (_socketServer != null) {
            try {
                int size = _socketServer.EndReceiveFrom(_asyncResult, ref _remoteEndPoint);
                //_socketServer.BeginReceiveFrom(new byte[1024], 0, 1024, SocketFlags.None, ref _remoteEndPoint, new AsyncCallback(OnReceive), null);
                _socketServer.BeginReceiveFrom(_receiveBuffer, 0, 1024, SocketFlags.None, ref _remoteEndPoint, new AsyncCallback(OnReceive), null);

                if (size <= 0) {
                    // Disconnecting client/player
                    Debug.LogWarning("size <= 0");
                    return;
                }
                
                // Having data, copying bytes into array
                byte[] _data = new byte[size];

                if (_receiveBuffer == null)
                    Debug.LogWarning("_receiveBuffer == null");
                if (_data == null)
                    Debug.LogWarning("_data == null");
                if (size == 0)
                    Debug.LogWarning("size == 0");

                Debug.Log("NM: Copying array ...");
                Array.Copy(sourceArray: _receiveBuffer, destinationArray: _data, length: size);

                // int = 4, no more data...
                if (_data.Length < 4) {
                    Debug.LogWarning("_data.Lengt < 4");
                    // Disconnecting client
                    //Instance.Disconnect();
                    return;
                }

                // Handle data
                Debug.Log("Calling handle data ...");
                _receivedData.Reset(HandleData(_data));

                // Send a pong to the remote (client)
                //byte[] str = Encoding.ASCII.GetBytes("pong");
                //_socketServer.SendTo(str, _remoteEndPoint);
            } catch (Exception ex) {
                Debug.Log(ex.ToString());
            }
        }
    }

    private bool HandleData(byte[] _data) {
        Debug.Log("Handling Data ...");
        int _packetLength = 0;

        _receivedData.SetBytes(_data);

        // int has 4 bytes
        if (_receivedData.UnreadLength() >= 4) {
            _packetLength = _receivedData.ReadInt();
            if (_packetLength <= 0) {
                Debug.Log("Finished Handling Data!");
                return true;
            }

            // TODO: Not working with "old" LanManager. It sends string directly, so no Packet.
            Debug.Log($"_packetLength = {_packetLength}");
        }

        // as long as we get data...
        while (_packetLength > 0 && _packetLength <= _receivedData.UnreadLength()) {
            Debug.Log($"Having data with packetLength: {_packetLength}");
            byte[] _packetBytes = _receivedData.ReadBytes(_packetLength);

            Debug.Log("NM:: Before handling the actual packet ...");
            // Note: _socketServer.RemoteEndPoint did not work!?
            using (Packet _packet = new Packet(_packetBytes)) {
                int _packetId = _packet.ReadInt();
                Debug.Log($"Calling packetHandler[{_packetId}]({_remoteEndPoint}, {_socketServer.ToString()}, {_packet.ToString()})!");
                packetHandlers[_packetId](ref _remoteEndPoint, ref _socketServer, _packet);
            }
            Debug.Log("NM:: After _packet ...");

            _packetLength = 0;

            // int has 4 bytes
            if (_receivedData.UnreadLength() >= 4) {
                _packetLength = _receivedData.ReadInt();
                if (_packetLength <= 0) {
                    return true;
                }
            }
        }

        // return the received data
        if (_packetLength <= 1) {
            return true;
        }

        Debug.Log($"Still having data. Returning false...");
        // partial packet left in data
        return false;
    }

    private static void InitializeServerData() {
        // TODO: Initialize Client Array

        packetHandlers = new Dictionary<int, PacketHandler>() {
            { (int)ClientPackets.ping, LocalServerReceive.Ping }
        };
        Debug.Log("Server::InitializeServerData(): Initialized packets.");
    }
}