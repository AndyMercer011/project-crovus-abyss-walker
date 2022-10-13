using System.Collections;
using System.Collections.Generic;
using MoreMountains.TopDownEngine;
using UnityEngine;

public class TopDownController3DOverride : TopDownController3D
{
    /// <summary>
    /// Resizes the collider to the new size set in parameters
    /// </summary>
    /// <param name="newSize">New size.</param>
    public override void ResizeColliderHeight(float newHeight)
    {
        float newYOffset = _originalColliderCenter.y - (_originalColliderHeight - newHeight) / 2;
        _characterController.height = newHeight;
        _characterController.center = ((_originalColliderHeight - newHeight) / 2) * Vector3.up;
    }
}
