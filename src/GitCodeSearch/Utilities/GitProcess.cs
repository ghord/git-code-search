using GitCodeSearch.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GitCodeSearch.Utilities;

public static class GitProcess
{
    public static async IAsyncEnumerable<string> RunLinesAsync(Repository repository, IEnumerable<string> arguments, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        using var process = StartProcess(repository, arguments, cancellationToken);

        if (process == null)
            yield break;

        var reader = process.StandardOutput;

        while (await reader.ReadLineAsync(cancellationToken) is string line)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                process.Kill();
                yield break;
            }

            yield return line;
        }

        await process.WaitForExitAsync(cancellationToken);

        if(process.ExitCode != 0)
        {
            yield return process.StandardError.ReadToEnd();
        }
    }

    public static async Task<string> RunAsync(Repository repository, IEnumerable<string> arguments, CancellationToken cancellationToken = default)
    {
        using var process = StartProcess(repository, arguments, cancellationToken);

        if (process == null)
        {
            return string.Empty;
        }

        string result = await process.StandardOutput.ReadToEndAsync(cancellationToken);

        await process.WaitForExitAsync(cancellationToken);

        return result;
    }

    public static async Task RunVoidAsync(Repository repository, IEnumerable<string> arguments, CancellationToken cancellationToken = default)
    {
        using var process = StartProcess(repository, arguments, cancellationToken);

        if (process != null)
        {
            await process.WaitForExitAsync(cancellationToken);
        }
    }

    private static Process? StartProcess(Repository repository, IEnumerable<string> arguments, CancellationToken cancellationToken)
    {
        var psi = new ProcessStartInfo("git", arguments)
        {
            WorkingDirectory = repository.Path,
            StandardOutputEncoding = Encoding.UTF8,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };

        var process = Process.Start(psi);
        if (process == null)
        {
            return null;
        }

        cancellationToken.Register(() =>
        {
            try
            {
                process.Kill();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to kill git process: {ex.Message}");
            }
        });

        return process;
    }
}
