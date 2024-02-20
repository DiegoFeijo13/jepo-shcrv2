using System;
using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
public class DestroyableItem : MonoBehaviour
{
    [SerializeField] private int startingHealthAmount = 1;
    [SerializeField] private SoundEffectSO destroySE;

    private BoxCollider2D boxCollider2D;
    private HealthEvent healthEvent;
    private Health health;
    private ReceiveContactDamage receiveContactDamage;

    private void Awake()
    {
        boxCollider2D = GetComponent<BoxCollider2D>();
        healthEvent = GetComponent<HealthEvent>();
        health = GetComponent<Health>();
        receiveContactDamage = GetComponent<ReceiveContactDamage>();
        
        health.SetStartingHealth(startingHealthAmount);
    }

    private void OnEnable()
    {
        healthEvent.OnHealthChanged += HealthEvent_OnHealthLost;
    }

    private void OnDisable()
    {
        healthEvent.OnHealthChanged -= HealthEvent_OnHealthLost;
    }

    private void HealthEvent_OnHealthLost(HealthEvent healthEvent, HealthEventArgs args)
    {
        if(args.healthAmount <= 0f)
        {
            StartCoroutine(DestroyCO());
        }
    }

    private IEnumerator DestroyCO()
    {
        Destroy(boxCollider2D);

        if (destroySE != null)
            SoundEffectManager.Instance.PlaySoundEffect(destroySE);

        //TODO: Play destroy animation here        

        yield return null;

        Destroy(receiveContactDamage);
        Destroy(health);
        Destroy(healthEvent);
        Destroy(this.gameObject);
        Destroy(this);
    }
}
