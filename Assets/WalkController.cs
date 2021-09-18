using LEGOMinifig;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class WalkController : MonoBehaviour
{
    public float walkSpeed = 10f;
    public float rotationSpeed = 10f;
    public OrientationController orientationController;
    private Rigidbody rb;

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
        if(orientationController != null)
            transform.Rotate(Vector3.up, -orientationController.Rotation.y * rotationSpeed);

    }

}
