using System.Diagnostics;
using System.Text;

namespace backend_application.Utilities;

public class ImageProcessing
{
    public static bool EncodeFaces(List<byte[]> images, int id)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build();

        string connectionString = configuration.GetConnectionString("DefaultConnection");
        
        string pythonScriptPath = "C:\\Users\\klaud\\Documents\\development\\SmartHome\\image_processing\\encode_faces.py";
        string imagesString = "[" + string.Join(", ", images.Select(image => $"b'{Convert.ToBase64String(image)}'")) + "]";
        
        string pythonExecutablePath = @"C:\Users\klaud\Documents\development\SmartHome\image_processing\.venv\Scripts\python.exe";

        ProcessStartInfo start = new ProcessStartInfo
        {
            FileName = pythonExecutablePath,
            Arguments = $"{pythonScriptPath} \"{imagesString}\" {id} \"{connectionString}\"",
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true,
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8,
        };

        try
        {
            using Process process = Process.Start(start);
            if (process == null) return false;
            process.WaitForExit();

            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();
            int exitCode = process.ExitCode;

            if (exitCode == 0)
            {
                Console.WriteLine(output);
                return true;
            }
            else
            {
                Console.WriteLine($"Error (Exit Code {exitCode}): {error}");
                return false;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception: {ex.Message}");
            return false;
        }
    }
}