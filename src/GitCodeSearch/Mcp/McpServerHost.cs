using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using System.Windows;

namespace GitCodeSearch.Mcp;

/// <summary>
/// Hosts the MCP server inside the application. It listens on the loopback interface only.
/// </summary>
internal static class McpServerHost
{
    public const string Route = "/mcp";

    private const string Instructions =
        "Git Code Search searches the user's local git repositories (several repositories and remote branches, without checking them out). " +
        "Use it when something you are looking for - a file, class, function, configuration or usage - is NOT found in the current project/repository, " +
        "or when the question involves other repositories, other branches or commit history. Prefer your own file search for the current project first. " +
        "Workflow: list_repositories for valid repository names, get_current_branch for the branch the user selected in the UI (searches default to HEAD, the branch checked out in each repository - ask the user if another branch should be searched, then pass it as branch), " +
        "search_file_content or search_commit_messages to find matches, then get_file_content to read a file.";

    private static WebApplication? _app;
    private static int _port;

    public static bool IsRunning => _app != null;

    public static int Port => _port;

    /// <summary>
    /// Reason the server could not be started with the last applied settings, if any.
    /// </summary>
    public static string? LastError { get; private set; }

    public static string Url(int port) => $"http://localhost:{port}{Route}";

    /// <summary>
    /// Brings the server in line with the given settings, (re)starting or stopping it when needed.
    /// </summary>
    public static async Task ApplyAsync(bool enabled, int port)
    {
        if (_app != null && (!enabled || port != _port))
        {
            await StopAsync();
        }

        if (enabled && _app == null)
        {
            await StartAsync(port);
        }
    }

    /// <summary>
    /// Same as <see cref="ApplyAsync"/> but reports failures (e.g. port already in use) to the user.
    /// </summary>
    public static async Task TryApplyAsync(bool enabled, int port)
    {
        try
        {
            LastError = null;
            await ApplyAsync(enabled, port);
        }
        catch (Exception ex)
        {
            LastError = ex.Message;
            MessageBox.Show($"Could not start the MCP server on port {port}: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    public static async Task StopAsync()
    {
        var app = _app;
        _app = null;

        if (app != null)
        {
            await app.StopAsync();
            await app.DisposeAsync();
        }
    }

    private static async Task StartAsync(int port)
    {
        var builder = WebApplication.CreateSlimBuilder();
        builder.Logging.ClearProviders();
        builder.WebHost.ConfigureKestrel(options => options.ListenLocalhost(port));

        builder.Services
            .AddMcpServer(options => options.ServerInstructions = Instructions)
            .WithHttpTransport(options => options.Stateless = true)
            .WithToolsFromAssembly();

        var app = builder.Build();
        app.MapMcp(Route);

        try
        {
            await app.StartAsync();
        }
        catch (Exception)
        {
            await app.DisposeAsync();
            throw;
        }

        _app = app;
        _port = port;
    }
}
