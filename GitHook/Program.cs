using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;

namespace GitHook;

class Program
{
    private static string repos;
    private static string port;

    static async Task Main(string[] args)
    {
        if (args.Length > 0)
        {
            repos = args[0];
            port = args[1];
            if (!Directory.Exists(repos))
            {
                Console.WriteLine("Not valid directory\n usage: ./GitHook /path/to/your/git/repos ");
                Environment.Exit(1);
            }
            if (!HttpListener.IsSupported)
            {
                Console.WriteLine("HttpListener not supported\n usage: ./GitHook /path/to/your/git/repos ");
                return;
            }

            var listener = new HttpListener
            {
                Prefixes = { $"http://*:{port}/" }
            };

            listener.Start();
            Console.WriteLine($"Listening for requests on http://*:{port}/");

            while (true)
            {
                var context = await listener.GetContextAsync();
                ProcessRequest(context);
            }
        }
        else
        {
            Console.WriteLine("No arguments were passed.");
            Environment.Exit(1);
        }
       
    }

    private static async void ProcessRequest(HttpListenerContext context)
    {
        var request = context.Request;
        var response = context.Response;

        if (request.HttpMethod == "POST")
        {
            using var reader = new StreamReader(request.InputStream, request.ContentEncoding);
            var postData = await reader.ReadToEndAsync();

            if (request.ContentType != null && request.ContentType.StartsWith("application/x-www-form-urlencoded"))
            {
                var formData = HttpUtility.ParseQueryString(postData);
                Console.WriteLine($"Form data received: {formData}");
                foreach (string k in formData.AllKeys)
                {
                    Console.WriteLine($"{k}: {formData.Get(k)}");
                }
            }
            else if (request.ContentType != null && request.ContentType.StartsWith("application/json"))
            {
                JsonDocument jsonData = JsonDocument.Parse(postData);
                // var secret = jsonData.RootElement.GetProperty("secret").GetString();
                ExecCmd($"cd {repos} && git reset --hard && git pull");
            }

            response.Close();
        }
    }

    private static void ExecCmd(string command)
    {
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "/bin/bash",
                Arguments = $"-c \"{command}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            }
        };

        process.Start();

        string output = process.StandardOutput.ReadToEnd();
        string error = process.StandardError.ReadToEnd();

        process.WaitForExit();

        Console.WriteLine("Command output:");
        Console.WriteLine(output);

        if (!string.IsNullOrEmpty(error))
        {
            Console.WriteLine("Command error:");
            Console.WriteLine(error);
        }
    }
}