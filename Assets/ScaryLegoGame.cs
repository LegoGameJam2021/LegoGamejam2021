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

    // Start is called before the first frame update
    void Start()
    {
        joystickController = GetComponent<CrankController>();
        orientationController = GetComponent<OrientationController>();
        deviceHandler.OnDeviceInitialized += OnDeviceInitialized;
        if (connectToBLE == true)
            deviceHandler.AutoConnectToDeviceOfType(HubType.Technic);
    }

    public void OnDeviceInitialized(ILEGODevice device)
    {
        Debug.LogFormat("OnDeviceInitialized {0}", device);
        joystickController.SetUpWithDevice(device);
        orientationController.SetUpWithDevice(device);
    }
}
