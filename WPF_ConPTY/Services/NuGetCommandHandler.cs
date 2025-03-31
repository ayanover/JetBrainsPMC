using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using WPF_ConPTY.Services.Interfaces;

namespace WPF_ConPTY.Services
{
    /// <summary>
    /// Handler for NuGet package related commands
    /// </summary>
    public class NuGetCommandHandler : INuGetCommandHandler
    {
        private readonly HttpClient _httpClient;

        public string CommandPrefix => "get-packages";

        public NuGetCommandHandler(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("NuGet");
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "YourApp Terminal");
        }

        public async Task<CommandResult> HandleCommandAsync(string command)
        {
            try
            {
                string query = command.Substring(CommandPrefix.Length).Trim();

                if (string.IsNullOrWhiteSpace(query))
                {
                    return new CommandResult
                    {
                        Handled = true,
                        Output = "Usage: get-packages [package name]\r\n"
                    };
                }

                var packages = await SearchPackagesAsync(query);
                return new CommandResult
                {
                    Handled = true,
                    Output = FormatPackages(packages)
                };
            }
            catch (Exception ex)
            {
                return new CommandResult
                {
                    Handled = true,
                    Output = $"Error searching for packages: {ex.Message}\r\n"
                };
            }
        }

        private async Task<List<NuGetPackage>> SearchPackagesAsync(string query)
        {
            // Use the NuGet API to search for packages
            string url = $"https://azuresearch-usnc.nuget.org/query?q={Uri.EscapeDataString(query)}&take=5";

            var response = await _httpClient.GetStringAsync(url);
            var searchResult = JsonSerializer.Deserialize<NuGetSearchResult>(response, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return searchResult?.Data ?? new List<NuGetPackage>();
        }

        private string FormatPackages(List<NuGetPackage> packages)
        {
            if (packages.Count == 0)
            {
                return "No packages found.\r\n";
            }

            var sb = new StringBuilder();
            sb.AppendLine("Packages found:");
            sb.AppendLine();

            foreach (var package in packages)
            {
                sb.AppendLine($"Id: {package.Id}");
                sb.AppendLine($"Version: {package.Version}");
                sb.AppendLine($"Description: {package.Description}");
                sb.AppendLine($"Downloads: {package.TotalDownloads:N0}");
                sb.AppendLine();
            }

            return sb.ToString();
        }

        // Models for NuGet API response
        private class NuGetSearchResult
        {
            public List<NuGetPackage> Data { get; set; }
        }
    }

    public class NuGetPackage
    {
        public string Id { get; set; }
        public string Version { get; set; }
        public string Description { get; set; }
        public long TotalDownloads { get; set; }
    }
}