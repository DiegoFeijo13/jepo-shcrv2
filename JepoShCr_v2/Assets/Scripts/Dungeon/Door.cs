using UnityEngine;

[DisallowMultipleComponent]
public class Door : MonoBehaviour
{

    [SerializeField] private BoxCollider2D doorCollider;
    [SerializeField] private GameObject doorSprite;

    [HideInInspector] public bool isBossRoomDoor = false;
    private BoxCollider2D doorTrigger;
    private bool isOpen = false;
    private bool previouslyOpened = false;

    private void Awake()
    {
        doorCollider.enabled = false;
        doorSprite.SetActive(false);
        
        doorTrigger = GetComponent<BoxCollider2D>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag(Settings.playerTag) || collision.CompareTag(Settings.playerWeapon))
        {
            OpenDoor();
        }
    }

    public void OpenDoor()
    {
        if (!isOpen)
        {
            isOpen = true;
            previouslyOpened = true;
            doorCollider.enabled = false;
            doorTrigger.enabled = false;
            doorSprite.SetActive(false);


            // play sound effect
            //SoundEffectManager.Instance.PlaySoundEffect(GameResources.Instance.doorOpenCloseSoundEffect);
        }
    }

    public void LockDoor()
    {
        isOpen = false;
        doorCollider.enabled = true;
        doorSprite.SetActive(true);
        doorTrigger.enabled = false;
    }

    public void UnlockDoor()
    {
        doorCollider.enabled = false;
        doorSprite.SetActive(false);
        doorTrigger.enabled = true;

        if (previouslyOpened == true)
        {
            isOpen = false;
            OpenDoor();
        }
    }

    #region Validation
#if UNITY_EDITOR
    private void OnValidate()
    {
        HelperUtilities.ValidateCheckNullValue(this, nameof(doorCollider), doorCollider);
        HelperUtilities.ValidateCheckNullValue(this, nameof(doorSprite), doorSprite);
    }
#endif
    #endregion

}
