using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathDevice : Device
{
    Path[] paths;
    float doneRaw;
    float done01 { get => doneRaw / paths.Length; }
}

[SerializeField] public struct Path
{
    public Transform[] points;
}
