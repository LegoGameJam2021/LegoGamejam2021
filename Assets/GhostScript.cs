using LEGOMinifig;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class GhostScript : MonoBehaviour
{
    public MinifigController enemy;

    public Transform player;

    public GameObject PlayerArm;

    public GameObject PlayerBody;

    public Camera CameraOnPlayer;

    private void Start()
    {
        Action onCompleted = () =>
        {
            PlayerBody.SetActive(true);
            PlayerArm.SetActive(false);
            var dirVector = Camera.main.transform.forward;
            var oldposition = Camera.main.transform.position;
            var newPosition = new Vector3(oldposition.x - 4f * dirVector.x, oldposition.y, oldposition.z - 4f * dirVector.z);
            Camera.main.transform.position = newPosition;
            MinifigController controller = PlayerBody.GetComponent<MinifigController>();
            Thread.Sleep(1000);
            controller.Explode();
        };
        enemy.Follow(player, 2f, onCompleted, 1, 0);

    }

}
