using System;
using System.IO;
using System.Text.Json;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;

class AliasRunner
{
    static int Main(string[] args)
    {
        string jsonPath = Path.Combine(Directory.GetCurrentDirectory(), "alias.json");

        if (!File.Exists(jsonPath))
        {
            Console.WriteLine($"Error: alias.json not found in {Directory.GetCurrentDirectory()}");
            return 1;
        }

        Dictionary<string, string>? aliases;
        try
        {
            string json = File.ReadAllText(jsonPath);
            aliases = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error reading alias.json: {ex.Message}");
            return 1;
        }

        if (aliases == null || aliases.Count == 0)
        {
            Console.WriteLine("No aliases found in alias.json.");
            return 1;
        }

        // If no args, list aliases
        if (args.Length == 0)
        {
            Console.WriteLine("Available aliases:\n");
            foreach (var kvp in aliases)
                Console.WriteLine($"  {kvp.Key,-15} → {kvp.Value}");
            return 0;
        }

        string aliasName = args[0];
        if (!aliases.ContainsKey(aliasName))
        {
            Console.WriteLine($"Alias '{aliasName}' not found.");
            return 1;
        }

        // Build full command (alias command + extra args)
        string command = aliases[aliasName];
        if (args.Length > 1)
        {
            string extraArgs = string.Join(" ", args, 1, args.Length - 1);
            command += " " + extraArgs;
        }

        Console.WriteLine($"> {command}");

        try
        {
            var process = new Process();

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                process.StartInfo.FileName = "cmd.exe";
                process.StartInfo.Arguments = $"/c {command}";
            }
            else
            {
                process.StartInfo.FileName = "/bin/bash";
                process.StartInfo.Arguments = $"-c \"{command}\"";
            }

            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.OutputDataReceived += (s, e) => { if (e.Data != null) Console.WriteLine(e.Data); };
            process.ErrorDataReceived += (s, e) => { if (e.Data != null) Console.Error.WriteLine(e.Data); };

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            process.WaitForExit();

            return process.ExitCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error executing alias '{aliasName}': {ex.Message}");
            return 1;
        }
    }
}
