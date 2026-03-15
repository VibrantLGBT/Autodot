using System.Diagnostics;
using System.Text.RegularExpressions;
using System.IO;

namespace Autodot;

public class Autodot
{
    public const int AUTODOT_VERSION_MAJOR = 1;
    public const int AUTODOT_VERSION_MINOR = 0;

    static async Task Main(string[] args)
    {
        Console.WriteLine("Autodot Version: " + AUTODOT_VERSION_MAJOR + "." + AUTODOT_VERSION_MINOR + "\n\n");

        if (!File.Exists("autodot.json"))
        {
            Console.WriteLine("`autodot.json` not found.");
            MessageBox.Show("`autodot.json` not found.", "Error", MessageBoxButtons.Ok, MessageBoxIcon.Warning);
            return;
        }
        
        try
        {
            string json = File.ReadAllText("autodot.json");
            Config? config = System.Text.Json.JsonSerializer.Deserialize<Config>(json);
            if(config == null)
            {
                Console.WriteLine("Failed to parse json.");
                MessageBox.Show("Failed to parse json.", "Error", MessageBoxButtons.Ok, MessageBoxIcon.Warning);
                return;
            }
            string versionTag = config.AutodotVersion;
            if (!string.IsNullOrEmpty(versionTag))
            {
                if (!versionTag.Contains('.'))
                {
                    Console.WriteLine("Malformed `AutodotVersion` tag in json file.");
                    MessageBox.Show("Malformed `AutodotVersion` tag in json file.", "Error", MessageBoxButtons.Ok, MessageBoxIcon.Warning);
                    return;
                } else
                {
                    string[] version = versionTag.Split('.');
                    bool parsed = false;
                    if (int.TryParse(version[0], out int major))
                    {
                        if (int.TryParse(version[1], out int minor))
                        {
                            if(major > AUTODOT_VERSION_MAJOR)
                            {
                                Console.WriteLine("This autodot.json requires a newer version of Autodot.");
                                MessageBox.Show("This autodot.json requires a newer version of Autodot.", "Error", MessageBoxButtons.Ok, MessageBoxIcon.Warning);
                                return;
                            }
                            if(major == AUTODOT_VERSION_MAJOR && minor > AUTODOT_VERSION_MINOR)
                            {
                                Console.WriteLine("This autodot.json requires a newer version of Autodot.");
                                MessageBox.Show("This autodot.json requires a newer version of Autodot.", "Error", MessageBoxButtons.Ok, MessageBoxIcon.Warning);
                                return;
                            }
                            parsed = true;
                        }
                    }
                    if (!parsed)
                    {
                        Console.WriteLine("Malformed `AutodotVersion` tag in json file.");
                        MessageBox.Show("Malformed `AutodotVersion` tag in json file.", "Error", MessageBoxButtons.Ok, MessageBoxIcon.Warning);
                        return;
                    }
                }
            }

            Directory.CreateDirectory(config.DeploymentFolder);
            string bp = config.BinaryPath;
#if LINUX
            if (!string.IsNullOrWhiteSpace(config.BinaryPathLinux)) bp = config.BinaryPathLinux;
#endif
            string binaryPath = Path.Combine(config.DeploymentFolder, bp);
            if (!File.Exists(binaryPath))
            {
                //download!
                string downloaduri = config.RemoteZipUri;
#if LINUX
                if (!string.IsNullOrWhiteSpace(config.RemoteZipUriLinux)) downloaduri = config.RemoteZipUriLinux;
#endif

                Console.WriteLine("Downloading remote file: " + downloaduri);
                MemoryStream download = await DownloadFile(downloaduri);
                //unzip!
                using (var archive = new System.IO.Compression.ZipArchive(download, System.IO.Compression.ZipArchiveMode.Read))
                {
                    foreach(var entry in archive.Entries)
                    {
                        if (string.IsNullOrEmpty(entry.Name)) continue; //skip dirs
                        string outpath = Path.Combine(config.DeploymentFolder, entry.FullName);
                        string? outdir = Path.GetDirectoryName(outpath);
                        if(outdir != null) Directory.CreateDirectory(outdir);
                        using (var entryStream = entry.Open())
                        {
                            using (var fileStream = new FileStream(outpath, FileMode.Create, FileAccess.Write))
                            {
                                await entryStream.CopyToAsync(fileStream);
                            }
                        }
                    }
                }
            }
#if LINUX
            File.SetUnixFileMode(binaryPath, UnixFileMode.UserExecute | UnixFileMode.GroupExecute);
#endif

            if (!File.Exists(binaryPath))
            {
                Console.WriteLine("Binary path not available.");
                MessageBox.Show("Binary path not available.", "Error", MessageBoxButtons.Ok, MessageBoxIcon.Warning);
                return;
            }

            string task = config.DefaultTask;
            if (args.Length > 0) task = args[0];

            if (!config.Tasks.ContainsKey(task))
            {
                Console.WriteLine("Unknown task: " + task);
                MessageBox.Show("Unknown task: " + task, "Error", MessageBoxButtons.Ok, MessageBoxIcon.Warning);
                return;
            }

            string pathArg = "--path " + Path.GetFullPath(config.ProjectFolder);

            string taskLine = config.Tasks[task].Replace("{bp}", binaryPath).Replace("{patharg}", pathArg);
            try
            {
                var (exePath, targs) = ParseCommand(taskLine);

                Process.Start(new ProcessStartInfo
                {
                    FileName = Path.GetFullPath(exePath),
                    Arguments = targs,
                    UseShellExecute = true,
                    WorkingDirectory = Path.GetFullPath(config.DeploymentFolder)
                });
                Console.WriteLine("Task complete: " + task);
                Environment.Exit(0);
            } catch (Exception ex)
            {
                Console.WriteLine("Failed to start task `" + task + "`: " + ex.Message);
                MessageBox.Show("Failed to start task `" + task + "`: " + ex.Message, "Error", MessageBoxButtons.Ok, MessageBoxIcon.Warning);
                return;
            }
        } catch (Exception ex)
        {
            Console.WriteLine("Failed to read autodot.json: " + ex.ToString());
            MessageBox.Show("Failed to read autodot.json: " + ex.ToString(), "Error", MessageBoxButtons.Ok, MessageBoxIcon.Warning);
            return;
        }
    }

    public static async Task<MemoryStream> DownloadFile(string uri)
    {
        using HttpClient httpClient = new HttpClient();
        using HttpResponseMessage response = await httpClient.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead);
        response.EnsureSuccessStatusCode();
        var memoryStream = new MemoryStream();
        using (var responseStream = await response.Content.ReadAsStreamAsync())
        {
            await responseStream.CopyToAsync(memoryStream);
        }
        memoryStream.Position = 0;
        return memoryStream;
    }

    private static (string exePath, string arguments) ParseCommand(string fullCommand)
    {
        // Regex: capture quoted or unquoted executable path
        var match = Regex.Match(fullCommand.Trim(),
            "^\\s*(\"[^\"]+\"|\\S+)(?:\\s+(.*))?$");

        if (!match.Success)
            throw new ArgumentException("Invalid command line format");

        string exe = match.Groups[1].Value.Trim('"'); // strip surrounding quotes
        string args = match.Groups[2].Success ? match.Groups[2].Value : "";

        return (exe, args);
    }
}
