using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Stored player information. Accessed e.g. by LocalServerSend and LocalServerManager-
/// </summary>

public class Player : MonoBehaviour {
    public enum PlayerType { Player, Clone, MasterClient }

    [Header("Prefrences")]
    [SerializeField]
    private int _playerId = 0;
    [SerializeField]
    private string _username = null;
    [SerializeField]
    private PlayerType _playerType = PlayerType.Clone;

    private void Start() {
        // Debug.Log($"Player::Start(): PlayerType is set to '{_playerType.ToString()}'.");
        /*
        if (_playerType == PlayerType.MasterClient)
            ServerManager.Instance.masterClient = this;
        */
    }

    public void Initialize(int _playerId, string _username) {
        this._playerId = _playerId;
        this._username = _username;
    }

    #region Getters und Setters
    public int GetPlayerID() => _playerId;
    public string GetUsername() => _username;
    public PlayerType GetPlayerType() => _playerType;

    public void SetPlayerID(int _playerId) {
        this._playerId = _playerId;
    }
    public void SetUsername(string _username) {
        this._username = _username;
    }
    #endregion
}