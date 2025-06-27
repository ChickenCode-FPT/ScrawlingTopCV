using MongoDB.Driver;
using Services.Entitys;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Services.MongoDBService
{
    public class MongoDbService
    {
        private readonly IMongoCollection<JobListing> _jobListingsCollection;

        public MongoDbService(string connectionString, string databaseName, string collectionName)
        {
            var client = new MongoClient(connectionString);
            var database = client.GetDatabase(databaseName);
            _jobListingsCollection = database.GetCollection<JobListing>(collectionName);
        }

        public async Task<bool> JobExistsByDescriptionHashAsync(string descriptionHash)
        {
            var filter = Builders<JobListing>.Filter.Eq(jl => jl.DescriptionHash, descriptionHash);
            var count = await _jobListingsCollection.CountDocumentsAsync(filter);
            return count > 0;
        }

        public async Task SaveJobDetailsAsync(string jobUrl, Dictionary<string, string> details)
        {
            string jobDescriptionForHash = string.Empty;
            if (details.TryGetValue("Mô tả công việc", out string? descriptionContent))
            {
                jobDescriptionForHash = descriptionContent;
            }
            else if (details.TryGetValue("Job description", out string? englishDescriptionContent))
            {
                jobDescriptionForHash = englishDescriptionContent; 
            }

            string descriptionHash = ComputeSha256Hash(jobDescriptionForHash);

           
            if (await JobExistsByDescriptionHashAsync(descriptionHash))
            {
                LogMessage($"  Công việc từ URL '{jobUrl}' (Hash: {descriptionHash.Substring(0, 8)}...) đã tồn tại. Bỏ qua lưu.");
                return;
            }

            var jobListing = new JobListing
            {
                JobUrl = jobUrl,
                DescriptionHash = descriptionHash, // Lưu hash vào model
                Details = details,
                ExtractedAt = DateTime.UtcNow
            };

            await _jobListingsCollection.InsertOneAsync(jobListing);
            LogMessage($"  Đã lưu dữ liệu công việc mới từ URL '{jobUrl}' vào MongoDB (Hash: {descriptionHash.Substring(0, 8)}...).");
        }

        // Hàm tính SHA256 hash
        private static string ComputeSha256Hash(string rawData)
        {
            // Tạo SHA256
            using (SHA256 sha256Hash = SHA256.Create())
            {
                // Tính toán Hash
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));

                // Chuyển đổi mảng byte sang chuỗi hex
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }

        // Hàm LogMessage giả định
        private static void LogMessage(string message)
        {
            Console.WriteLine(message);
        }
    }
}
