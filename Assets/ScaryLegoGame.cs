using LEGODeviceUnitySDK;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScaryLegoGame : MonoBehaviour
{
    [SerializeField] bool connectToBLE;
    public DeviceHandler deviceHandler;
    CrankController joystickController;
    OrientationController orientationController;
    ColorScannerController colorScannerController;

    // Start is called before the first frame update
    void Start()
    {
        joystickController = GetComponent<CrankController>();
        orientationController = GetComponent<OrientationController>();
        colorScannerController = GetComponent<ColorScannerController>();
        deviceHandler.OnDeviceInitialized += OnDeviceInitialized;
        if (connectToBLE == true)
            deviceHandler.AutoConnectToDeviceOfType(HubType.Technic);
    }

    public void OnDeviceInitialized(ILEGODevice device)
    {
        Debug.LogFormat("OnDeviceInitialized {0}", device);
        if(joystickController != null)
            joystickController.SetUpWithDevice(device);
        if(orientationController != null)
            orientationController.SetUpWithDevice(device);
        if(colorScannerController != null)
            colorScannerController.SetUpWithDevice(device);
    }
}
