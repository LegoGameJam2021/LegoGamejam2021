using UnityEngine;
using LEGODeviceUnitySDK;

public class JoystickExampleGame : MonoBehaviour
{
    [SerializeField] bool connectToBLE;
    public DeviceHandler deviceHandler;

    // Start is called before the first frame update
    void Start()
    {
        deviceHandler.OnDeviceInitialized += OnDeviceInitialized;
        if (connectToBLE == true)
            deviceHandler.AutoConnectToDeviceOfType(HubType.Technic);
    }

    public void OnDeviceInitialized(ILEGODevice device)
    {
        Debug.LogFormat("OnDeviceInitialized {0}", device);
    }
}
