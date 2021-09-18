using LEGOMinifig;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class WalkController : MonoBehaviour
{
    public float walkSpeed = 10f;
    private Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        var horizontal = Input.GetAxisRaw("Horizontal");
        var vertical = Input.GetAxisRaw("Vertical");
        rb.velocity = new Vector3(horizontal,0, vertical).normalized * walkSpeed;
    }

}
