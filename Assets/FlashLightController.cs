using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FlashLightController : MonoBehaviour
{
    public float SecondsToEmpty;

    public bool FlashLightIsOn;

    public bool FlashLightIsEmpty;

    public int CrankingValue;

    public Slider FlashLightSlider;

    public Light Light;


    [Range(0, 100)]
    public float FlashLightValue = 0;

    public int StartPointOfFlashing = 10;

    public float flashlightStunCooldown = 10f;

    float timeSinceHitenemy;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (FlashLightIsOn && this.FlashLightValue > 0)
        {
            var newVal = this.FlashLightValue - (Time.deltaTime * 100 / SecondsToEmpty);
            this.FlashLightValue = newVal > 0 ? newVal : 0;

            if (this.FlashLightValue < this.StartPointOfFlashing)
            {
                StartCoroutine(this.FlashLight());
            }


            // Do raycast
            if(Time.realtimeSinceStartup > timeSinceHitenemy + flashlightStunCooldown &&
                Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out RaycastHit hit, 100f))
            {
                if (hit.transform.CompareTag("Enemy"))
                {
                    // do we stuff
                    timeSinceHitenemy = Time.realtimeSinceStartup;


                }
            }
        }
        if (this.FlashLightValue == 0)
        {
            this.FlashLightIsOn = false;
            this.SetLight();
        }

        this.FlashLightIsEmpty = this.FlashLightValue == 0;
        this.FlashLightSlider.value = FlashLightValue / 100f;
    }

    public void CrankFlashLight(float crankValue)
    {
        var newVal = this.FlashLightValue + crankValue / 1000f * CrankingValue;
        this.FlashLightValue = newVal < 100 ? newVal : 100;
    }

    public void TurnOnOff()
    {
        if (!this.FlashLightIsEmpty)
        {
            this.FlashLightIsOn = !this.FlashLightIsOn;
        }
        else
        {
            this.FlashLightIsOn = false;
        }
        this.SetLight();
    }

    private void SetLight()
    {
        this.Light.intensity = this.FlashLightIsOn ? 3 : 0;
    }

    private IEnumerator FlashLight()
    {
        while(this.FlashLightValue > 0 && this.FlashLightValue < this.StartPointOfFlashing)
        {
            this.Light.intensity = 0;
            yield return new WaitForSeconds(1f);
            this.Light.intensity = 3;
            yield return new WaitForSeconds(2f);
        }

    }

}
