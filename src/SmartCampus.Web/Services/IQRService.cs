using QRCoder;

namespace SmartCampus.Web.Services;

public interface IQRService
{
    string GenerateBase64QrCode(string payload);
}

public class QRService : IQRService
{
    public string GenerateBase64QrCode(string payload)
    {
        using var qrGenerator = new QRCodeGenerator();
        using var qrCodeData = qrGenerator.CreateQrCode(payload, QRCodeGenerator.ECCLevel.Q);
        using var qrCode = new PngByteQRCode(qrCodeData);
        byte[] qrCodeBytes = qrCode.GetGraphic(20);
        return Convert.ToBase64String(qrCodeBytes);
    }
}
