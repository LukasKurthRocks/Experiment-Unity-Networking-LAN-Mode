using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class LocalServerManager : Singleton<LocalServerManager> {
    [Header("Preferences")]
    // Prefabs
    [SerializeField]
    private GameObject _playerPrefab = null;
    [SerializeField]
    private GameObject _masterClientPrefab = null;
    // Other Stuff
    [SerializeField]
    private bool _isStarted = false;

    void Start() {
        if (_playerPrefab == null)
            Debug.LogWarning("LocalServerManager::Start(): _playerPrefab is null.");
        if (_masterClientPrefab == null)
            Debug.LogWarning("LocalServerManager::Start(): _masterClientPrefab is null.");

        // Create Server in batch mode / headless mode
        // Call StartServer() from "Start Host" button when not!
        if (IsHeadlessMode()) {
            Debug.Log("LocalServerManager::Start(): detected headless mode. Calling StartServer()...");
            StartServer();
        } else {
            Debug.Log("LocalServerManager::Start(): headless mode not detected. Start server with 'Start Host' button.");
        }
    }

    /// <summary>Closing unity's still open sockets...</summary>
    private void OnApplicationQuit() {
        // just to make sure it only stops if it has been started
        // either via button "Start Host" or when in headless/batch mode.
        if (IsServerStarted()) {
            StopServer();
        }
    }

    #region PlayerLogic
    public Player InstantiatePlayer() {
        Player _thatPlayer;
        try {
            _thatPlayer = Instantiate(_playerPrefab, new Vector3(0F, .5F, 0F), Quaternion.identity).GetComponent<Player>();
            _thatPlayer.SetUsername("PlayerClone_from_LSS");
        } catch (Exception _exception) {
            Debug.LogError($"LocalServerManager::InstantiatePlayer(): Error on instantiating player: {_exception}");
            return null;
        }
        return _thatPlayer;
    }

    // TODO: public void SpawnMasterClient() {}
    #endregion

    public void StartServer() {
        if (_isStarted) {
            Debug.LogError("LocalServerManager::StartServer(): Server already started. Returning from function. TIPP: Call IsServerStarted() if needed.");
            return;
        }

        // Lower server CPU usage.
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 30;

        _isStarted = true;

        /*
         * Also quick note here: Needed to check if there is a way I can call the SendPing() via the LocalServer class.
         * Unfortunately this is not possible, as I would have to have an existing client connection.
         * So i might have to create the ping socket on another port ...
         */
        //LocalPingServer.Start(NetworkingConstants.STD_MAX_PLAYERS, NetworkingConstants.STD_SERVER_PORT);
        //LocalPingServer.Start(NetworkingConstants.STD_SERVER_PORT);
        
        // Test: Start "PingServer" AND "GameServer"
        LocalPingServer.Start(NetworkingConstants.PING_SERVER_PORT);
        LocalServer.Start(2, NetworkingConstants.STD_SERVER_PORT);
    }

    public void StopServer() {
        _isStarted = false;

        // Test: Stopping "PingServer" AND "GameServer"
        LocalPingServer.Stop();
        LocalServer.Stop();
    }

    /// <summary>Checking server for headless mode</summary>
    public bool IsHeadlessMode() {
        return SystemInfo.graphicsDeviceType == GraphicsDeviceType.Null;
    }

    /// <summary>Checking server for headless mode</summary>
    public bool IsHeadlessModeViaDeviceID() {
        return SystemInfo.graphicsDeviceID == 0;
    }

    public bool IsServerStarted() {
        return _isStarted;
    }
}