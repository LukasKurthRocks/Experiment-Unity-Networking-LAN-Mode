using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

// TODO: Rename when done... "LANManager"

public class NewLANManager : Singleton<NewLANManager> {
    [Header("Preferences")]
    [SerializeField]
    private bool _isStarted = false;

    // Start is called before the first frame update
    void Start() {
        // Checking for prefabs and headless mode (but not in client)
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

        //Server.Start(50, 26950);
        _isStarted = true;
        LANServer.Start(NetworkingConstants.STD_MAX_PLAYERS, NetworkingConstants.STD_SERVER_PORT);
    }

    public void StopServer() {
        _isStarted = false;
        LANServer.Stop();
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