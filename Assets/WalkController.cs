using LEGOMinifig;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class WalkController : MonoBehaviour
{
    public float walkSpeed = 10f;
    public float rotationSpeed = 1f;
    public float lerpSpeed = 1f;
    public OrientationController orientationController;
    public CrankController crankController;
    private Rigidbody rb;

    private float targetRotationY;
    private float lerpProgress = 0f;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        // Movement
        var horizontal = Input.GetAxisRaw("Horizontal") * transform.right;
        var vertical = Input.GetAxisRaw("Vertical") * transform.forward;
        rb.velocity = (horizontal + vertical) * walkSpeed;

        // Rotation
        if(orientationController != null && !crankController.IsCranking)
        {
            transform.Rotate(Vector3.up, -orientationController.Rotation.y * rotationSpeed);
        }

        //if(transform.rotation.eulerAngles.y != targetRotationY) // Lerp to target rotation
        //{
        //    targetRotationY = transform.rotation.y + -orientationController.Rotation.y;
        //    var rotation = Mathf.Lerp(transform.rotation.eulerAngles.y, targetRotationY, lerpProgress);
        //}

    }

}
