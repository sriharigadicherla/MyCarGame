using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Top-Left Minimap / Radar Controller.
/// Tracks the player's position and rotation, displays road bounds, traffic dots, and checkpoint markers.
/// </summary>
public class MinimapController : MonoBehaviour
{
    [Header("Tracking")]
    public Transform player;
    public Camera minimapCamera;
    public RectTransform playerIcon;
    public float minimapHeight = 120f;
    public float orthographicSize = 90f;

    void Start()
    {
        FindPlayerIfNull();
        SetupMinimapCamera();
    }

    private void FindPlayerIfNull()
    {
        if (player == null)
        {
            var p = GameObject.Find("SportsSedan");
            if (p != null) player = p.transform;
        }
    }

    private void SetupMinimapCamera()
    {
        if (minimapCamera == null)
        {
            GameObject camObj = new GameObject("MinimapCamera");
            camObj.transform.SetParent(transform, false);
            minimapCamera = camObj.AddComponent<Camera>();
            minimapCamera.orthographic = true;
            minimapCamera.orthographicSize = orthographicSize;
            minimapCamera.nearClipPlane = 0.5f;
            minimapCamera.farClipPlane = 300f;
            minimapCamera.clearFlags = CameraClearFlags.Color;
            minimapCamera.backgroundColor = new Color(0.08f, 0.09f, 0.11f, 1f);
            minimapCamera.cullingMask = ~0; // Render all layers
            minimapCamera.depth = 10;

            // Minimap viewport in top-left
            minimapCamera.rect = new Rect(0.02f, 0.72f, 0.20f, 0.25f);
        }
    }

    void LateUpdate()
    {
        if (player == null)
        {
            FindPlayerIfNull();
            return;
        }

        // Follow player from bird's eye view
        if (minimapCamera != null)
        {
            Vector3 camPos = player.position;
            camPos.y = minimapHeight;
            minimapCamera.transform.position = camPos;
            // North-up or car-forward orientation
            minimapCamera.transform.rotation = Quaternion.Euler(90f, player.eulerAngles.y, 0f);
        }

        // Rotate player arrow icon in UI
        if (playerIcon != null)
        {
            playerIcon.localRotation = Quaternion.Euler(0f, 0f, -player.eulerAngles.y);
        }
    }
}
