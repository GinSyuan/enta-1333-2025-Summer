using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles camera panning, zooming, and rotation based on keyboard and mouse inputs.
/// - WASD or arrow keys: pan horizontally and vertically.
/// - Middle mouse drag: pan horizontally/vertically by dragging.
/// - Mouse scroll wheel: zoom in/out.
/// - Q/E keys: rotate camera around Y axis.
/// Clamps camera within specified boundaries.
/// </summary>
public class CameraController : MonoBehaviour
{
    [Header("Movement Settings")]
    [Tooltip("Movement speed when using keyboard or dragging the mouse.")]
    public float panSpeed = 20f;
    [Tooltip("Minimum and maximum X coordinate for camera position.")]
    public Vector2 panLimitX = new Vector2(-10f, 10f);
    [Tooltip("Minimum and maximum Z coordinate for camera position.")]
    public Vector2 panLimitZ = new Vector2(-10f, 10f);

    [Header("Zoom Settings")]
    [Tooltip("Speed multiplier for zooming via scroll wheel.")]
    public float zoomSpeed = 2f;
    [Tooltip("Lowest Y position (zoomed in) camera can go.")]
    public float minZoom = 5f;
    [Tooltip("Highest Y position (zoomed out) camera can go.")]
    public float maxZoom = 20f;

    [Header("Rotation Settings")]
    [Tooltip("Speed at which the camera rotates around the Y axis.")]
    public float rotationSpeed = 50f;

    private Vector3 lastMousePosition;

    /// <summary>
    /// Called once per frame to handle input and update camera transform.
    /// </summary>
    private void Update()
    {
        Vector3 pos = transform.position;

        // Keyboard panning (WASD or arrow keys)
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        pos.x += h * panSpeed * Time.deltaTime;
        pos.z += v * panSpeed * Time.deltaTime;

        // Mouse drag panning (middle mouse button)
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

        // Zoom via scroll wheel
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        pos.y -= scroll * zoomSpeed * 100f * Time.deltaTime;

        // Rotation around Y axis using Q/E keys
        if (Input.GetKey(KeyCode.Q))
        {
            transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);
        }
        if (Input.GetKey(KeyCode.E))
        {
            transform.Rotate(Vector3.up, -rotationSpeed * Time.deltaTime, Space.World);
        }

        // Clamp zoom between minZoom and maxZoom
        pos.y = Mathf.Clamp(pos.y, minZoom, maxZoom);

        // Clamp camera panning within x/z limits
        pos.x = Mathf.Clamp(pos.x, panLimitX.x, panLimitX.y);
        pos.z = Mathf.Clamp(pos.z, panLimitZ.x, panLimitZ.y);

        transform.position = pos;
    }
}
