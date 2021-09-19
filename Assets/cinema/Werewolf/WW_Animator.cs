using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WW_Animator : MonoBehaviour
{
    Animator m_Animator;
    // Start is called before the first frame update
    void Start()
    {
        //Get the Animator attached to the GameObject you are intending to animate.
        m_Animator = gameObject.GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
         if (Input.GetKeyDown(KeyCode.C))
        {
            //Send the message to the Animator to activate the trigger parameter named "Jump"
            m_Animator.SetTrigger("attack");
        }
    }
}
