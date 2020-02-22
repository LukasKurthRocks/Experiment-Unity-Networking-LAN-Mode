using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// TODO: Make searching for LAN hosts replacing the Server_IP.
// TODO: Remove the todo from this class...

public class NetworkingConstants : MonoBehaviour {
    public const int TICKS_PER_SECOND = 30;
    public const int MS_PER_TICK = 1000 / TICKS_PER_SECOND;

    public const int STD_MAX_PLAYERS = 50;
    public const string STD_SERVER_IP = "127.0.0.1"; // Especially in a LAN.
    public const int STD_SERVER_PORT = 26950;
}