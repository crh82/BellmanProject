using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

public class CursorTrail : MonoBehaviour
{
    public float         distanceFromCamera = 2;
    public float         startWidth = 0.007f;
    public float         endWidth = 0f;
    public Camera        cameraRenderingTheCursorTrail;
    public Transform     trail;
    public TrailRenderer trailRenderer;

    // Start is called before the first frame update
    void Start()
    {
        cameraRenderingTheCursorTrail = Camera.main;
        trailRenderer = trail.GetComponent<TrailRenderer>();
        trailRenderer.startWidth = startWidth;
        trailRenderer.endWidth = endWidth;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButton(1))
        {
            trailRenderer.startWidth = startWidth;
            trailRenderer.endWidth = endWidth;
            trailRenderer.emitting = true;
            MoveTrailToCursor(Input.mousePosition);
        }
        else
        {
            trailRenderer.startWidth = startWidth;
            trailRenderer.endWidth = endWidth;
            trailRenderer.emitting = false;
            MoveTrailToCursor(Input.mousePosition);
        }
        
        trailRenderer.time = Input.GetKeyDown(KeyCode.BackQuote) ? 0f : Single.PositiveInfinity;
        
    }

    void MoveTrailToCursor(Vector3 screenPosition)
    {
        var vec = Vector3.Lerp(
            new Vector3(screenPosition.x, screenPosition.y, distanceFromCamera), 
            new Vector3(Input.mousePosition.x, Input.mousePosition.y, distanceFromCamera),
            Time.deltaTime * 20);
        trail.position = cameraRenderingTheCursorTrail.ScreenToWorldPoint(vec);
    }
}
