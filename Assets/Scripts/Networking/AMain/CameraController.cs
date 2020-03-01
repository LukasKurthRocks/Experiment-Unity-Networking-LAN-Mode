using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// TODO: Adjust this!
// TODO: Add controller to LocalPlayerPrefabs Camera component

public class CameraController : MonoBehaviour {
    public Player player;
    public float sensitivity = 100F;
    public float clampAngle = 85F;

    private float verticalRotation;
    private float horizontalRotation;

    void Start() {
        if (player == null)
            Debug.LogWarning("CameraController::Start(): player is null");

        verticalRotation = transform.localEulerAngles.x;
        horizontalRotation = player.transform.eulerAngles.y;
    }

    void Update() {
        Look();
        Debug.DrawRay(transform.position, transform.forward * 2, Color.red);
    }

    private void Look() {
        float _mouseVertical = -Input.GetAxis("Mouse Y");
        float _mouseHorizontal = Input.GetAxis("Mouse X");

        verticalRotation += _mouseVertical * sensitivity * Time.deltaTime;
        horizontalRotation += _mouseHorizontal * sensitivity * Time.deltaTime;

        verticalRotation = Mathf.Clamp(verticalRotation, -clampAngle, clampAngle);

        transform.localRotation = Quaternion.Euler(verticalRotation, 0F, 0F);
        player.transform.rotation = Quaternion.Euler(0F, horizontalRotation, 0F);
    }
}