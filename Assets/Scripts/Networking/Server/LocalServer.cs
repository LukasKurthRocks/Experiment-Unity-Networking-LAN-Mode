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

    // TODO: check if needed.
    public static bool _isStarted = false;

    public static void Start(int _maxPlayers, int _portNumber) {
        MaxPlayers = _maxPlayers;
        Port = _portNumber;

        Debug.Log("LocalServer::Start(): Starting server...");
        InitializeServerData();

        // TODO: Throw user back to menu is connection could not be set!
        try {
            _tcpListener = new TcpListener(IPAddress.Any, Port);
            _tcpListener.Start();
            _tcpListener.BeginAcceptTcpClient(new AsyncCallback(TCPConnectCallback), null);
        } catch (Exception _exception) {
            Stop();
            Debug.LogError($"LocalServer::Start(): Exception while trying to create tcp connection on port '{Port}': {_exception}");
            throw _exception; // Remove when throwback
        }

        try {
            _udpListener = new UdpClient(Port);
            _udpListener.BeginReceive(UDPReceiveCallback, null);
        } catch (Exception _exception) {
            Debug.LogError($"LocalServer::Start(): Exception while trying to create udp connection on port '{Port}': {_exception}");
            throw _exception; // Remove when throwback
        }

        _isStarted = true;
        Debug.Log($"LocalServer::Start(): Started server on Port {Port}");
    }

    private static void TCPConnectCallback(IAsyncResult _asyncResult) {
        // Store returned value as "tcp client instance"
        TcpClient _client = _tcpListener.EndAcceptTcpClient(_asyncResult);
        _tcpListener.BeginAcceptTcpClient(new AsyncCallback(TCPConnectCallback), null);

        Debug.Log($"LocalServer::TCPConnectCallback(): Incoming connection from {_client.Client.RemoteEndPoint}...");

        for (int i = 1; i <= MaxPlayers; i++) {
            if (clients[i].tcp.socket == null) {
                clients[i].tcp.Connect(_client);
                return;
            }
        }

        Debug.Log($"LocalServer::TCPConnectCallback(): {_client.Client.RemoteEndPoint} failed to connect: Server full!");
    }

    private static void UDPReceiveCallback(IAsyncResult _asyncResult) {
        try {
            IPEndPoint _clientEndPoint = new IPEndPoint(IPAddress.Any, 0);
            byte[] _data = _udpListener.EndReceive(_asyncResult, ref _clientEndPoint);

            // "Dont miss any incoming data"
            _udpListener.BeginReceive(UDPReceiveCallback, null);

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
                    /*
                     * Here is the position I could implement the "ping" if not using the localping server.
                     * One Problem is: first value would be the _packetID, not clientID.
                     * Another thing: Most of the stuffed is not ready handling ping data,
                     * as ping just gets resend to the one who sends it.
                    */

                    //HandleNonClientData( _packet);
                    return;
                }

                Debug.Log("received client id: " + _clientId);

                // This should normally be reached via MaxPlayer count in tcp.
                // No client should send data via udp if not connected. But it MIGHT BE happening.
                if(clients.Count < _clientId) {
                    Debug.LogError($"LocalServer::UDPReceiveCallback(): There is no client available for playerID '{_clientId}' when players dictionary count is '{clients.Count}'.");
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
            Debug.Log($"LocalServer::UDPReceiveCallback(): UDP object has already been disposed, is server still open?: {_exception}");
        } catch (Exception _exception) {
            Debug.Log($"LocalServer::UDPReceiveCallback(): Error receiving UDP data: {_exception}");
        }
    }

    /*
    /// <summary>Catching non-client data and handling it like it should.</summary>
    private static void HandleNonClientData(Packet _packet) {
        Debug.Log("Handling non client data received...");

        string _packetMessage = _packet.ReadString();

        Debug.Log($"Received 0 with message: {_packetMessage}. Calling packet Handler...");

        packetHandlers[0](0, _packet);
    }
    */

    public static void SendUDPData(IPEndPoint _clientEndPoint, Packet _packet) {
        try {
            if (_clientEndPoint != null) {
                _udpListener.BeginSend(_packet.ToArray(), _packet.Length(), _clientEndPoint, null, null);
            }
        } catch (Exception _exception) {
            Debug.Log($"LocalServer::SendUDPData(): Error sending data to {_clientEndPoint} via UDP: {_exception}");
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
        Debug.Log("LocalServer::InitializeServerData(): Initialized packets.");
    }

    /// <summary>Calling stop everytime Unity's PlayMode or application is closed.</summary>
    public static void Stop() {
        if (_tcpListener != null)
            _tcpListener.Stop();
        if (_udpListener != null)
            _udpListener.Close();

        clients.Clear();
        _isStarted = false;
    }
}