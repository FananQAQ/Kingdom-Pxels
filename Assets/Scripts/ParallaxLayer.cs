using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-50)]
public class ParallaxLayer : MonoBehaviour
{
    public Transform cam;
    [Range(0f, 1f)]
    public float parallaxX = 0.3f;
    public bool lockY = true;

    Vector3 startPos;
    Vector3 camStart;

    void Start()
    {
        if (cam == null && Camera.main != null)
            cam = Camera.main.transform;

        startPos = transform.position;
        if (cam != null)
            camStart = cam.position;
    }

    void LateUpdate()
    {
        if (cam == null)
            return;

        Vector3 delta = cam.position - camStart;
        float x = startPos.x + delta.x * parallaxX;
        float y = lockY ? startPos.y : startPos.y + delta.y * parallaxX;
        transform.position = new Vector3(x, y, startPos.z);
    }
}
