using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class ActivateRooms : MonoBehaviour
{
    [SerializeField] private Camera miniMapCamera;

    private Camera mainCamera;

    private void Start()
    {
        mainCamera = Camera.main;
        InvokeRepeating(nameof(EnableRooms), 0.5f, 0.75f);
    }

    private void EnableRooms()
    {
        HelperUtilities.CameraWorldPositionBounds(out Vector2Int miniMapCameraWorldPositionLowerBounds, out Vector2Int miniMapCameraWorldPositionUpperBounds, miniMapCamera);

        HelperUtilities.CameraWorldPositionBounds(out Vector2Int mainCameraWorldPositionLowerBounds, out Vector2Int mainCameraWorldPositionUpperBounds, mainCamera);

        foreach (var pair in DungeonBuilder.Instance.dungeonBuilderRoomDictionary)
        {
            var room = pair.Value;

            if (IsInsideBounds(room, miniMapCameraWorldPositionLowerBounds, miniMapCameraWorldPositionUpperBounds))
            {
                room.instantiatedRoom.gameObject.SetActive(true);

                if (IsInsideBounds(room, mainCameraWorldPositionLowerBounds, mainCameraWorldPositionUpperBounds))
                {
                    room.instantiatedRoom.ActivateEnvironmentObjects();
                }
                else
                {
                    room.instantiatedRoom.DeactivateEnvironmentObjects();
                }
            }
            else
            {
                room.instantiatedRoom.gameObject.SetActive(false);
            }
        }
    }

    private bool IsInsideBounds(Room room, Vector2Int cameraLowerBounds, Vector2Int cameraUpperBounds)
    {
        return (room.lowerBounds.x <= cameraUpperBounds.x && room.lowerBounds.y <= cameraUpperBounds.y) &&
               (room.upperBounds.x >= cameraLowerBounds.x && room.upperBounds.y >= cameraLowerBounds.y);
        
    }

    #region Validation
#if UNITY_EDITOR
    private void OnValidate()
    {
        HelperUtilities.ValidateCheckNullValue(this, nameof(miniMapCamera), miniMapCamera);
    }
#endif
    #endregion Validation
}
