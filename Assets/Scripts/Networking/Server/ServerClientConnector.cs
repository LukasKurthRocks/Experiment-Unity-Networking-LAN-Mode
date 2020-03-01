using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

/// <summary>
/// This is basically the same as the ClientConnector (Client.cs) from Toms tutorial series.
/// Just adding the host as a client in here.
/// TODO: Part #8 of the series...
/// </summary>

public class ServerClientConnector {
    // Storing client Information
    public static int dataBufferSize = 4096; // 4096 Bytes = 4 MBytes? Where?

    public int id;
    public Player player; // Player reference
    public TCP tcp;
    public UDP udp;

    // Assign and initialize TCP
    public ServerClientConnector(int _clientID) {
        id = _clientID;
        tcp = new TCP(id);
        udp = new UDP(id);
    }

    public class TCP {
        // Server instance callback
        public TcpClient socket;

        private readonly int _id;
        private NetworkStream _networkStream;
        private Packet _receivedData; // Handling data
        private byte[] _receiveBuffer;

        public TCP(int _ID) {
            _id = _ID;
        }

        public void Connect(TcpClient _socket) {
            socket = _socket;
            socket.ReceiveBufferSize = dataBufferSize;
            socket.SendBufferSize = dataBufferSize;

            _networkStream = socket.GetStream();

            // Handling data
            _receivedData = new Packet();
            _receiveBuffer = new byte[dataBufferSize];

            _networkStream.BeginRead(_receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);

            // Send welcome to connected client
            Debug.Log($"SCC::REMOVE(): Sending welcome message to {_id}");
            LocalServerSend.Welcome(_id, "Welcome to the server!");
        }

        public void SendData(Packet _packet) {
            try {
                if (socket != null) {
                    _networkStream.BeginWrite(_packet.ToArray(), 0, _packet.Length(), null, null);
                }
            } catch (Exception _exception) {
                Debug.Log($"ClientConnectionConsolidator::TCP::SendData(): Error sending data to player {_id} via TCP: {_exception}");
            }
        }

        private void ReceiveCallback(IAsyncResult _asyncResult) {
            try {
                int _byteLength = _networkStream.EndRead(_asyncResult);
                if (_byteLength <= 0) {
                    // Disconnecting client/player
                    LocalServer.clients[_id].Disconnect();
                    return;
                }

                // Having data, copying bytes into array
                byte[] _data = new byte[_byteLength];
                Array.Copy(_receiveBuffer, _data, _byteLength);

                // Handle data
                _receivedData.Reset(HandleData(_data));

                // Continue reading data from stream
                _networkStream.BeginRead(_receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);
            } catch (Exception _exception) {
                Debug.Log($"ClientConnectionConsolidator::TCP::ReceiveCallback(): Error receiving TCP data: {_exception}");

                // Disconnecting client/player
                LocalServer.clients[_id].Disconnect();
            }
        }

        private bool HandleData(byte[] _data) {
            Debug.Log($"ServerClientConnector::TCP::HandleData(): Handling server side client tcp data");
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
                ThreadManager.ExecuteOnMainThread(() => {
                    using (Packet _packet = new Packet(_packetBytes)) {
                        int _packetId = _packet.ReadInt();
                        LocalServer.packetHandlers[_packetId](_id, _packet);
                    }
                });

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

        /// <summary>
        /// Properly handling client disconnections.
        /// </summary>
        public void Disconnect() {
            socket.Close();
            _networkStream = null;
            _receivedData = null;
            _receiveBuffer = null;
            socket = null;
        }
    } // end of TCP

    public class UDP {
        public IPEndPoint endPoint;

        private int _id;

        public UDP(int _id) {
            this._id = _id;
        }

        public void Connect(IPEndPoint _endPoint) {
            endPoint = _endPoint;
        }

        public void SendData(Packet _packet) {
            LocalServer.SendUDPData(endPoint, _packet);
        }

        public void HandleData(Packet _packetData) {
            int _packetLength = _packetData.ReadInt();
            byte[] _packetBytes = _packetData.ReadBytes(_packetLength);

            ThreadManager.ExecuteOnMainThread(() => {
                using (Packet _packet = new Packet(_packetBytes)) {
                    int _packetId = _packet.ReadInt();
                    LocalServer.packetHandlers[_packetId](_id, _packet);
                }
            });
        }


        /// <summary>
        /// Properly handling client disconnections.
        /// </summary>
        public void Disconnect() {
            endPoint = null;
        }
    } // end of UDP

    /// <summary>Sending the player into the game. Combined with player movement.</summary>
    public void SendIntoGame(string _playerName) {
        Debug.Log("Instantiating Player from Server // REMOVE");
        player = LocalServerManager.Instance.InstantiatePlayer();
        player.Initialize(id, _playerName);

        // send data from connected clients to current player
        foreach (ServerClientConnector _client in LocalServer.clients.Values) {
            if (_client.player != null) {
                if (_client.id != id) {
                    Debug.Log($"ClientConnectionConsolidator::SendIntoGame(): Sending spawning information for player with id {_client.id} to client with id {id}.");
                    LocalServerSend.SpawnPlayer(id, _client.player);
                }
            }
        }

        // send new player informations to other players (and himself)
        foreach (ServerClientConnector _client in LocalServer.clients.Values) {
            if (_client.player != null) {
                Debug.Log($"ClientConnectionConsolidator::SendIntoGame(): Sending spawning information for new player with id {player.GetPlayerID()} to player with id {_client.id}.");
                LocalServerSend.SpawnPlayer(_client.id, player);
            }
        }

        // Sending "MasterClient" information to everyone!
        // Might create a player controller abstraction to use...
        /*
        if (LocalServerManager.Instance != null && LocalServerManager.Instance.masterClient != null) {
            PlayerController _masterClient = LocalServerManager.Instance.masterClient;
            if (_masterClient != null && _masterClient.player != null) {
                foreach (ServerClientConnector _client in LocalServerSend.clients.Values) {
                    if (_client.player != null && _client.id != _masterClient.player.GetPlayerID()) {
                        Debug.Log($"ClientConnectionConsolidator::SendIntoGame(): Sending spawning information for new player with id {_masterClient.player.GetPlayerID()} to player with id {_client.id}.");
                        LocalServerSend.SpawnPlayer(_client.id, _masterClient.player.GetPlayerID(), _masterClient.player.GetUsername(), _masterClient.gameObject.transform.position, _masterClient.gameObject.transform.rotation);
                    }
                }
            }
        }
        */
    }

    /// <summary>Properly handling client disconnections.</summary>
    public void Disconnect() {
        Debug.Log($"ClientConnectionConsolidator::Disconnect(): {tcp.socket.Client.RemoteEndPoint} has disconnected.");

        // Attempting to destroy object from other than main thread will not work.
        ThreadManager.ExecuteOnMainThread(() => {
            // Had a problem while moving project. Player was null once. Should not happen normally.
            if (player == null) {
                Debug.LogError("ClientConnectionConsolidator::Disconnect(): ThreadManager wanted to destroy player, but is already null...! THIS SHOULD NOT HAPPEN!");
                return;
            }
            UnityEngine.Object.Destroy(player.gameObject);
            player = null;
        });

        tcp.Disconnect();
        udp.Disconnect();
    }
}