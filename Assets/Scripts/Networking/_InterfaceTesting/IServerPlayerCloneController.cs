using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IServerPlayerCloneControllers {
    int playerID { get; set; }

    string playerName { get; set; }

    void Initialize(int _playerID, string _playerName);

    /// <summary>Do what you have to do to set those inputs and calculate player movement...</summary>
    void SetPlayerInput(bool[] inputs, Quaternion rotation);

    void MovePlayer(Vector2 _inputDirection);
}