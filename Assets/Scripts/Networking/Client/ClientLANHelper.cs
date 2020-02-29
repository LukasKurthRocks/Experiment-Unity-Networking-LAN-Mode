using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;

// https://stackoverflow.com/questions/37951902/how-to-get-ip-addresses-of-all-devices-in-local-network-with-unity-unet-in-c
// https://forum.unity.com/threads/c-detecting-connected-devices-through-lan.297115/

public class ClientLANHelper : Singleton<ClientLANHelper> {
    // BufferSize for handling packets
    public static int dataBufferSize = 4096;

    // Addresses of the computer (Ethernet, WiFi, etc.)
    public List<string> _localAddresses { get; private set; }
    public List<string> _localSubAddresses { get; private set; }

    // Addresses found on the network with a server launched
    public List<string> _addresses { get; private set; }

    public bool _isSearching { get; private set; }
    public float _percentSearching { get; private set; }

    private Socket _socketServer;
    private Socket _socketClient;

    private EndPoint _remoteEndPoint;

    // In local I might be my own server.
    private bool _suppressLocalAddress = false;

    // Implementing Packets-Handling
    private static Packet _receivedData; // Handling data
    private static byte[] _receiveBuffer;

    public ClientLANHelper() {
        _addresses = new List<string>();
        _localAddresses = new List<string>();
        _localSubAddresses = new List<string>();

        // Handling data
        _receivedData = new Packet();
        _receiveBuffer = new byte[dataBufferSize];
    }

    #region Starting/Stopping server
    public void StartServer(int port) {
        if (_socketServer == null && _socketClient == null) {
            try {
                _socketServer = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

                if (_socketServer == null) {
                    Debug.LogWarning("SocketServer creation failed");
                    return;
                }

                // Check if we received pings
                _socketServer.Bind(new IPEndPoint(IPAddress.Any, port));

                _remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);

                _socketServer.BeginReceiveFrom(new byte[1024], 0, 1024, SocketFlags.None, ref _remoteEndPoint, new AsyncCallback(_ReceiveServer), null);
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
    #endregion

    #region Starting/Stopping client
    public void StartClient(int port) {
        if (_socketServer == null && _socketClient == null) {
            try {
                _socketClient = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

                if (_socketClient == null) {
                    Debug.LogWarning("ClientLANHelper::StartClient(): SocketClient creation failed");
                    return;
                }

                // Check if we received response from a remote (server)
                _socketClient.Bind(new IPEndPoint(IPAddress.Any, port));

                _socketClient.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, 1);
                _socketClient.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.DontRoute, 1);

                _remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);

                _socketClient.BeginReceiveFrom(_receiveBuffer, 0, 1024, SocketFlags.None, ref _remoteEndPoint, new AsyncCallback(ReceiveClient), null);
            } catch (Exception ex) {
                Debug.Log(ex.Message);
            }
        }
        Debug.Log($"ClientLANHelper::StartClient(): Started client on port {port}");
    }

    public void CloseClient() {
        if (_socketClient != null) {
            _socketClient.Close();
            _socketClient = null;
        }

        Debug.Log("ClientLANHelper::CloseClient(): Closed socket on client.");
    }
    #endregion

    public IEnumerator SendPing(int port, bool allowLocalAddress = false) {
        _suppressLocalAddress = allowLocalAddress;

        Debug.Log("ClientLANHelper::SendPing(): Started coroutine");

        _addresses.Clear();

        if (_socketClient != null) {
            Debug.Log("socket != null");

            int maxSend = 4;
            float countMax = (maxSend * _localSubAddresses.Count) - 1;

            float index = 0;

            _isSearching = true;

            // Send several pings just to be sure (a ping can be lost!)
            for (int i = 0; i < maxSend; i++) {
                Debug.Log($"Ping {i + 1}/{maxSend}");

                // For each address that this device has
                foreach (string subAddress in _localSubAddresses) {
                    IPEndPoint destinationEndPoint = new IPEndPoint(IPAddress.Parse(subAddress + ".255"), port);
                    byte[] str = Encoding.ASCII.GetBytes("ping");

                    /*
                     * Like in server packets I COULD set the clientID to 0, but the _packetID
                     * would be send first and would need to be asked for first.
                     * I just can not change this for now without changing a WHOLE LOT of code.
                     * Maybe when Toms series has ended i will create my own code...
                     */
                    // Have to create a normal packet here as the udp send needs
                    // the UDP already set. Not messing around with that yet.
                    using (Packet _packet = new Packet((int)ClientPackets.ping)) {
                        // Write a message to the server. Is not needed though.
                        _packet.Write("ping");

                        _packet.WriteLength();
                        _socketClient.SendTo(_packet.ToArray(), _packet.Length(), SocketFlags.None, destinationEndPoint);
                    }

                    _percentSearching = index / countMax;

                    index++;

                    yield return new WaitForSeconds(0.1f);
                }
            }
            _isSearching = false;
        } else {
            Debug.LogError("socket == null");
        }
    }

    #region ReceiveServer
    private void _ReceiveServer(IAsyncResult ar) {
        if (_socketServer != null) {
            try {
                int size = _socketServer.EndReceiveFrom(ar, ref _remoteEndPoint);
                byte[] str = Encoding.ASCII.GetBytes("pong");

                // Send a pong to the remote (client)
                _socketServer.SendTo(str, _remoteEndPoint);

                _socketServer.BeginReceiveFrom(new byte[1024], 0, 1024, SocketFlags.None, ref _remoteEndPoint, new AsyncCallback(_ReceiveServer), null);
            } catch (Exception ex) {
                Debug.Log(ex.ToString());
            }
        }
    }
    #endregion

    private void ReceiveClient(IAsyncResult ar) {
        if (_socketClient != null) {
            try {
                //
                // Starting Packet Handling
                //
                int size = _socketClient.EndReceiveFrom(ar, ref _remoteEndPoint);

                if (size <= 0) {
                    // There should not be an empty packet.
                    Debug.Log("ClientLANHelper::ReceiveClient(): Client received empty packet. Closing connection...");
                    CloseClient();
                    return;
                }

                // Having data, copying bytes into array
                byte[] _data = new byte[size];
                Array.Copy(sourceArray: _receiveBuffer, destinationArray: _data, length: size);

                // int = 4, no more data...
                if (_data.Length < 4) {
                    Debug.Log("_data.Length < 4");
                    CloseClient();
                    return;
                }

                int _packetLength = 0;

                // Setting packet data
                _receivedData.SetBytes(_data);

                // int has 4 bytes
                if (_receivedData.UnreadLength() >= 4) {
                    Debug.Log("ClientLANHelper::ReceiveClient(): UnreadLength() >= 4");
                    _packetLength = _receivedData.ReadInt();
                    Debug.Log($"ClientLANHelper::ReceiveClient(): _packageLength == {_packetLength}");
                    if (_packetLength <= 0) {
                        _receivedData.Reset();
                        //return;
                    }
                }

                // as long as we get data...
                while (_packetLength > 0 && _packetLength <= _receivedData.UnreadLength()) {
                    Debug.Log("ClientLANHelper::ReceiveClient(): While having data ...");
                    byte[] _packetBytes = _receivedData.ReadBytes(_packetLength);

                    // Here is where the server packet is getting unwrapped.
                    // Normally handled by a "ServerHandle" class, but this is just ping/pong.
                    using (Packet _packet = new Packet(_packetBytes)) {
                        // has to be in the same order as the client is sending it.
                        int _packetId = _packet.ReadInt();
                        string _packetMessage = _packet.ReadString();

                        Debug.Log("" + _packetId + " msg: " + _packetMessage);
                    }
                    Debug.Log("ClientLANHelper::ReceiveClient(): After packet.");

                    _packetLength = 0;

                    // int has 4 bytes
                    if (_receivedData.UnreadLength() >= 4) {
                        Debug.Log("_receivedData.UnreadLength() >= 4: " + _receivedData.UnreadLength());
                        _packetLength = _receivedData.ReadInt();
                        if (_packetLength <= 0) {
                            Debug.Log("_packetLength <= 0: " + _packetLength);
                            _receivedData.Reset();
                        }
                    }
                }
                // return the received data
                if (_packetLength <= 1) {
                    Debug.Log("_packetLength <= 1: " + _packetLength);
                    _receivedData.Reset();
                }
                //
                // Ending Packet Handling
                //

                string address = _remoteEndPoint.ToString().Split(':')[0];

                // This is not ourself and we do not already have this address
                if (!_localAddresses.Contains(address) && !_addresses.Contains(address)) {
                    _addresses.Add(address);
                }

                // Might add the local server, especially for testing.
                if (_localAddresses.Contains(address) && !_addresses.Contains(address) && _suppressLocalAddress) {
                    Debug.LogWarning("Adding local server with ip: " + address);
                    _addresses.Add(address);
                } else if (_localAddresses.Contains(address)) {
                    Debug.LogWarning("Server is local: " + address);
                }

                _socketClient.BeginReceiveFrom(new byte[1024], 0, 1024, SocketFlags.None, ref _remoteEndPoint, new AsyncCallback(ReceiveClient), null);
            } catch (Exception ex) {
                Debug.Log(ex.ToString());
            }
        }
    }

    /// <summary>Adding local ip addresses - from current host - to dictionary.</summary>
    public void ScanHost() {
        Debug.Log("ClientLANHelper::ScanHost(): Scanning host for local addresses ...");
        IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());

        foreach (IPAddress ip in host.AddressList) {
            if (ip.AddressFamily == AddressFamily.InterNetwork) {
                string address = ip.ToString();
                string subAddress = address.Remove(address.LastIndexOf('.'));

                Debug.Log("ClientLANHelper::ScanHost(): IP: " + address);
                _localAddresses.Add(address);

                if (!_localSubAddresses.Contains(subAddress)) {
                    _localSubAddresses.Add(subAddress);
                }
            }
        }
    }
}