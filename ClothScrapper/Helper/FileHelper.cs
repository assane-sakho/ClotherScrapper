using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ClothScrapper.Helper
{
    class FileHelper
    {
        private readonly HttpClient _httpClient;
        private readonly string _dataFolder = ConfigurationManager.AppSettings["dataFolder"];
        private static FileHelper _instance;
        private FileHelper()
        {
            _httpClient = new HttpClient();
        }

        public async Task DownloadImageAsync(string fileName, string cat, Uri uri)
        {
            // Get the file extension
            var uriWithoutQuery = uri.GetLeftPart(UriPartial.Path);
            var fileExtension = Path.GetExtension(uriWithoutQuery);

            // Create file path and ensure directory exists
            var pathC = Path.Combine(_dataFolder, cat);
            var path = Path.Combine(pathC, $"{fileName}");

            if (!Directory.Exists(pathC))
                Directory.CreateDirectory(pathC);

            // Download the image and write to the file
            var imageBytes = await _httpClient.GetByteArrayAsync(uri);

            await File.WriteAllBytesAsync(path, imageBytes);
        }

        public static FileHelper GetInstance()
        {
            if (_instance == null) ;
            _instance = new FileHelper();
            return _instance;
        }
    }
}
