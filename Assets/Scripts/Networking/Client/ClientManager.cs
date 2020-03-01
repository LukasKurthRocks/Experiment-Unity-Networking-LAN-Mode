using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClientManager : Singleton<ClientManager> {
    // storing player info on client side (do not open on server side)
    public static Dictionary<int, Player> players = new Dictionary<int, Player>();

    [Header("Player Prefabs")]
    [SerializeField]
    private GameObject _playerPrefab = null;
    [SerializeField]
    private GameObject _playerClonePrefab = null;
    [SerializeField]
    private GameObject _masterClientPrefab = null;

    /// <summary>Instantiate playerprefab and instatiate it's values on the PlayerManager.</summary>
    public void SpawnPlayer(int _id, string _username, Vector3 _position, Quaternion _rotation) {
        GameObject _player;

        // is player = localPlayer?
        if (_id == ClientConnector.Instance.myId) {
            _player = Instantiate(_playerPrefab, _position, _rotation);
        } else {
            _player = Instantiate(_playerClonePrefab, _position, _rotation);
        }

        _player.GetComponent<Player>().SetPlayerID(_id);
        _player.GetComponent<Player>().SetUsername(_username);

        Debug.Log($"ClientManager::SpawnPlayer(): Adding player with id {_id} to _player array.");
        players.Add(_id, _player.GetComponent<Player>());
    }

    public void SpawnMasterClient(int _id, string _username, Vector3 _position, Quaternion _rotation) {
        GameObject _masterClient = Instantiate(_masterClientPrefab, _position, _rotation);

        _masterClient.GetComponent<Player>().SetPlayerID(_id);
        _masterClient.GetComponent<Player>().SetUsername(_username);

        Debug.Log($"ClientManager::SpawnMasterClient(): Adding player with id {_id} to _player array.");
        players.Add(_id, _masterClient.GetComponent<Player>());
    }
}
