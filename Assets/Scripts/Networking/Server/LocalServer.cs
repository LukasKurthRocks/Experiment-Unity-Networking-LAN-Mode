using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

public class LocalServer {
    public static int MaxPlayers { get; private set; }
    public static int Port { get; private set; }

    public static Dictionary<int, ServerClientConnector> clients = new Dictionary<int, ServerClientConnector>();
    public delegate void PacketHandler(int _fromClient, Packet _packet);
    public static Dictionary<int, PacketHandler> packetHandlers;

    private static TcpListener _tcpListener;
    private static UdpClient _udpListener;

    public static void Start(int _maxPlayers, int _portNumber) {
        MaxPlayers = _maxPlayers;
        Port = _portNumber;

        Debug.Log("Server::Start(): Starting server...");
        InitializeServerData();

        // TODO: Throw user back to menu is connection could not be set!
        try {
            _tcpListener = new TcpListener(IPAddress.Any, Port);
            _tcpListener.Start();
            _tcpListener.BeginAcceptTcpClient(new AsyncCallback(TCPConnectCallback), null);
        } catch (Exception _exception) {
            Debug.LogError($"Server::Start(): Exception while trying to create tcp connection on port '{Port}': {_exception}");
            throw _exception; // Remove when throwback
        }

        try {
            _udpListener = new UdpClient(Port);
            _udpListener.BeginReceive(UDPReceiveCallback, null);
        } catch (Exception _exception) {
            Debug.LogError($"Server::Start(): Exception while trying to create udp connection on port '{Port}': {_exception}");
            throw _exception; // Remove when throwback
        }

        Debug.Log($"Server::Start(): Started server on Port {Port}");
    }

    private static void TCPConnectCallback(IAsyncResult _asyncResult) {
        // Store returned value as "tcp client instance"
        TcpClient _client = _tcpListener.EndAcceptTcpClient(_asyncResult);
        _tcpListener.BeginAcceptTcpClient(new AsyncCallback(TCPConnectCallback), null);

        Debug.Log($"Server::TCPConnectCallback(): Incoming connection from {_client.Client.RemoteEndPoint}...");

        for (int i = 1; i <= MaxPlayers; i++) {
            if (clients[i].tcp.socket == null) {
                clients[i].tcp.Connect(_client);
                return;
            }
        }

        Debug.Log($"Server::TCPConnectCallback(): {_client.Client.RemoteEndPoint} failed to connect: Server full!");
    }

    private static void UDPReceiveCallback(IAsyncResult _asyncResult) {
        try {
            IPEndPoint _clientEndPoint = new IPEndPoint(IPAddress.Any, 0);
            byte[] _data = _udpListener.EndReceive(_asyncResult, ref _clientEndPoint);

            // "Dont miss any incoming data"
            _udpListener.BeginReceive(UDPReceiveCallback, null);

            // We could ask the udp packet types in here i guess ...
            // TODO: Check if I SHOULD do this, as this would combine both servers.
            // Maybe just leaving as it is?
            // Comparing PacketID, PlayerID, STRING, LENGTH (WelcomeReceived) to PacketID, STRING, LENGTH (LanHelper=>Ping())

            // int = 4, no more data...
            if (_data.Length < 4) {
                return;
            }

            // additional checks for future implementation / checks...
            using (Packet _packet = new Packet(_data)) {
                int _clientId = _packet.ReadInt();

                // check for invalid clientId...
                // should never get into here!
                if (_clientId == 0) {
                    return;
                }

                Debug.Log("received client id: " + _clientId);

                // This should normally be reached via MaxPlayer count in tcp.
                // No client should send data via udp if not connected. But it MIGHT BE happening.
                if(clients.Count < _clientId) {
                    Debug.LogError($"Server::UDPReceiveCallback(): There is no client available for playerID '{_clientId}' when players dictionary count is '{clients.Count}'.");
                    return;
                }

                // Creating a new connection.
                // empty packet that open the clients port.
                if (clients[_clientId].udp.endPoint == null) {
                    // create new connection and returning out of method before handling data is needed...
                    clients[_clientId].udp.Connect(_clientEndPoint);
                    return;
                }

                // client id check | hacking impersonation prevention
                // converting to string to campare it properly.
                if (clients[_clientId].udp.endPoint.ToString() == _clientEndPoint.ToString()) {
                    clients[_clientId].udp.HandleData(_packet);
                }
            }
        } catch (ObjectDisposedException _exception) {
            // When exiting PlayMode, this exception in thrown.
            // Catching it, in case it happens when NOT exiting PlayMode.
            Debug.Log($"Server::UDPReceiveCallback(): UDP object has already been disposed, is server still open?: {_exception}");
        } catch (Exception _exception) {
            Debug.Log($"Server::UDPReceiveCallback(): Error receiving UDP data: {_exception}");
        }
    }

    public static void SendUDPData(IPEndPoint _clientEndPoint, Packet _packet) {
        try {
            if (_clientEndPoint != null) {
                _udpListener.BeginSend(_packet.ToArray(), _packet.Length(), _clientEndPoint, null, null);
            }
        } catch (Exception _exception) {
            Debug.Log($"Server::SendUDPData(): Error sending data to {_clientEndPoint} via UDP: {_exception}");
        }
    }

    /*
     * INFO for (not only) me:
     * Searched some time later WHERE IN HELL the packet handler is called.
     * So quick guide in this comment:
     * Receiving inside UDPReceiveCallback
     *   => Server.clients[_clientID].UDP.HandleData(_data) (Client/ServerClientConnector.UDP.HandleData())
     *   => LocalServer.packetHandlers[_packetId](_id, _packet);
     * In words: Server getting the UDP Callback, calling the client for handling udp data,
     *   client then calling the servers packetHandler dictionary ...
     *   
     * Unfortunately this can not be used for ping, as this would require an existing UDP connection!!
     */
    private static void InitializeServerData() {
        for (int i = 1; i <= MaxPlayers; i++) {
            clients.Add(i, new ServerClientConnector(i));
        }

        packetHandlers = new Dictionary<int, PacketHandler>() {
            //{ (int)ClientPackets.ping, LocalServerReceive.Ping },
            { (int)ClientPackets.welcomeReceived, LocalServerReceive.WelcomeReceived },
            { (int)ClientPackets.playerMovement, LocalServerReceive.PlayerMovement }
        };
        Debug.Log("Server::InitializeServerData(): Initialized packets.");
    }

    /// <summary>Calling stop everytime Unity's PlayMode or application is closed.</summary>
    public static void Stop() {
        if (_tcpListener != null)
            _tcpListener.Stop();
        if (_udpListener != null)
            _udpListener.Close();

        clients.Clear();
    }
}