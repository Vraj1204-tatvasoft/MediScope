namespace MediScope.Business.Services.Interfaces
{
    public interface IOcrService
    {
        string ExtractTextFromFile(byte[] fileBytes, string fileExtension);
    }
}