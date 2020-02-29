using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class LocalServerManager : Singleton<LocalServerManager> {
    [Header("Preferences")]
    [SerializeField]
    private bool _isStarted = false;

    // Start is called before the first frame update
    void Start() {
        if(IsHeadlessMode()) {
            // TODO: Do this.
            Debug.Log("LocalServerManager::Start(): Still nothing todo when headless (poor me). Shouldn't I check for master client prefab or starting the headless erver?");
        }
    }

    /// <summary>Closing unity's still open sockets...</summary>
    private void OnApplicationQuit() {
        // just to make sure it only stops if it has been started
        // either via button "Start Host" or when in headless/batch mode.
        if (_isStarted) {
            StopServer();
        }
    }

    #region PlayerLogic
    // InstantiatePlayer
    // Spawn "MasterClient"
    #endregion

    public void StartServer() {
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
    public static bool IsHeadlessMode() {
        return SystemInfo.graphicsDeviceType == GraphicsDeviceType.Null;
    }

    /// <summary>Checking server for headless mode</summary>
    public static bool IsHeadlessModeViaDeviceID() {
        return SystemInfo.graphicsDeviceID == 0;
    }
}