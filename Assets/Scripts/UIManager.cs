using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : Singleton<UIManager> {
    [Header("Input Fields")]
    [SerializeField]
    private InputField _port;
    [SerializeField]
    private InputField _username; // TODO: Still have to save this somewhere

    // For controlling the button state
    [SerializeField]
    private Button _localServerSearchButton;
    [SerializeField]
    private Button _loginConnectButton;
    [SerializeField]
    private Button _disconnectButton;
    [SerializeField]
    private Button _quitButton;
    [SerializeField]
    private Button _startPingServer; // TODO: Maybe just the headless version?
    [SerializeField]
    private Button _startHostServer;
    [SerializeField]
    private Button _stopPingServer; // TODO: Maybe just the headless version?
    [SerializeField]
    private Button _stopHostServer;
    [SerializeField]
    private Dropdown _serverListDropdown;
    [SerializeField]
    private Slider _percentSlider;
    [SerializeField]
    private GameObject _menuPanel;
    [SerializeField]
    private GameObject _statusPanel;

    private bool _isLoading = false;

    // Creating an instance of the LANHelper
    private ClientLANHelper LANHelperInstance = null;

    void Start() {
        LANHelperInstance = ClientLANHelper.Instance;
        if (LANHelperInstance == null) {
            Debug.LogError("UIManager::Start(): Need lan server instance to search for local server.");
            _localServerSearchButton.interactable = false;
        }

        if (_percentSlider == null)
            Debug.LogWarning("UIManager::Start(): _percentSlider not assigned.");
        if (_username == null)
            Debug.LogWarning("UIManager::Start(): _username field not assigned.");
        if (_port == null)
            Debug.LogWarning("UIManager::Start(): _port field not assigned.");
        if (_localServerSearchButton == null)
            Debug.LogWarning("UIManager::Start(): _localServerSearchButton button not assigned.");
        if (_loginConnectButton == null)
            Debug.LogWarning("UIManager::Start(): _loginConnectButton button not assigned.");
        if (_disconnectButton == null)
            Debug.LogWarning("UIManager::Start(): _loginConnectButton button not assigned.");
        if (_quitButton == null)
            Debug.LogWarning("UIManager::Start(): _quitButton button not assigned.");
        if (_startPingServer == null)
            Debug.LogWarning("UIManager::Start(): _startPingServer button not assigned.");
        if (_startHostServer == null)
            Debug.LogWarning("UIManager::Start(): _startHostServer button not assigned.");
        if (_stopPingServer == null)
            Debug.LogWarning("UIManager::Start(): _stopPingServer button not assigned.");
        if (_stopHostServer == null)
            Debug.LogWarning("UIManager::Start(): _stoptHostServer button not assigned.");

        if (_port != null)
            _port.text = NetworkingConstants.STD_SERVER_PORT.ToString();
        /*if (_quitButton != null)
            _quitButton.interactable = false;*/
        if (_percentSlider != null)
            _percentSlider.value = 0;
        if(_serverListDropdown != null) {
            //_serverListDropdown.options.Clear();
        }

        _stopPingServer.gameObject.SetActive(false);
        _stopHostServer.gameObject.SetActive(false);
        _disconnectButton.gameObject.SetActive(false);
    }

    private void Update() {
        if (_isLoading) {
            float progress = LANHelperInstance._percentSearching;
            _percentSlider.GetComponent<Slider>().value = progress;

            if (!LANHelperInstance._isSearching) {
                // disabling the slider
                _percentSlider.gameObject.SetActive(false);
                _isLoading = false;

                // Re-enabling menu
                DisableMenuItems(false);

                // closing the client
                LANHelperInstance.CloseClient();

                // add ips to dropdown lis
                foreach(string address in LANHelperInstance._addresses) {
                    // Todo: Port?
                    var _thisOption = new List<string> {address};
                    _serverListDropdown.AddOptions(_thisOption);
                }
                if (LANHelperInstance._addresses.Count > 0) {
                    foreach (string address in LANHelperInstance._addresses) {
                        Debug.Log($"Result::FoundAddress: {address}");
                    }
                } else {
                    Debug.LogWarning("Result::NoAddressFound");
                }
            }
        }
    }

    /// <summary>Disabling the interactive controls of the menu. There might be a better way, but this works!</summary>
    private void DisableMenuItems(bool disable = true) {
        foreach(Transform child in _menuPanel.gameObject.transform) {
            // is there a better way for this?
            if(child.gameObject.GetComponent<Button>() != null)
                child.gameObject.GetComponent<Button>().interactable = !disable;
            if (child.gameObject.GetComponent<Dropdown>() != null)
                child.gameObject.GetComponent<Dropdown>().interactable = !disable;
            if (child.gameObject.GetComponent<InputField>() != null)
                child.gameObject.GetComponent<InputField>().interactable = !disable;
        }
    }

    public void StartServer() {
        Debug.Log("UI::StartServer(): Server started.");
        LocalServerManager.Instance.StartServer();

        DisableMenuItems();
        _startHostServer.gameObject.SetActive(false);
        _stopHostServer.gameObject.SetActive(true);
        _stopHostServer.interactable = true;
    }
    public void StopServer() {
        Debug.Log("UI::StopServer(): Server stopped.");
        LocalServerManager.Instance.StopServer();

        DisableMenuItems(false);
        _startHostServer.gameObject.SetActive(true);
        _stopHostServer.gameObject.SetActive(false);
        _stopHostServer.interactable = false;
    }

    public void StartClient() {
        ClientConnector.Instance.ConnectToServer();

        DisableMenuItems();

        _loginConnectButton.gameObject.SetActive(false);
        _disconnectButton.gameObject.SetActive(true);
        _disconnectButton.interactable = true;
    }
    public void StopClient() {
        ClientConnector.Instance.Disconnect();

        DisableMenuItems(false);

        _loginConnectButton.gameObject.SetActive(true);
        _disconnectButton.gameObject.SetActive(false);
        _disconnectButton.interactable = false;
    }

    public void StartPing() {
        Debug.Log("UIManager::StartPing(): has been called ...");

        // While ping => NO CONTROL
        // This is important, because this opens a separate socket!
        DisableMenuItems();

        // Then enabling the slider and setting the bool to know if done (Update()).
        _percentSlider.gameObject.SetActive(true);
        _isLoading = true;

        // Opening LAN controls. Closing sockets in Update()
        LANHelperInstance.StartClient(55555);
        LANHelperInstance.ScanHost();

        // Watch the coroutines. While loops might block the whole system.
        Debug.Log("UIManager::StartPing(): About to start coroutine SendPing with port " + NetworkingConstants.STD_SERVER_PORT);
        StartCoroutine(LANHelperInstance.SendPing(NetworkingConstants.STD_SERVER_PORT, allowLocalAddress: true));
    }

    public void QuitGame() {
        Debug.Log("UIManager::QuitGame(): ByeBye.");
        
        #if UNITY_EDITOR
        // Application.Quit() does not work in the editor so
        // UnityEditor.EditorApplication.isPlaying need to be set to false to end the game
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit(0);
        #endif
    }

    private void OnApplicationQuit() {
        LANHelperInstance.CloseClient();
        LANHelperInstance.CloseServer();

        Debug.Log("UIManager::OnQuit(): Still need to implement this.");
    }
}