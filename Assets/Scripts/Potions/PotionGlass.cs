using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PotionGlass : MonoBehaviour
{
    public PotionStats stats;
    public float fill { get => potionLevel.fill; }
    [SerializeField] string sizePrefix;
    public float capacity;
    PotionLevel potionLevel;

    void Start()
    {
        potionLevel = GetComponentInChildren<PotionLevel>();
        ChangeStats(stats);
    }

    public void ChangeStats(PotionStats newStats)
    {
        stats = newStats;
        PortableItem item = GetComponent<PortableItem>();
        switch ((bool)stats)
        {
            case true:
                potionLevel.SetMaterial(stats.material);
                item.Name = sizePrefix + " " + stats.name;
                break;
            case false:
                potionLevel.fill = 0;
                item.Name = sizePrefix + " empty vial";
                break;
        }
        potionLevel.gameObject.SetActive(stats);
    }
    public void SetFill(float f)
    {
        potionLevel.fill = f;
    }
}
