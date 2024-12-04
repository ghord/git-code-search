using GitCodeSearch.Model;
using GitCodeSearch.Search.Result;
using GitCodeSearch.ViewModels;
using Microsoft.Web.WebView2.Core;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Web;
using System.Windows;
using System.Windows.Controls;

#pragma warning disable CA1416 // Validate platform compatibility

namespace GitCodeSearch.Views
{
    /// <summary>
    /// Interaction logic for PreviewView.xaml
    /// </summary>
    public partial class PreviewView : UserControl
    {
        public PreviewView()
        {
            InitializeComponent();
            InitializeAsync();
        }

        private CoreWebView2Environment? _environment;

        private async void InitializeAsync()
        {
            _environment = await CoreWebView2Environment.CreateAsync(userDataFolder: Path.Combine(Path.GetTempPath(), "GitCodeSearch"));
            await WebView.EnsureCoreWebView2Async(_environment);

            WebView.NavigationCompleted += async (o, e) => await LoadContentToEditor();
            WebView.Source = new Uri(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Monaco\index.html"));
        }

        private async void UserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            await LoadContentToEditor();
        }

        private async Task LoadContentToEditor()
        {
            if (WebView.CoreWebView2 != null)
            {
                string javaScript = CreateJavascriptToLoadContent();
                await WebView.ExecuteScriptAsync(javaScript);
            }
        }

        private string CreateJavascriptToLoadContent()
        {
            PreviewViewModel viewModel = (PreviewViewModel)DataContext;
            FileContentSearchResult searchResult = viewModel.SearchResult;

            string language = DetectLanguageFromFileName(searchResult.FullPath);
            string content = HttpUtility.JavaScriptStringEncode(viewModel.Content);

            return $@"loadContentToEditor('{language}', {searchResult.Line}, {searchResult.Column}, 
                                           {searchResult.Expression.Length}, '{Settings.Current.PreviewTheme}', '{content}')";
        }

        private static string DetectLanguageFromFileName(string fullPath)
        {
            var extension = Path.GetExtension(fullPath);
            switch (extension)
            {
                case ".cs":
                    return "csharp";

                case ".js":
                    return "javascript";

                case ".css":
                    return "css";

                case ".sql":
                    return "sql";

                case ".py":
                    return "python";

                case ".java":
                    return "java";

                case ".cpp":
                case ".h":
                    return "cpp";

                case ".ps1":
                    return "powershell";

                case ".ts":
                    return "typescript";

                case ".build":
                case ".xml":
                case ".xsl":
                case ".ism":
                case ".csproj":
                case ".dcproj":
                case ".nuspec":
                case ".include":
                case ".resx":
                case ".targets":
                case ".wsdl":
                    return "xml";

                case ".html":
                    return "html";

                case ".md":
                    return "markdown";

                case ".bat":
                case ".cmd":
                    return "bat";

                case ".json":
                    return "json";

                case ".yml":
                case ".yaml":
                    return "yaml";

                case ".sh":
                    return "shell";

                case "":
                    if(Path.GetFileNameWithoutExtension(fullPath).Equals("Dockerfile", StringComparison.OrdinalIgnoreCase))
                        return "dockerfile";
                    else
                        return "plaintext";

                default:
                    return "plaintext";
            }
        }
    }
}
