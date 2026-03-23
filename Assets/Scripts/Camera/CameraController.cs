using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Camera))]
public class CameraController : MonoBehaviour
{
    [HideInInspector] private Camera playerCamera = null;

    [Header("GameObject References")]
    public Transform target = null;

    public enum CameraStyles { Locked, Overhead, Free };
    public CameraStyles cameraMovementStyle = CameraStyles.Locked;
    [Range(0, 0.75f)] public float freeCameraMouseTracking = 0.5f;
    public float maxDistanceFromTarget = 5.0f;
    public float cameraZCoordinate = -10.0f;

    [Header("Input Actions & Controls")]
    public InputAction lookAction;

    [Header("Limites de Camera")]
    public bool usarLimites = true;
    public Vector2 minBounds = new Vector2(-22f, -16f);
    public Vector2 maxBounds = new Vector2(15f, 5f);
    public bool detectarLimitesAutomaticamente = false;

    void OnEnable() { lookAction.Enable(); }
    void OnDisable() { lookAction.Disable(); }

    void Start()
    {
        playerCamera = GetComponent<Camera>();
        if (detectarLimitesAutomaticamente) CalcularLimitesDosBoundaries();
    }

    void Update() { SetCameraPosition(); }

    private void CalcularLimitesDosBoundaries()
    {
        GameObject[] boundaries = GameObject.FindGameObjectsWithTag("Boundary");
        if (boundaries.Length == 0)
        {
            Debug.LogWarning("CameraController: nenhuma boundary com tag 'Boundary' foi encontrada.");
            return;
        }

        float minX = float.MaxValue;
        float maxX = float.MinValue;
        float minY = float.MaxValue;
        float maxY = float.MinValue;
        bool encontrouCollider = false;

        foreach (GameObject boundary in boundaries)
        {
            Collider2D[] colliders = boundary.GetComponentsInChildren<Collider2D>(true);
            foreach (Collider2D collider in colliders)
            {
                minX = Mathf.Min(minX, collider.bounds.min.x);
                maxX = Mathf.Max(maxX, collider.bounds.max.x);
                minY = Mathf.Min(minY, collider.bounds.min.y);
                maxY = Mathf.Max(maxY, collider.bounds.max.y);
                encontrouCollider = true;
            }
        }

        if (!encontrouCollider)
        {
            Debug.LogWarning("CameraController: boundaries encontradas, mas sem Collider2D.");
            return;
        }

        float cameraHalfHeight = playerCamera.orthographicSize;
        float cameraHalfWidth = cameraHalfHeight * playerCamera.aspect;

        minBounds = new Vector2(
            minX + cameraHalfWidth,
            minY + cameraHalfHeight
        );
        maxBounds = new Vector2(
            maxX - cameraHalfWidth,
            maxY - cameraHalfHeight
        );

        if (minBounds.x > maxBounds.x)
        {
            float centerX = (minX + maxX) * 0.5f;
            minBounds.x = centerX;
            maxBounds.x = centerX;
        }

        if (minBounds.y > maxBounds.y)
        {
            float centerY = (minY + maxY) * 0.5f;
            minBounds.y = centerY;
            maxBounds.y = centerY;
        }

        Debug.Log("Limites detectados: min=" + minBounds + ", max=" + maxBounds);
    }

    private void SetCameraPosition()
    {
        if (target != null)
        {
            Vector3 targetPosition = GetTargetPosition();
            Vector3 mousePosition = GetPlayerMousePosition();
            Vector3 desiredCameraPosition = ComputeCameraPosition(targetPosition, mousePosition);

            if (usarLimites)
            {
                desiredCameraPosition.x = Mathf.Clamp(desiredCameraPosition.x, minBounds.x, maxBounds.x);
                desiredCameraPosition.y = Mathf.Clamp(desiredCameraPosition.y, minBounds.y, maxBounds.y);
            }

            transform.position = desiredCameraPosition;
        }
    }

    public Vector3 GetTargetPosition()
    {
        return target != null ? target.position : transform.position;
    }

    public Vector3 GetPlayerMousePosition()
    {
        if (cameraMovementStyle == CameraStyles.Locked) return Vector3.zero;
        return playerCamera.ScreenToWorldPoint(lookAction.ReadValue<Vector2>());
    }

    public Vector3 ComputeCameraPosition(Vector3 targetPosition, Vector3 mousePosition)
    {
        Vector3 result = Vector3.zero;
        switch (cameraMovementStyle)
        {
            case CameraStyles.Locked:
                result = transform.position;
                break;
            case CameraStyles.Overhead:
                result = targetPosition;
                break;
            case CameraStyles.Free:
                Vector3 desiredPosition = Vector3.Lerp(targetPosition, mousePosition, freeCameraMouseTracking);
                Vector3 difference = desiredPosition - targetPosition;
                difference = Vector3.ClampMagnitude(difference, maxDistanceFromTarget);
                result = targetPosition + difference;
                break;
        }

        result.z = cameraZCoordinate;
        return result;
    }
}
