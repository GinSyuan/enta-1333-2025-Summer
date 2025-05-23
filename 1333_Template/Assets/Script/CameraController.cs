using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class CameraController : MonoBehaviour
{
    [Header("Movement Settings")]
    [Tooltip("Movement speed for keyboard and mouse drag")]
    public float panSpeed = 20f;
    [Tooltip("Limit for camera X movement")]
    public Vector2 panLimitX = new Vector2(-10f, 10f);
    [Tooltip("Limit for camera Z movement")]
    public Vector2 panLimitZ = new Vector2(-10f, 10f);

    [Header("Zoom Settings")]
    [Tooltip("Speed at which camera zooms in/out via scroll wheel")]
    public float zoomSpeed = 2f;
    [Tooltip("Minimum camera height for zoom clamp")]
    public float minZoom = 5f;
    [Tooltip("Maximum camera height for zoom clamp")]
    public float maxZoom = 20f;

    [Header("Rotation Settings")]
    [Tooltip("Speed at which camera rotates around the Y axis")]
    public float rotationSpeed = 50f;

    private Vector3 lastMousePosition;

    void Update()
    {
        Vector3 pos = transform.position;

        // Keyboard panning: WASD 
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        pos.x += h * panSpeed * Time.deltaTime;
        pos.z += v * panSpeed * Time.deltaTime;

        // Mouse drag panning: hold middle mouse button
        if (Input.GetMouseButtonDown(2))
        {
            lastMousePosition = Input.mousePosition;
        }
        if (Input.GetMouseButton(2))
        {
            Vector3 delta = Input.mousePosition - lastMousePosition;
            pos.x -= delta.x * panSpeed * Time.deltaTime;
            pos.z -= delta.y * panSpeed * Time.deltaTime;
            lastMousePosition = Input.mousePosition;
        }

        // Zooming: scroll wheel
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        pos.y -= scroll * zoomSpeed * 100f * Time.deltaTime;

        // Rotation: Q/E keys rotate around Y axis
        if (Input.GetKey(KeyCode.Q))
        {
            transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);
        }
        if (Input.GetKey(KeyCode.E))
        {
            transform.Rotate(Vector3.up, -rotationSpeed * Time.deltaTime, Space.World);
        }

        // Clamp zoom height
        pos.y = Mathf.Clamp(pos.y, minZoom, maxZoom);

        // Clamp camera panning within limits
        pos.x = Mathf.Clamp(pos.x, panLimitX.x, panLimitX.y);
        pos.z = Mathf.Clamp(pos.z, panLimitZ.x, panLimitZ.y);

        // Apply the final position
        transform.position = pos;
    }
}

