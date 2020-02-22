using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

// Functionality only in LAN mode ...
// TODO: Check if singleton needed ...

public class ClientLANHelper : Singleton<ClientLANHelper> {
    // Addresses of the computer (Ethernet, WiFi, etc.)
    public List<string> _localAddresses { get; private set; }
    public List<string> _localSubAddresses { get; private set; }

    // Addresses found on the network with a server launched
    public List<string> _addresses { get; private set; }

    public bool _isSearching { get; private set; }
    public float _percentSearching { get; private set; }

    // TODO: Remove from this class ...
    private Socket _socketClient;
    private EndPoint _remoteEndPoint;

    // TODO: No Instance? Check for this init. ...
    public ClientLANHelper() {
        _addresses = new List<string>();
        _localAddresses = new List<string>();
        _localSubAddresses = new List<string>();
    }

    public IEnumerator SendPing(int port) {
        Debug.Log("SendPing(): started");
        _addresses.Clear();

        if (ClientConnector.Instance.udp.endPoint == null)
            Debug.LogWarning("endpoint is null");

        if (ClientConnector.Instance.udp.endPoint != null) {
            Debug.Log("SendPing(): socket client != null");

            int maxSend = 4;
            float countMax = (maxSend * _localSubAddresses.Count) - 1;

            float index = 0;

            _isSearching = true;

            Debug.Log($"Values: {maxSend}, {countMax}, {index}, {_isSearching} ");

            // Send several pings just to be sure (a ping can be lost!)
            for (int i = 0; i < maxSend; i++) {
                Debug.Log($"Ping {i+1}/{maxSend}");

                // For each address that this device has
                foreach (string subAddress in _localSubAddresses) {
                    IPEndPoint destinationEndPoint = new IPEndPoint(IPAddress.Parse(subAddress + ".255"), port);
                    Debug.Log($"Ping {i + 1}/{maxSend} to {destinationEndPoint}");

                    byte[] str = Encoding.ASCII.GetBytes("ping");

                    // TODO: Not passing _socketClient and destinationEndPoint
                    //ClientSend.SendPing(ref _socketClient, ref destinationEndPoint);
                    ClientSend.SendPing();

                    _percentSearching = index / countMax;

                    index++;

                    yield return new WaitForSeconds(0.1f);
                }
            }
            _isSearching = false;
        }
    }

    /// <summary>Adding local ip addresses - from current host - to dictionary.</summary>
    public void ScanHost() {
        Debug.Log("LanHelper::ScanHost(): Scanning host for local addresses ...");
        IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());

        foreach (IPAddress ip in host.AddressList) {
            if (ip.AddressFamily == AddressFamily.InterNetwork) {
                string address = ip.ToString();
                string subAddress = address.Remove(address.LastIndexOf('.'));

                Debug.Log("LanHelper::ScanHost(): IP: " + address);
                _localAddresses.Add(address);

                if (!_localSubAddresses.Contains(subAddress)) {
                    _localSubAddresses.Add(subAddress);
                }
            }
        }
    }

    public void AddAddress(string _address) {
        // This is not ourself and we do not already have this address
        if (!_localAddresses.Contains(_address) && !_addresses.Contains(_address)) {
            _addresses.Add(_address);
        }

        // Just in case someone asking why local server not found.
        if (_localAddresses.Contains(_address))
            Debug.LogWarning("Server is local: " + _address);
    }
}