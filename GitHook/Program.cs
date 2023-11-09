using System.Diagnostics;
using System.Net;
using System.Text.Json;
using System.Web;

namespace GitHook;

internal class Program
{
    private static async Task Main(string[] args)
    {
        if (args.Length > 0)
        {
            var repos = args[0];
            var port = args[1];
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
                var pr = new ProcessRequest(context, repos);
                pr.StartProcess();
            }
        }

        Console.WriteLine("No arguments were passed.");
        Environment.Exit(1);
    }
}

class ProcessRequest
{
    private readonly HttpListenerContext _context;
    private readonly string _repos;

    public ProcessRequest(HttpListenerContext context, string repos)
    {
        _context = context;
        _repos = repos;
    }

    public async Task StartProcess()
    {
        var request = _context.Request;
        using var response = _context.Response;

        if (request.HttpMethod == "POST")
        {
            using var reader = new StreamReader(request.InputStream, request.ContentEncoding);
            var postData = await reader.ReadToEndAsync();

            if (request.ContentType != null && request.ContentType.StartsWith("application/x-www-form-urlencoded"))
            {
                var formData = HttpUtility.ParseQueryString(postData);
                Console.WriteLine($"Form data received: {formData}");
                foreach (var k in formData.AllKeys) Console.WriteLine($"{k}: {formData.Get(k)}");
            }
            else if (request.ContentType != null && request.ContentType.StartsWith("application/json"))
            {
                var jsonData = JsonDocument.Parse(postData);
                // var secret = jsonData.RootElement.GetProperty("secret").GetString();
                await ExecCmd($"cd {_repos} && git reset --hard && git pull");
            }
        }
    }

    private async Task ExecCmd(string command)
    {
        using var process = new Process();
        process.StartInfo = new ProcessStartInfo
        {
            FileName = "/bin/bash",
            Arguments = $"-c \"{command}\"",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        process.Start();

        var output = await process.StandardOutput.ReadToEndAsync();
        var error = await process.StandardError.ReadToEndAsync();

        await process.WaitForExitAsync();

        Console.WriteLine("Command output:");
        Console.WriteLine(output);

        if (!string.IsNullOrEmpty(error))
        {
            Console.WriteLine("Command error:");
            Console.WriteLine(error);
        }
    }
}