using Google.Apis.Auth.OAuth2;
using Google.Cloud.Storage.V1;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using SharedService.Interfaces;

namespace SharedService.Services
{
    //Service for running Google Cloud service
    public class GoogleCloudStorage : ICloudStorage
    {
        private readonly GoogleCredential googleCredential;
        private readonly StorageClient storageClient;
        private readonly string bucketName;
        private readonly string rootfolder;
        private readonly string newfolder;

        public GoogleCloudStorage(IConfiguration configuration)
        {
            googleCredential = GoogleCredential.FromFile(configuration.GetValue<string>("GoogleCredentialFile"));
            //googleCredential = GoogleCredential.FromFile()
            storageClient = StorageClient.Create(googleCredential);
            //bucketName = configuration.GetValue<string>("GoogleCloudStorageBucket");
            bucketName = "ifd-aspnetcore-training";
            rootfolder = configuration.GetValue<string>("GoogleWorkingFileDictionary:root");
            newfolder = configuration.GetValue<string>("GoogleWorkingFileDictionary:new");
        }

        /// <summary>
        /// Upload the file to GG Cloud
        /// </summary>
        /// <param name="imageFile"></param>
        /// <param name="fileNameForStorage"></param>
        /// <returns></returns>
        public async Task<string> UploadFileAsync(IFormFile imageFile, string fileNameForStorage)
        {
            using (var memoryStream = new MemoryStream())
            {
                await imageFile.CopyToAsync(memoryStream);
                var dataObject = await storageClient.UploadObjectAsync(bucketName, "Viet/" + fileNameForStorage, null, memoryStream);
                return dataObject.MediaLink; // the Public link of the file
            }
        }

        /// <summary>
        /// Delete the file from GG Cloud
        /// </summary>
        /// <param name="fileNameForStorage"></param>
        /// <returns></returns>
        public async Task DeleteFileAsync(string fileNameForStorage)
        {
            await storageClient.DeleteObjectAsync(bucketName, "Viet/" + fileNameForStorage);
        }

        //Download the file from GG Cloud
        public async Task DownLoadFileAsync(string nameFile,string dest)
        {
            var fileNameForStorage = $"Viet/Course/new/{nameFile}";

            //Download file location (Should use environment file local location)
            var fileStream = File.Create(Path.Combine(dest,nameFile));

            await storageClient.DownloadObjectAsync(bucketName, fileNameForStorage, fileStream);

            fileStream.Close();
        }

        public async Task<bool> CheckDuplicate(string fileName)
        {
            List<string> duplicate = new List<string>();
            var listobject = storageClient.ListObjects(bucketName, rootfolder); // Get list of file in GCS
            //bool filetocheck = false;
            foreach (var storageObject in listobject)
            {
                if (storageObject.Name.Contains(newfolder))
                {
                    continue;
                }
                if (duplicate.Contains(fileName))
                {          
                        return true;
                }
                /*if (storageObject.Name == $"Viet/Course/new/{fileName}" && filetocheck == false)
                {
                    filetocheck = true;
                    continue;
                }*/
                if (Path.GetFileName(storageObject.Name) != "" )
                    duplicate.Add(Path.GetFileName(storageObject.Name));              
            }
            return false;
        }

        public async Task MoveFileInGCS(string source, string dest)
        {
            storageClient.CopyObject(bucketName, source, bucketName, dest);
            storageClient.DeleteObject(bucketName, source);
        }
    }
}
