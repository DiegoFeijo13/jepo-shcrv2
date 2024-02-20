using UnityEngine;

[DisallowMultipleComponent]
public class Environment : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;

#if UNITY_EDITOR
    private void OnValidate()
    {
        HelperUtilities.ValidateCheckNullValue(this, nameof(spriteRenderer), spriteRenderer);
    }
#endif
}
