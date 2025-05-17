using System.Collections.Generic;
using System;
using UnityEngine;
public interface ISensorCapability
{
    Vector3? GetTargetPosition();
    IEdible GetTargetObject();
    bool HasTarget();
}