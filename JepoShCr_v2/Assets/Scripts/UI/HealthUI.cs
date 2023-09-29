using System;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class HealthUI : MonoBehaviour
{
    [SerializeField] private Transform healthBar;

    private void OnEnable()
    {
        GameManager.Instance.GetPlayer().healthEvent.OnHealthChanged += HealthEvent_OnHealthChanged;
    }

    private void OnDisable()
    {
        GameManager.Instance.GetPlayer().healthEvent.OnHealthChanged -= HealthEvent_OnHealthChanged;
    }

    private void HealthEvent_OnHealthChanged(HealthEvent healthEvent, HealthEventArgs args)
    {
        UpdateHealthBar(args);
    }

    private void UpdateHealthBar(HealthEventArgs args)
    {
        var barFill = args.healthPercent;

        if(barFill < 0)
            barFill = 0;
        
        healthBar.transform.localScale = new Vector3(barFill, 1f, 1f);
    }
}
