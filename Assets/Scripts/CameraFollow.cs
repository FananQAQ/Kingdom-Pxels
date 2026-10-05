using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(100)]
public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0f, 0.5f, -10f);
    public float smoothTime = 0.15f;
    public bool followY = true;
    public Collider2D worldBounds;

    Camera cam;
    Vector3 velocity;

    void Start()
    {
        cam = GetComponent<Camera>();
    }

    void LateUpdate()
    {
        if (target == null)
            return;

        Vector3 desired = target.position + offset;
        if (!followY)
            desired.y = transform.position.y;

        desired = ClampToBounds(desired);
        transform.position = Vector3.SmoothDamp(transform.position, desired, ref velocity, smoothTime);
        transform.position = ClampToBounds(transform.position);
    }

    Vector3 ClampToBounds(Vector3 position)
    {
        if (worldBounds == null || cam == null || !cam.orthographic)
            return position;

        Bounds b = worldBounds.bounds;
        float halfH = cam.orthographicSize;
        float halfW = halfH * cam.aspect;

        float minX = b.min.x + halfW;
        float maxX = b.max.x - halfW;
        float minY = b.min.y + halfH;
        float maxY = b.max.y - halfH;

        if (minX > maxX)
            position.x = b.center.x;
        else
            position.x = Mathf.Clamp(position.x, minX, maxX);

        if (followY)
        {
            if (minY > maxY)
                position.y = b.center.y;
            else
                position.y = Mathf.Clamp(position.y, minY, maxY);
        }

        return position;
    }
}
