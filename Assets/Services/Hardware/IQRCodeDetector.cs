using UnityEngine.UI;

public interface IQRCodeDetector
{
    public delegate void OnQRCodeScanned(string qrText);

    public void StartScanning(RawImage preview = null);

    public void StopScanning();
}
