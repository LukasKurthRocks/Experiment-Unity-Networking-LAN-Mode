using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

// Local Server Logic ...
// Part is still split in LANManager and NetworkManager

public class LocalServer {
    public static int MaxPlayers { get; private set; }
    public static int Port { get; private set; }
    //public static Dictionary<int, LANClient> clients = new Dictionary<int, LANClient>();

    public static int dataBufferSize = 4096;

    public delegate void PacketHandler(ref EndPoint _remoteEndPoint, ref Socket _socketServer, Packet _packet);
    public static Dictionary<int, PacketHandler> packetHandlers;

    private static Socket _socketServer;
    private static EndPoint _remoteEndPoint;

    private static Packet _receivedData; // Handling data
    private static byte[] _receiveBuffer;

    public static void Start(int _maxPlayers, int _portNumber) {
        MaxPlayers = _maxPlayers;
        Port = _portNumber;

        // Handling data
        _receivedData = new Packet();
        _receiveBuffer = new byte[dataBufferSize];

        Debug.Log("LANServer::Start(): Starting server...");
        InitializeServerData();

        if (_socketServer == null) {
            try {
                // TODO. Find out which is better: new UdpClient(Port);
                _socketServer = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

                if (_socketServer == null) {
                    Debug.LogWarning("LANServer::Start(): SocketServer creation failed");
                    return;
                }

                // Check if we received pings
                _socketServer.Bind(new IPEndPoint(IPAddress.Any, Port));

                // incoming traffic endpoint
                _remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);

                //_socketServer.BeginReceiveFrom(new byte[1024], 0, 1024, SocketFlags.None, ref _remoteEndPoint, new AsyncCallback(OnReceive), null);
                _socketServer.BeginReceiveFrom(_receiveBuffer, 0, 1024, SocketFlags.None, ref _remoteEndPoint, new AsyncCallback(ServerReceiveCallback), null);
            } catch (Exception ex) {
                Debug.Log(ex.Message);
            }

            Debug.Log($"LANServer::Start(): Started server on port {Port}");
        }
    }

    private static void ServerReceiveCallback(IAsyncResult _asyncResult) {
        Debug.Log("LANServer::SocketOnReceiveCallBack(): received ...");
        if (_socketServer != null) {
            try {
                int size = _socketServer.EndReceiveFrom(_asyncResult, ref _remoteEndPoint);
                _socketServer.BeginReceiveFrom(_receiveBuffer, 0, 1024, SocketFlags.None, ref _remoteEndPoint, new AsyncCallback(ServerReceiveCallback), null);

                if (size <= 0) {
                    // TODO: Disconnecting client/player?
                    return;
                }

                // Having data, copying bytes into array
                byte[] _data = new byte[size];
                Array.Copy(sourceArray: _receiveBuffer, destinationArray: _data, length: size);

                // int = 4, no more data...
                if (_data.Length < 4) {
                    // TODO: Disconnecting client!?
                    //Instance.Disconnect();
                    return;
                }

                // Handle data
                _receivedData.Reset(HandleData(_data));
            } catch (Exception ex) {
                Debug.Log(ex.ToString());
            }
        }
    }

    private static bool HandleData(byte[] _data) {
        int _packetLength = 0;

        _receivedData.SetBytes(_data);

        // int has 4 bytes
        if (_receivedData.UnreadLength() >= 4) {
            _packetLength = _receivedData.ReadInt();
            if (_packetLength <= 0) {
                return true;
            }
        }

        // as long as we get data...
        while (_packetLength > 0 && _packetLength <= _receivedData.UnreadLength()) {
            byte[] _packetBytes = _receivedData.ReadBytes(_packetLength);

            // Note: _socketServer.RemoteEndPoint did not work!?
            using (Packet _packet = new Packet(_packetBytes)) {
                int _packetId = _packet.ReadInt();
                Debug.Log($"LANServer::HandleData(): Calling packetHandler[{_packetId}]({_remoteEndPoint}, {_socketServer.ToString()}, {_packet.ToString()})!");
                packetHandlers[_packetId](ref _remoteEndPoint, ref _socketServer, _packet);
            }

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

        // partial packet left in data
        return false;
    }

    public static void Stop() {
        if (_socketServer != null) {
            _socketServer.Close();
            _socketServer = null;

            Debug.Log("LANServer::CloseServer(): Closed socket on server.");
        }

        //clients.Clear();
    }

    // TODO: Create this for the rest of the funcs.
    public static void SendUDPData(EndPoint _clientEndPoint, Packet _packet) {
    }


    // TODO: CHeck if only usable for ping?
    public static void SendUDPData(Packet _packet) {
        try {
            if (_remoteEndPoint != null) {
                _socketServer.SendTo(_packet.ToArray(), _packet.Length(), SocketFlags.None, _remoteEndPoint);
            }
        } catch (Exception _exception) {
            Debug.Log($"Server::SendUDPData(): Error sending data to {_remoteEndPoint} via UDP: {_exception}");
        }
    }

    private static void InitializeServerData() {
        /*
        for (int i = 1; i <= MaxPlayers; i++) {
            clients.Add(i, new LANClient(i));
        }
        */

        packetHandlers = new Dictionary<int, PacketHandler>() {
            { (int)ClientPackets.ping, LocalServerReceive.Ping }
        };
        Debug.Log("Server::InitializeServerData(): Initialized packets.");
    }

    // TODO: Move this somewhere else!!
    /*
    private void OnApplicationQuit() {
        CloseServer();
    }
    */
}