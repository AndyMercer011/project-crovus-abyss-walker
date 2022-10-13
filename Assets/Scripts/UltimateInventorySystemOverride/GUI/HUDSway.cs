using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HUDSway : MonoBehaviour
{

    public Transform Target;
    [Range(0, 1)]
    public float smooth;
    public float velocityScale;
    public float maxVelocityMagnitude;

    private RectTransform rect;
    private Vector2 currentVelocity;
    private Vector2 targetVelocity = Vector2.zero;
    private Vector2 lastPosition = Vector2.zero;

    private void Start()
    {
        rect = GetComponent<RectTransform>();
        lastPosition = (Vector2)Target.localPosition;
    }

    private void Update()
    {
        float deltaTime = Time.deltaTime;
        if (deltaTime != 0)
        {
            targetVelocity = Vector2.ClampMagnitude(((Vector2)Target.localPosition - lastPosition) / deltaTime * velocityScale
                , maxVelocityMagnitude);

            lastPosition = (Vector2)Target.localPosition;

            rect.localPosition = Vector2.SmoothDamp(rect.localPosition, targetVelocity, ref currentVelocity, smooth);
        }
    }

}
