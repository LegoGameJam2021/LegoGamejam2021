using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndingSequence : MonoBehaviour
{

    public Camera mainCamera;
    public GameObject WolfMan;
    public Vector3 CameraPosition;
    public float wolfSpeed = 10f;
    public float attackCooldown = 3f;
    public GameObject SliderGO;

    Animator wolfAnimator;
    Vector3 endPos;
    bool didWeEnd;
    bool isWalking;
    float timeSinceAttack;

    private void Start()
    {
        CameraPosition = new Vector3(101.4f, 27.19f, -121.9f);
        endPos = new Vector3(120, 0.6f, -172f);
        wolfAnimator = WolfMan.GetComponent<Animator>();

    }

    private void Update()
    {
        if (isWalking)
        {

            var dir = (endPos - WolfMan.transform.position).normalized;
            WolfMan.transform.position += dir * wolfSpeed * Time.deltaTime;
            if(Vector3.Distance(WolfMan.transform.position, endPos) < 1)
            {
                isWalking = false;
                timeSinceAttack = Time.realtimeSinceStartup;
                // Play the attack animation
                wolfAnimator.SetTrigger("attack");
            }
        }
        if(!isWalking && Time.realtimeSinceStartup > timeSinceAttack + attackCooldown)
        {
            wolfAnimator.SetTrigger("attack");
            timeSinceAttack = Time.realtimeSinceStartup;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        print("Triggered");

        if (!didWeEnd && other.CompareTag("Player"))
        {
            print("Triggered2");
            didWeEnd = true;
            StartCoroutine(StartCinematic());
        }
    }


    public IEnumerator StartCinematic()
    {
        yield return new WaitForSeconds(3);
        SliderGO.SetActive(false);
        mainCamera.transform.position = CameraPosition;
        mainCamera.transform.rotation = Quaternion.Euler(new Vector3(22.9f, -212, 0));
        WolfMan.SetActive(true);
        WolfMan.transform.position = new Vector3(114, 0.6f, -240);
        isWalking = true;




        //WolfMan.transform.rotation = Quaternion.Euler(0, 107, 0);
        // TODO: Maybe lookat rotation


    }


}
