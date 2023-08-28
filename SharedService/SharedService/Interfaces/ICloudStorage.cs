using Google.Cloud.Storage.V1;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace SharedService.Interfaces
{
    public interface ICloudStorage
    {
        Task<string> UploadFileAsync(IFormFile imageFile, string fileNameForStorage);
        Task DeleteFileAsync(string fileNameForStorage);
        Task DownLoadFileAsync(string nameFile, string dest);
        Task<bool> CheckDuplicate(string nameFile);
        Task movieFileInGCS(string source, string dest);
    }
}
