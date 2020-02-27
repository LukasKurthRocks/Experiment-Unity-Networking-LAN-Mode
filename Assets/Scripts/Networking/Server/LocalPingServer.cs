using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

// Creating a socket for answering to pings...

public class LocalPingServer {
    public static int Port { get; private set; }

    public static int dataBufferSize = 4096;

    // TODO: Check is this is enough. Packets in server having "string _remoteIP, Packet _packet".
    //public delegate void PacketHandler(ref EndPoint _remoteEndPoint, ref Socket _socketServer, Packet _packet);
    public delegate void PacketHandler(string _remoteConnection, Packet _packet);
    //public delegate void PacketHandler(Packet _packet);
    public static Dictionary<int, PacketHandler> packetHandlers;

    private static Socket _socketServer;
    private static EndPoint _remoteEndPoint;

    private static Packet _receivedData; // Handling data
    private static byte[] _receiveBuffer;

    // TODO: Using this somewhere.
    //private static bool _isStarted = false;

    public static void Start(int _portNumber) {
        Port = _portNumber;

        // Handling data
        _receivedData = new Packet();
        _receiveBuffer = new byte[dataBufferSize];

        Debug.Log("LANServer::Start(): Starting server...");
        InitializeServerData();

        if (_socketServer == null) {
            try {
                // TODO. Find out which is better: this or "new UdpClient(Port);"!?
                _socketServer = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

                if (_socketServer == null) {
                    Debug.LogWarning("LANServer::Start(): SocketServer creation failed");
                    return;
                }

                // Check if we received pings
                IPEndPoint _serverEndPoint = new IPEndPoint(IPAddress.Any, Port);

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

        //_isStarted = true;
    }

    private static void ServerReceiveCallback(IAsyncResult _asyncResult) {
        Debug.Log("LANServer::SocketOnReceiveCallBack(): received ...");
        if (_socketServer != null) {
            try {
                int size = _socketServer.EndReceiveFrom(_asyncResult, ref _remoteEndPoint);
                _socketServer.BeginReceiveFrom(_receiveBuffer, 0, 1024, SocketFlags.None, ref _remoteEndPoint, new AsyncCallback(ServerReceiveCallback), null);

                if (size <= 0) {
                    // On Server: Disconnect, On PingServer: Ignore!
                    return;
                }

                // Having data, copying bytes into array
                byte[] _data = new byte[size];
                Array.Copy(sourceArray: _receiveBuffer, destinationArray: _data, length: size);

                // int = 4, no more data...
                if (_data.Length < 4) {
                    // On Server: Disconnect, On PingServer: Ignore!
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

                //packetHandlers[_packetId](ref _remoteEndPoint, ref _socketServer, _packet);
                packetHandlers[_packetId](_remoteEndPoint.ToString(), _packet);
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

        //_isStarted = false;
    }

    // Sending packet back to the remote end point.
    public static void SendUDPData(Packet _packet) {
        try {
            if (_remoteEndPoint != null) {
                Debug.Log($"LocalServer::SendUDPData(): Sending with {_socketServer.LocalEndPoint}, {_remoteEndPoint}");
                _socketServer.SendTo(_packet.ToArray(), _packet.Length(), SocketFlags.None, _remoteEndPoint);
            }
        } catch (Exception _exception) {
            Debug.Log($"Server::SendUDPData(): Error sending data to {_remoteEndPoint} via UDP: {_exception}");
        }
    }

    // On the Ping Socket there are no clients!
    private static void InitializeServerData() {
        packetHandlers = new Dictionary<int, PacketHandler>() {
            { (int)ClientPackets.ping, LocalServerReceive.Ping }
        };
        Debug.Log("LANServer::InitializeServerData(): Initialized packets.");
    }

    // just for sending the pong, so local player has ip
    public static string GetLocalAddress() {
        IPEndPoint endPoint = _socketServer.LocalEndPoint as IPEndPoint;
        return endPoint.Address.ToString();
    }

    // TODO: Not only close the server in here.
    // Can not even close this in here because NOT MONOBEHAVIOUR!
    private void OnApplicationQuit() {
        Stop();
    }
}