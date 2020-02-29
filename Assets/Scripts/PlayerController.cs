using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour, IPlayerController {
    private bool[] _inputs;

    private void Start() {
        _inputs = new bool[5];
    }

    public void SetPlayerInput(bool[] _inputs, Quaternion rotation) {
        this._inputs = _inputs;

        throw new System.NotImplementedException();
    }

}