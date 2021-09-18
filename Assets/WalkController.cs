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
    public float turningRate = 30f;
    public OrientationController orientationController;
    public CrankController crankController;
    private Rigidbody rb;

    private Quaternion targetRotation;
    private float lerpProgress = 0f;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        targetRotation = transform.rotation;
    }

    private void FixedUpdate()
    {
        // Movement
        var horizontal = Input.GetAxisRaw("Horizontal") * transform.right;
        var vertical = Input.GetAxisRaw("Vertical") * transform.forward;
        rb.velocity = (horizontal + vertical) * walkSpeed;
        // Rotation
        if (orientationController != null && !crankController.IsCranking)
        {
            //transform.Rotate(Vector3.up, -orientationController.Rotation.y * rotationSpeed);
            targetRotation = transform.rotation * Quaternion.Euler(new Vector3(0, (-orientationController.Rotation.y * rotationSpeed), 0));
        }

        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, turningRate * Time.deltaTime);



        //if(transform.rotation.eulerAngles.y != targetRotationY) // Lerp to target rotation
        //{
        //    targetRotationY = transform.rotation.y + -orientationController.Rotation.y;
        //    var rotation = Mathf.Lerp(transform.rotation.eulerAngles.y, targetRotationY, lerpProgress);
        //}

    }

    public void SetBlendedEulerAngles(Vector3 angles)
    {
        targetRotation = Quaternion.Euler(angles);
    }

}
