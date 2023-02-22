using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OpenKh.Tools.ModsManager.Services
{
    public class OpenkhUpdateProceederService
    {
        public async Task UpdateAsync(string downloadZipUrl, Action<float> progress, CancellationToken cancellation)
        {
            var tempId = Guid.NewGuid().ToString("N");
            var tempZipFile = Path.Combine(Path.GetTempPath(), $"openkh-{tempId}.zip");

            try
            {
                using (var client = new HttpClient())
                {
                    using (var zipOutput = File.Create(tempZipFile))
                    {
                        using (var resp = await client.GetAsync(downloadZipUrl, cancellation))
                        {
                            var maxLen = resp.Content.Headers.ContentLength;
                            var zipInput = await resp.Content.ReadAsStreamAsync();
                            await CopyToAsyncWithProgress(zipInput, zipOutput, maxLen, progress, cancellation);
                        }
                    }
                }

                var tempZipDir = Path.Combine(Path.GetTempPath(), $"openkh-{tempId}");
                Directory.CreateDirectory(tempZipDir);

                try
                {
                    using (var zip = ZipFile.OpenRead(tempZipFile))
                    {
                        zip.ExtractToDirectory(tempZipDir);
                    }

                    var tempBatFile = Path.Combine(Path.GetTempPath(), $"openkh-{tempId}.bat");

                    try
                    {
                        var copyTo = AppDomain.CurrentDomain.BaseDirectory;

                        await CreateBatchFileAsync(
                            tempBatFile: tempBatFile,
                            copyFrom: tempZipDir,
                            copyTo: copyTo,
                            execAfter: $"start \"\" \"{Path.Combine(copyTo, "OpenKh.Tools.ModsManager.exe")}\"" // no enclosing double-quotes!
                        );

                        var process = Process.Start(
                            new ProcessStartInfo(
                                tempBatFile
                            )
                            {
                                UseShellExecute = true,
                            }
                        );
                        process.WaitForExit();
                    }
                    finally
                    {
                        File.Delete(tempBatFile);
                    }
                }
                finally 
                {
                    while (true)
                    {
                        try
                        {
                            Directory.Delete(tempZipDir, true);
                            break;
                        }
                        catch (IOException)
                        {
                            // Directory still exists, wait and try again
                            await Task.Delay(100);
                        }
                    }
                }
            }
            finally
            {
                File.Delete(tempZipFile);
            }
        }

        private async Task CopyToAsyncWithProgress(Stream input, Stream output, long? maxLen, Action<float> progress, CancellationToken cancellation)
        {
            byte[] buffer = new byte[8192];
            var totalTransferred = 0L;
            while (true)
            {
                var read = await input.ReadAsync(buffer, cancellation);
                if (read <= 0)
                {
                    break;
                }
                await output.WriteAsync(buffer.AsMemory(0, read), cancellation);
                totalTransferred += read;
                if (maxLen != null)
                {
                    progress?.Invoke((totalTransferred * 1.0f / maxLen.Value));
                }
            }
        }

        private async Task CreateBatchFileAsync(string tempBatFile, string copyFrom, string copyTo, string execAfter)
        {
            var bat = new StringWriter();
            bat.WriteLine($"xcopy /d /e /y \"{copyFrom}\" \"{copyTo}\" || pause");
            bat.WriteLine($"{execAfter}");
            bat.WriteLine($"exit");
            await File.WriteAllTextAsync(tempBatFile, bat.ToString(), Encoding.Default);
        }
    }
}
