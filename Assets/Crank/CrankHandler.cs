using LEGODeviceUnitySDK;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrankHandler : AbstractServiceUI, ILEGOGeneralServiceDelegate
{
    LEGOTachoMotorCommon motor;

    public override void SetupWithService(ILEGOService service)
    {
        base.SetupWithService(service);

        Debug.LogFormat("{0} SetupWithService {1}", this, service);

        service.ResetCombinedModesConfiguration();
        service.AddCombinedMode((int)MotorWithTachoMode.Speed, 1);
        service.AddCombinedMode((int)MotorWithTachoMode.Position, 1);
        service.ActivateCombinedModes();

        motor = (LEGOTachoMotorCommon)service;
        print("Did motor get setup? ");
    }

    public void DidChangeState(ILEGOService service, ServiceState oldState, ServiceState newState)
    {
        throw new System.NotImplementedException();
    }

    public void DidUpdateInputFormat(ILEGOService service, LEGOInputFormat oldFormat, LEGOInputFormat newFormat)
    {
        throw new System.NotImplementedException();
    }

    public void DidUpdateInputFormatCombined(ILEGOService service, LEGOInputFormatCombined oldFormat, LEGOInputFormatCombined newFormat)
    {
        throw new System.NotImplementedException();
    }

    public void DidUpdateValueData(ILEGOService service, LEGOValue oldValue, LEGOValue newValue)
    {
        // Debug.LogFormat("Service {0} DidUpdateValueData to {1} {2}", service, newValue.RawValues[0], newValue.Mode);
        if (newValue.Mode == (int)MotorWithTachoMode.Position)
        {
            var position = newValue.RawValues[0].ToString();
            var angles  = Vector3.forward * newValue.RawValues[0];
            print("position: "  + position);
            print("angles: "  + angles);
            
        }
        else if (newValue.Mode == (int)MotorWithTachoMode.Speed)
        {
            var speed = newValue.RawValues[0].ToString();
            print("Speed: " + speed);
        }

        print(motor.Speed);
    }
}
