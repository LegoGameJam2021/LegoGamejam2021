using LEGODeviceUnitySDK;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class ColorScannerController : MonoBehaviour, ILEGOGeneralServiceDelegate
{

    LEGOTechnicColorSensor colorSensor;
    public Color currentColor;
    ILEGODevice device;


    float crankValue = 0;


    // Update is called once per frame
    void Update()
    {

    }



    public void SetUpWithDevice(ILEGODevice device)
    {

        this.device = device;

        var colorSensors = ServiceHelper.GetServicesOfType(device, IOType.LEIOTypeTechnicColorSensor);
        print("Found color scanner sensor? " + colorSensors != null);
        if (colorSensors == null || colorSensors.Count() == 0)
        {
            Debug.LogFormat("No color scanner sensor found!");
        }
        else
        {
            print("Setting up color scanner sensor!");
            colorSensor = (LEGOTechnicColorSensor)colorSensors.First();
            colorSensor.UpdateInputFormat(new LEGOInputFormat(colorSensor.ConnectInfo.PortID, colorSensor.ioType, (int)LEGOColorSensor.LEColorSensorMode.Color, 1, LEGOInputFormat.InputFormatUnit.LEInputFormatUnitRaw, true));
            colorSensor.RegisterDelegate(this);
        }
    }

    #region ILEGOGeneralService Delegates

    public void DidUpdateValueData(ILEGOService service, LEGOValue oldValue, LEGOValue newValue)
    {
        if (service == colorSensor)
        {
            int index = (int)newValue.RawValues[0];
            if (index == -1)
            {
                currentColor = new Color(0, 0, 0, 0);
            }
            else
            {
                currentColor = _defaultColorSet[index];
            }

            print($"{currentColor}");
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
    private Color[] _defaultColorSet = new Color[]
            {
                Color0(),
                Color1(),
                Color2(),
                Color3(),
                Color4(),
                Color5(),
                Color6(),
                Color7(),
                Color8(),
                Color9(),
                Color10(),
            };
    private static Color Color10()
    {
        return new Color32(255, 110, 60, 255);
    }

    private static Color Color9()
    {
        return new Color32(255, 0, 0, 255);
    }

    private static Color Color8()
    {
        return new Color32(255, 20, 0, 255);
    }

    private static Color Color7()
    {
        return new Color32(255, 55, 0, 255);
    }

    private static Color Color6()
    {
        return new Color32(0, 200, 5, 255);
    }

    private static Color Color5()
    {
        return new Color32(0, 255, 60, 255);
    }

    private static Color Color4()
    {
        return new Color32(70, 155, 140, 255);
    }

    private static Color Color3()
    {
        return new Color32(0, 0, 255, 255);
    }

    private static Color Color2()
    {
        return new Color32(145, 0, 130, 255);
    }

    private static Color Color1()
    {
        return new Color32(255, 10, 18, 255);
    }

    private static Color Color0()
    {
        //aka. off
        return new Color32(0, 0, 0, 255);
    }
    #endregion
}
