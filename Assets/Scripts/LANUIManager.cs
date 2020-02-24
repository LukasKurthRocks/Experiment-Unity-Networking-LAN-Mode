using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LANUIManager : MonoBehaviour {
    [Header("Input Fields")]
    public InputField _port;
    [SerializeField]
    private Button _startServerButton = null;
    [SerializeField]
    private Button _stopServerButton = null;
    [SerializeField]
    private Button _startClientButton = null;
    [SerializeField]
    private Button _stopClientButton = null;
    [SerializeField]
    private Button _startPingButton = null;
    [SerializeField]
    private GameObject _percentSlider = null;

    private bool _isLoading = false;

    private ClientLocalConnectionHelper LANHelperInstance = null;

    // Start is called before the first frame update
    void Start() {
        _port.text = NetworkingConstants.STD_SERVER_PORT.ToString();
        _port.interactable = false;

        //LanManager.Instance.ScanHost();
        //ClientLANHelper.Instance.ScanHost();
        LANHelperInstance = ClientLocalConnectionHelper.Instance;

        //_port.interactable = true;

        _startServerButton.interactable = true;
        _stopServerButton.interactable = false;
        _startClientButton.interactable = true;
        _stopClientButton.interactable = false;

        //_startPingButton.interactable = false;

        _percentSlider.SetActive(false);
        _percentSlider.GetComponent<Slider>().value = 0;
    }

    void Update() {
        if (_isLoading) {
            float progress = LANHelperInstance._percentSearching;
            _percentSlider.GetComponent<Slider>().value = progress;

            if (!LANHelperInstance._isSearching) {
                _startPingButton.interactable = true;
                _percentSlider.SetActive(false);
                _isLoading = false;

                LANHelperInstance.CloseClient();
            }
        }
    }

    private void OnApplicationQuit() {
        // TODO: Move where it belongs
        LANHelperInstance.CloseClient();
        LANHelperInstance.CloseServer();
    }

    public void StartServer() {
        Debug.Log("Start Server");
        // LanManager.Instance.StartServer(NetworkingConstants.STD_SERVER_PORT);
        //NetworkManager.Instance.DoStart();
        NewLANManager.Instance.StartServer();
        _startServerButton.interactable = false;
        _stopServerButton.interactable = true;
        _startClientButton.interactable = false;
    }
    public void StopServer() {
        //LanManager.Instance.CloseServer();
        //NetworkManager.Instance.CloseServer();
        NewLANManager.Instance.StopServer();
        _startServerButton.interactable = true;
        _stopServerButton.interactable = false;
        _startClientButton.interactable = true;
    }
    public void StartClient() {
        // NetworkingConstants.STD_SERVER_PORT
        //LanManager.Instance.StartClient(55555);
        ClientConnector.Instance.CreateShortInstance(55555);

        _startClientButton.interactable = false;
        _stopClientButton.interactable = true;
        _startServerButton.interactable = false;

        _startPingButton.interactable = true;
    }
    public void StopClient() {
        //LanManager.Instance.CloseClient();
        ClientConnector.Instance.Disconnect();

        _startClientButton.interactable = true;
        _stopClientButton.interactable = false;
        _startServerButton.interactable = true;

        _startPingButton.interactable = false;
    }

    public void StartPing() {
        Debug.Log("Ping(): has been started...");

        _startPingButton.interactable = false;
        _percentSlider.SetActive(true);
        _isLoading = true;

        // TODO: Deactivate all controls? Not messing with LAN scripting in here.
        LANHelperInstance.StartClient(55555);
        LANHelperInstance.ScanHost();


        // ATTENTION: Coroutines tend to block the main thread.
        Debug.Log("About to start coroutine SendPing with port " + NetworkingConstants.STD_SERVER_PORT);
        StartCoroutine(LANHelperInstance.SendPing(NetworkingConstants.STD_SERVER_PORT, allowLocalAddress: true));

        // TODO: Create a list or something...
        //PrintResults();

        /*
        Debug.Log("Starting ping coroutine ...");
        //StartCoroutine(LanManager.Instance.SendPing(NetworkingConstants.STD_SERVER_PORT));
        StartCoroutine(ClientLANHelper.Instance.SendPing(NetworkingConstants.STD_SERVER_PORT));

        _startPingButton.interactable = false;
        _percentSlider.SetActive(true);
        _isLoading = true;
        */
    }

    public void PrintResults() {
        if (LANHelperInstance._addresses.Count > 0) {
            foreach (string address in LANHelperInstance._addresses) {
                Debug.Log($"Result::FoundAddress: {address}");
            }
        } else {
            Debug.LogWarning("Result::NoAddressFound");
        }
        /*
        if (LanManager.Instance._addresses.Count > 0) {
            foreach (string address in LanManager.Instance._addresses) {
                Debug.Log($"Result::FoundAddress: {address}");
            }
        } else {
            Debug.LogWarning("Result::NoAddressFound");
        }
        */
    }
}