using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class Chatbotbackend : ControllerBase
    {
         private readonly HttpClient _httpClient;

        public Chatbotbackend(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] ChatRequest request)
        {
            var response = await GetGoogleGenerativeAIResponse(request.Message);
            var codeSnippet = ExtractCodeSnippet(response);
            
            if (string.IsNullOrEmpty(codeSnippet))
            {
                return Ok(new { Response = response });
            }

            var projectPath = CreateProject(request.Message, codeSnippet);
            var folderStructure = GetFolderStructure(projectPath);

            return Ok(new { Response = response, FolderStructure = folderStructure });
        }

        // private async Task<string> GetHuggingFaceResponse(string message)
        // {
        //     var huggingFaceApiUrl = "https://api-inference.huggingface.co/models/meta-llama/Meta-Llama-3-8B-Instruct";
        //     var apiKey = "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";
        //     _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
        //     var content = new StringContent($"{{\"inputs\":\"{message}\"}}", System.Text.Encoding.UTF8, "application/json");
        //     var response = await _httpClient.PostAsync(huggingFaceApiUrl, content);
        //     response.EnsureSuccessStatusCode();
        //     var responseString = await response.Content.ReadAsStringAsync();
        //     return responseString;
        // }
        private async Task<string> GetGoogleGenerativeAIResponse(string message)
        {
            var googleGenerativeAiServiceUrl = "http://localhost:5000/generate"; // The URL of your Node.js service

            try
            {
                var payload = new { prompt = message };
                var jsonPayload = JsonSerializer.Serialize(payload);
                var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(googleGenerativeAiServiceUrl, content);
                response.EnsureSuccessStatusCode();

                var responseString = await response.Content.ReadAsStringAsync();
                Console.WriteLine(responseString);
                return responseString; // Return the raw JSON response
            
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error calling Node.js service: {ex.Message}");
                return $"{{ \"error\": \"{ex.Message}\" }}"; // Return error JSON
            }
        }

        private string ExtractCodeSnippet(string response)
        {
            // This is where we can write code extraction logic
            var codeSnippet = response;
            return codeSnippet;
        }

        private string CreateProject(string query, string codeSnippet)
        {
            var projectName = "GeneratedProject";
            var location = "C:\\Exl\\Folders"; 
            var framework = ".NET 8.0";
            var applicationType = "console"; // Default to console app; adjust as needed

            string projectPath = Path.Combine(location, projectName);
            if (!Directory.Exists(projectPath))
            {
                Directory.CreateDirectory(projectPath);
            }

            var processInfo = new ProcessStartInfo("dotnet", $"new {applicationType} -o {projectPath}")
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (var process = Process.Start(processInfo))
            {
                process.WaitForExit();
                var output = process.StandardOutput.ReadToEnd();
                var error = process.StandardError.ReadToEnd();
                if (process.ExitCode != 0)
                {
                    throw new Exception($"Failed to create project: {error}");
                }
            }

            var programFilePath = Path.Combine(projectPath, "Program.cs");
            System.IO.File.WriteAllText(programFilePath, codeSnippet);

            return projectPath;
        }

        private object GetFolderStructure(string path)
        {
            var directoryInfo = new DirectoryInfo(path);
            return GetDirectoryStructure(directoryInfo);
        }

        private object GetDirectoryStructure(DirectoryInfo directoryInfo)
        {
            return new
            {
                name = directoryInfo.Name,
                files = directoryInfo.GetFiles().Select(file => new { name = file.Name, content = System.IO.File.ReadAllText(file.FullName) }).ToList(),
                folders = directoryInfo.GetDirectories().Select(GetDirectoryStructure).ToList()
            };
        }
    }
    public class ChatRequest
    {
        public string? Message { get; set; }
    }
}