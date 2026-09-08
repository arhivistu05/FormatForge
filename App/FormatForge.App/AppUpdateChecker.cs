using System.Net.Http;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace FormatForge.App;

internal sealed record AppUpdateInfo(bool IsConfigured, bool IsUpdateAvailable, string LatestVersion, string ReleaseUrl, string Message);

internal static class AppUpdateChecker
{
    public static async Task<AppUpdateInfo> CheckLatestReleaseAsync(CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(AppInfo.LatestReleaseApiUrl))
        {
            return new AppUpdateInfo(false, false, string.Empty, string.Empty, "Update checking is not configured.");
        }

        try
        {
            using HttpClient client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(6);
            client.DefaultRequestHeaders.UserAgent.ParseAdd(AppInfo.ProductName + "/" + AppInfo.Version);
            using HttpResponseMessage response = await client.GetAsync(AppInfo.LatestReleaseApiUrl, cancellationToken);
            response.EnsureSuccessStatusCode();

            string json = await response.Content.ReadAsStringAsync(cancellationToken);
            using JsonDocument document = JsonDocument.Parse(json);
            JsonElement root = document.RootElement;

            string latestVersion = GetString(root, "tag_name");
            if (string.IsNullOrWhiteSpace(latestVersion))
            {
                latestVersion = GetString(root, "name");
            }

            string releaseUrl = GetString(root, "html_url");
            if (string.IsNullOrWhiteSpace(releaseUrl))
            {
                releaseUrl = AppInfo.ReleasesPageUrl;
            }

            bool updateAvailable = IsNewerVersion(latestVersion, AppInfo.Version);
            return new AppUpdateInfo(
                true,
                updateAvailable,
                latestVersion,
                releaseUrl,
                updateAvailable ? "A new FormatForge version is available." : "FormatForge is up to date.");
        }
        catch (Exception ex)
        {
            return new AppUpdateInfo(true, false, string.Empty, string.Empty, "Update check failed: " + ex.Message);
        }
    }

    private static string GetString(JsonElement element, string propertyName)
    {
        return element.TryGetProperty(propertyName, out JsonElement property) && property.ValueKind == JsonValueKind.String
            ? property.GetString() ?? string.Empty
            : string.Empty;
    }

    private static bool IsNewerVersion(string latestVersion, string currentVersion)
    {
        string normalizedLatest = NormalizeVersion(latestVersion);
        string normalizedCurrent = NormalizeVersion(currentVersion);
        if (Version.TryParse(normalizedLatest, out Version? latest) &&
            Version.TryParse(normalizedCurrent, out Version? current))
        {
            return latest > current;
        }

        return !string.IsNullOrWhiteSpace(latestVersion) &&
               !string.Equals(latestVersion.Trim(), currentVersion.Trim(), StringComparison.OrdinalIgnoreCase);
    }

    private static string NormalizeVersion(string version)
    {
        if (string.IsNullOrWhiteSpace(version))
        {
            return string.Empty;
        }

        string value = version.Trim();
        if (value.StartsWith("v", StringComparison.OrdinalIgnoreCase))
        {
            value = value[1..];
        }

        Match match = Regex.Match(value, @"\d+(\.\d+){0,3}");
        return match.Success ? match.Value : value;
    }
}
