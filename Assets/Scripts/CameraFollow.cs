using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0f, 0.5f, -10f);
    public float smoothTime = 0.15f;
    public bool followY = true;

    Vector3 velocity;

    void LateUpdate()
    {
        if (target == null)
            return;

        Vector3 desired = target.position + offset;
        if (!followY)
            desired.y = transform.position.y;

        transform.position = Vector3.SmoothDamp(transform.position, desired, ref velocity, smoothTime);
    }
}
