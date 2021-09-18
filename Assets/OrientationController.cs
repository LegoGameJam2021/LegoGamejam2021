using LEGODeviceUnitySDK;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class OrientationController : MonoBehaviour, ILEGOGeneralServiceDelegate
{
    [SerializeField] float rollFactor = -10f;
    [SerializeField] float pitchFactor = 10f;

    LEGOTechnic3AxisOrientationSensor orientationSensor;

    ILEGODevice device;


    float crankValue = 0;

    public Vector3 Rotation;

    // Update is called once per frame
    void Update()
    {

    }



    public void SetUpWithDevice(ILEGODevice device)
    {

        this.device = device;

        var orientationSensors = ServiceHelper.GetServicesOfType(device, IOType.LEIOTypeTechnic3AxisOrientationSensor);
        print("Found orientation sensor? " + orientationSensors != null);
        if (orientationSensors == null || orientationSensors.Count() == 0)
        {
            Debug.LogFormat("No orientation sensor found!");
        }
        else
        {
            print("Setting up orientation sensor!");
            orientationSensor = (LEGOTechnic3AxisOrientationSensor)orientationSensors.First();
            orientationSensor.UpdateInputFormat(new LEGOInputFormat(orientationSensor.ConnectInfo.PortID, orientationSensor.ioType, 0, 1, LEGOInputFormat.InputFormatUnit.LEInputFormatUnitRaw, true));
            orientationSensor.RegisterDelegate(this);
        }
    }

    #region ILEGOGeneralService Delegates

    public void DidUpdateValueData(ILEGOService service, LEGOValue oldValue, LEGOValue newValue)
    {
        if (service == orientationSensor)
        {
            Rotation.x = newValue.RawValues[1] - oldValue.RawValues[1];
            Rotation.y = newValue.RawValues[0] - oldValue.RawValues[0];
            Rotation.z = newValue.RawValues[2] - oldValue.RawValues[2];

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
