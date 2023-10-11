using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class ActivateRooms : MonoBehaviour
{
    [SerializeField] private Camera miniMapCamera;

    private void Start()
    {
        InvokeRepeating(nameof(EnableRooms), 0.5f, 0.75f);
    }

    private void EnableRooms()
    {
        foreach (var pair in DungeonBuilder.Instance.dungeonBuilderRoomDictionary)
        {
            var room = pair.Value;

            HelperUtilities.CameraWorldPositionBounds(out Vector2Int miniMapCameraWorldPositionLowerBounds, out Vector2Int miniMapCameraWorldPositionUpperBounds, miniMapCamera);

            if((room.lowerBounds.x <= miniMapCameraWorldPositionUpperBounds.x && room.lowerBounds.y <= miniMapCameraWorldPositionUpperBounds.y) &&
               (room.upperBounds.x >= miniMapCameraWorldPositionLowerBounds.x && room.upperBounds.y >= miniMapCameraWorldPositionLowerBounds.y))
            {
                room.instantiatedRoom.gameObject.SetActive(true);
            }
            else
            {
                room.instantiatedRoom.gameObject.SetActive(false);
            }
        }
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