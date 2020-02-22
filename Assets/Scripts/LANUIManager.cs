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

    // Start is called before the first frame update
    void Start() {
        _port.text = NetworkingConstants.STD_SERVER_PORT.ToString();
        _port.interactable = false;

        LanManager.Instance.ScanHost();
        //_port.interactable = true;

        _startServerButton.interactable = true;
        _stopServerButton.interactable = false;
        _startClientButton.interactable = true;
        _stopClientButton.interactable = false;

        _startPingButton.interactable = false;

        _percentSlider.SetActive(false);
        _percentSlider.GetComponent<Slider>().value = 0;
    }

    void Update() {
        if (_isLoading) {
            float progress = LanManager.Instance._percentSearching;
            _percentSlider.GetComponent<Slider>().value = progress;

            if (!LanManager.Instance._isSearching) {
                _startPingButton.interactable = true;
                _percentSlider.SetActive(false);
                _isLoading = false;
            }
        }
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
        LanManager.Instance.StartClient(55555);
        //ClientConnector.Instance.CreateShortInstance(55555);

        _startClientButton.interactable = false;
        _stopClientButton.interactable = true;
        _startServerButton.interactable = false;

        _startPingButton.interactable = true;
    }
    public void StopClient() {
        LanManager.Instance.CloseClient();
        //ClientConnector.Instance.Disconnect();

        _startClientButton.interactable = true;
        _stopClientButton.interactable = false;
        _startServerButton.interactable = true;

        _startPingButton.interactable = false;
    }

    public void StartPing() {
        Debug.Log("Starting ping corountine ...");
        StartCoroutine(LanManager.Instance.SendPing(NetworkingConstants.STD_SERVER_PORT));
        //StartCoroutine(ClientLANHelper.Instance.SendPing(NetworkingConstants.STD_SERVER_PORT));

        _startPingButton.interactable = false;
        _percentSlider.SetActive(true);
        _isLoading = true;
    }

    public void PrintResults() {
        if (LanManager.Instance._addresses.Count > 0) {
            foreach (string address in LanManager.Instance._addresses) {
                Debug.Log($"Result::FoundAddress: {address}");
            }
        } else {
            Debug.LogWarning("Result::NoAddressFound");
        }
    }
}