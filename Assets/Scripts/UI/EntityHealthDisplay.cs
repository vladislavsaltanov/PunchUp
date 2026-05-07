using System;
using UnityEngine;
using UnityEngine.UI;

public class EntityHealthDisplay : MonoBehaviour
{
    [SerializeField] BaseEntity entity;
    [SerializeField] private Image fillImage;

    private void OnEnable()
    {
        entity.onHealthChanged += UpdateHealth;
    }

    private void UpdateHealth(ushort current, ushort max, ushort finalDamage)
    {
        fillImage.fillAmount = (float)current / (float)max;
    }
}
