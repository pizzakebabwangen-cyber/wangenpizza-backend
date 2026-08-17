using Microsoft.AspNetCore.Http;
using NuGet.DependencyResolver;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickMover.Helper
{

    public static class FileUploader
    {
        public static string UploadFile(string localPath, IFormFile file)
        {
            try
            {
                // 1. Get Directory Path
                string directoryPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", localPath);

                // Ensure the directory exists
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                // 2. Get Original File Name
                string originalFileName = Path.GetFileName(file.FileName);

                // 3. Create Final Path (Avoid Overwriting by Appending Unique Suffix if the file exists)
                string finalPath = Path.Combine(directoryPath, originalFileName);
                int counter = 1;
                while (File.Exists(finalPath))
                {
                    // If the file exists, append a suffix to the file name (e.g., test(1).png)
                    string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(originalFileName);
                    string extension = Path.GetExtension(originalFileName);
                    string newFileName = $"{fileNameWithoutExtension}({counter}){extension}";
                    finalPath = Path.Combine(directoryPath, newFileName);
                    counter++;
                }

                // 4. Save File As Stream
                using (var stream = new FileStream(finalPath, FileMode.Create))
                {
                    file.CopyTo(stream);
                }

                // Return the final file name (this is what you save to the database)
                return Path.GetFileName(finalPath);
            }
            catch (Exception ex)
            {
                // Log or return the error message
                return $"Error: {ex.Message}";
            }
        }

        public static string RemoveFile(string localPath, string fileName)
        {
            try
            {
                string deletedPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", localPath, fileName);
                if (File.Exists(deletedPath))
                {
                    File.Delete(deletedPath);
                }

                return "Deleted";
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }
    }
}
