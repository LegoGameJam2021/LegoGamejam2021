using LEGODeviceUnitySDK;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class CrankController : MonoBehaviour, ILEGOGeneralServiceDelegate
{
    [SerializeField] float rollFactor = -10f;
    [SerializeField] float pitchFactor = 10f;

    LEGOTechnicMotor motor;

    ILEGODevice device;

    public Slider FlashLightSlider;

    float crankValue = 0;

    // Update is called once per frame
    void Update()
    {
        
    }
    


    public void SetUpWithDevice(ILEGODevice device)
    {

        this.device = device;

        Debug.LogFormat("Setting pitch motor");// Must be connected to port C.

        var largeMotor = ServiceHelper.GetServicesOfType(device, IOType.LEIOTypeTechnicMotorL);
        print("Found motor? " + largeMotor != null);
        if (largeMotor == null || largeMotor.Count() == 0)
        {
            Debug.LogFormat("No large motors found!");
        }
        else
        {
            print("Setting up large motor!");
            motor = (LEGOTechnicMotor) largeMotor.First();
            motor.UpdateInputFormat(new LEGOInputFormat(motor.ConnectInfo.PortID, motor.ioType, motor.PositionModeNo, 1, LEGOInputFormat.InputFormatUnit.LEInputFormatUnitRaw, true));
            motor.RegisterDelegate(this);
        }
    }

    #region ILEGOGeneralService Delegates

    public void DidUpdateValueData(ILEGOService service, LEGOValue oldValue, LEGOValue newValue)
    {
        if(service == motor)
        {
            crankValue = newValue.RawValues[0];
            print("Old: " + oldValue.RawValues[0] + " new: " + newValue.RawValues[0]);
            var delta = oldValue.RawValues[0] - newValue.RawValues[0];
            print("delta: " + delta);
            print("Flashlight: " + Mathf.Abs(delta / 100f));
            this.FlashLightSlider.value += Mathf.Abs(delta/1000f);

        }
    }

    public void DidChangeState(ILEGOService service, ServiceState oldState, ServiceState newState)
    {
        //    Debug.LogFormat("DidChangeState {0} to {1}", service, newState);
    }

    public void DidUpdateInputFormat(ILEGOService service, LEGOInputFormat oldFormat, LEGOInputFormat newFormat)
    {
        //    Debug.LogFormat("DidUpdateInputFormat {0} to {1}", service, newFormat);
    }

    public void DidUpdateInputFormatCombined(ILEGOService service, LEGOInputFormatCombined oldFormat, LEGOInputFormatCombined newFormat)
    {
        //    Debug.LogFormat("DidUpdateInputFormatCombined {0} to {1}", service, newFormat);
    }

    #endregion
}
