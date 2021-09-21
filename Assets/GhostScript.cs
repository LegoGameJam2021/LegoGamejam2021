using LEGOMinifig;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.UI;

public class GhostScript : MonoBehaviour
{
    public MinifigController enemy;

    public Transform player;

    public GameObject PlayerArm;

    public GameObject PlayerBody;

    public Camera CameraOnPlayer;

    public UIManager uiManager;

    public GameObject EnergyBar;
    private void Start()
    {
        EnergyBar.SetActive(false);

        Action onCompleted = () =>
        {
            PlayerBody.SetActive(true);
            PlayerArm.SetActive(false);
            var dirVector = Camera.main.transform.forward;
            var oldposition = Camera.main.transform.position;
            var newPosition = new Vector3(oldposition.x - 4f * dirVector.x, oldposition.y, oldposition.z - 4f * dirVector.z);
            Camera.main.transform.position = newPosition;
            MinifigController controller = PlayerBody.GetComponent<MinifigController>();
            //Thread.Sleep(1000);
            controller.Explode();
            //Thread.Sleep(2000);
            EnergyBar.SetActive(false);

            uiManager.ShowDeathMessage();

        };
        uiManager.ShowStartScene();

        StartCoroutine(EnemyFollowPlayer(onCompleted));



    }

    private IEnumerator EnemyFollowPlayer(Action onCompleted)
    {

        yield return new WaitForSeconds(12f);
        EnergyBar.SetActive(true);
        if (enemy != null)
            enemy.Follow(player, 2f, onCompleted, 1, 0);
    }

    

}
