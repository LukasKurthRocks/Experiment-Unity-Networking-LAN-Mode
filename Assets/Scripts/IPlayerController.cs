using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// TODO: Check if I can call this interface in the PlayerMovement of the localServerReceive class...
// The user whould hav to implement sending on himself, but this is a start!?

interface IPlayerController {
    /// <summary>Do what you have to do to set those inputs and calculate player movement...</summary>
    void SetPlayerInput(bool[] inputs, Quaternion rotation);
}