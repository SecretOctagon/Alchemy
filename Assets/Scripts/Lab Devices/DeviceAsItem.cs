using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeviceAsItem : PortableItem
{
    protected override void Start()
    {
        base.Start();
        InDevice = GetComponent<Device>();
        Name = InDevice.Name;
    }
}
