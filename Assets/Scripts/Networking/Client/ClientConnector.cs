using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

public class ClientConnector : Singleton<ClientConnector> {
    public static int dataBufferSize = 4096;

    public string ip = NetworkingConstants.STD_SERVER_IP;
    public int port = NetworkingConstants.STD_SERVER_PORT;

    public int myId = 0;
    public TCP tcp;
    public UDP udp;

    private bool _clientIsConnected = false;
    private delegate void PacketHandler(Packet _packet);
    private static Dictionary<int, PacketHandler> _packetHandlers;

    private void Start() {
        Debug.Log("ClientConnector::Start(): called...");
        tcp = new TCP();
        udp = new UDP();
    }

    // Unity Editor does not properly close open connections via PlayMode
    private void OnApplicationQuit() {
        Disconnect();
    }

    // TODO: Do not want to connect to server just yet, but i NEED the udp part for pinging....
    public void CreateShortInstance(int port) {
        Debug.Log("Creating a ping instance ...");

        InitializeClientData();

        // only in lan... hm?
        if (udp.endPoint == null)
            udp = new UDP();

        _clientIsConnected = true;
        udp.Connect(port);
    }

    public void ConnectToServer() {
        if (tcp == null)
            Debug.LogError("ClientConnector:ConnectToServer(): tcp is null...");

        InitializeClientData();

        _clientIsConnected = true;
        tcp.Connect();
    }

    public class TCP {
        public TcpClient socket;

        private NetworkStream _networkStream;
        private Packet _receivedData;
        private byte[] _receiveBuffer;

        public void Connect() {
            socket = new TcpClient {
                ReceiveBufferSize = dataBufferSize,
                SendBufferSize = dataBufferSize
            };

            _receiveBuffer = new byte[dataBufferSize];
            IAsyncResult connectResult = socket.BeginConnect(Instance.ip, Instance.port, ConnectCallback, socket);

            // Timout for the server connection
            // Might have to move this to the main thread for not having GUI Hang...
            bool success = connectResult.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(1));

            if (!success) {
                Debug.LogError($"ClientConnector::TCP::Connect(): Could not connect to server '{Instance.ip}:{Instance.port}'.");
                Disconnect();
                Instance.Disconnect();

                // TODO: Move somewhere or implement a LANSceneController or something ...
                // Call your function for main menu throwback...
                //SceneController.Instance.LoadScene(0);
            }
        }

        private void ConnectCallback(IAsyncResult _asyncResult) {
            socket.EndConnect(_asyncResult);

            if (!socket.Connected)
                return;

            _networkStream = socket.GetStream();

            _receivedData = new Packet();

            _networkStream.BeginRead(_receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);
        }

        public void SendData(Packet _packet) {
            try {
                if (socket != null) {
                    _networkStream.BeginWrite(_packet.ToArray(), 0, _packet.Length(), null, null);
                }
            } catch (Exception _exception) {
                Debug.Log($"ClientConnector::TCP::SendData(): Error sending data to server via TCP: {_exception}");
            }
        }

        private void ReceiveCallback(IAsyncResult _asyncResult) {
            try {
                int _byteLength = _networkStream.EndRead(_asyncResult);
                if (_byteLength <= 0) {
                    // Disconnecting client/player
                    Instance.Disconnect();
                    return;
                }

                byte[] _data = new byte[_byteLength];
                Array.Copy(_receiveBuffer, _data, _byteLength);

                // Handle data
                _receivedData.Reset(HandleData(_data));
                _networkStream.BeginRead(_receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);
            } catch {
                // Disconnecting client/player
                Disconnect();
            }
        }

        private bool HandleData(byte[] _data) {
            int _packetLength = 0;

            _receivedData.SetBytes(_data);

            if (_receivedData.UnreadLength() >= 4) {
                _packetLength = _receivedData.ReadInt();
                if (_packetLength <= 0) {
                    return true;
                }
            }

            while (_packetLength > 0 && _packetLength <= _receivedData.UnreadLength()) {
                byte[] _packetBytes = _receivedData.ReadBytes(_packetLength);
                ThreadManager.ExecuteOnMainThread(() => {
                    using (Packet _packet = new Packet(_packetBytes)) {
                        int _packetId = _packet.ReadInt();
                        _packetHandlers[_packetId](_packet);
                    }
                });

                _packetLength = 0;

                if (_receivedData.UnreadLength() >= 4) {
                    _packetLength = _receivedData.ReadInt();
                    if (_packetLength <= 0) {
                        return true;
                    }
                }
            }

            if (_packetLength <= 1) {
                return true;
            }

            return false;
        }

        private void Disconnect() {
            Instance.Disconnect();

            _networkStream = null;
            _receivedData = null;
            _receiveBuffer = null;
            socket = null;
        }
    }

    public class UDP {
        public UdpClient socket;
        public IPEndPoint endPoint;

        public UDP() {
            endPoint = new IPEndPoint(IPAddress.Parse(Instance.ip), Instance.port);
        }

        public void Connect(int _localPort) {
            socket = new UdpClient(_localPort);

            if (endPoint == null)
                Debug.LogWarning("ClientMain::UDP::Connect(): endPoint is null");

            socket.Connect(endPoint);
            socket.BeginReceive(ReceiveCallback, null);

            // Initiate connexion to server
            using (Packet _packet = new Packet()) {
                SendData(_packet);
            }
        }

        public void SendData(Packet _packet) {
            try {
                _packet.InsertInt(Instance.myId);
                if (socket != null) {
                    socket.BeginSend(_packet.ToArray(), _packet.Length(), null, null);
                }
            } catch (Exception _exception) {
                Debug.Log($"ClientMain::UDP::SendData(): Error sending data to server via UDP: {_exception}");
            }
        }
        
        // TODO: Test if this can work in LAN or remove this.
        public void SendDataWithoutID(Packet _packet) {
            try {
                if (socket != null) {
                    socket.BeginSend(_packet.ToArray(), _packet.Length(), null, null);
                } else {
                    Debug.Log("SendDataWithoutID: Socket is null...");
                }
            } catch (Exception _exception) {
                Debug.Log($"ClientMain::UDP::SendData(): Error sending data to server via UDP: {_exception}");
            }
        }

        private void ReceiveCallback(IAsyncResult _asyncResult) {
            try {
                if (endPoint == null)
                    Debug.LogError("ClientMain::UDP::ReceiveCallback(): endPoint is null");
                if (socket == null)
                    Debug.LogError("ClientMain::UDP::ReceiveCallback(): socket is null");

                byte[] _data = socket.EndReceive(_asyncResult, ref endPoint);
                socket.BeginReceive(ReceiveCallback, null);

                if (_data == null)
                    Debug.LogWarning("_data in clientconnector is null");
                Debug.Log("_data.Length = " + _data.Length);

                // LESS THAN 4 STUPID MEE!!
                // int = 4, no more data...
                if (_data.Length < 4) {
                    // Disconnecting client
                    Instance.Disconnect();
                    return;
                }

                // MS DOCS!!
                HandleData(_data);
            } catch (Exception _exception) {
                Debug.LogError($"ClientMain::UDP::ReceiveCallback(): Error handling receiveCallback: {_exception}");

                // Disconnecting client
                Disconnect();
            }
        }

        private void HandleData(byte[] _data) {
            Debug.Log("ClientConnector::UDP::HandleData(): Handling received data.");
            using (Packet _packet = new Packet(_data)) {
                int _packetLength = _packet.ReadInt();
                _data = _packet.ReadBytes(_packetLength);

                Debug.Log($"ClientConnector::UDP::HandleData(): Length: {_packetLength}, DataInfo: {_data.Length}");
            }

            ThreadManager.ExecuteOnMainThread(() => {
                using (Packet _packet = new Packet(_data)) {
                    int _packetId = _packet.ReadInt();
                    Debug.Log($"ClientConnector::UDP::HandleData(): Calling packet handler with id {_packetId}");
                    _packetHandlers[_packetId](_packet);
                }
            });
        }

        private void Disconnect() {
            Instance.Disconnect();

            endPoint = null;
            socket = null;
        }
    }

    private void InitializeClientData() {
        _packetHandlers = new Dictionary<int, PacketHandler>() {
            {(int)ServerPackets.pong, ClientReceive.Pong },
            {(int)ServerPackets.welcome, ClientReceive.Welcome },
            {(int)ServerPackets.spawnPlayer, ClientReceive.SpawnPlayer },
            {(int)ServerPackets.playerPosition, ClientReceive.PlayerPosition },
            {(int)ServerPackets.playerRotation, ClientReceive.PlayerRotation }
        };

        Debug.Log("ClientConnector::InitializeClientData(): Initialized packets.");
    }

    public void Disconnect() {
        if (_clientIsConnected) {
            _clientIsConnected = false;

            if (tcp.socket != null)
                tcp.socket.Close();
            if (udp.socket != null)
                udp.socket.Close();

            Debug.Log("ClientConnector::Disconnect(): Disconnected from server.");
        }
    }
}