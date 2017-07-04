using System;
using System.Threading.Tasks;
using DotnetStatus.Services.Http;
using Semver;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace DotnetStatus.Services
{
    public class NuGetStatusService : IPackageStatusService, IDisposable
    {
        private readonly IXmlClient _xmlClient;
        private readonly IJsonClient _jsonClient;
        private readonly ILogger<NuGetStatusService> _log;

        public NuGetStatusService(
            IXmlClient xmlClient,
            IJsonClient jsonClient,
            ILogger<NuGetStatusService> log)
        {
            _xmlClient = xmlClient;
            _jsonClient = jsonClient;
            _log = log;
        }

        public async Task<Result> GetStatusAsync(string csprojUrl)
        {
            var csproj = await _xmlClient.GetAsync<Csproj>(csprojUrl);

            if (csproj == null)
            {
                _log.LogInformation($"Could not locate {csprojUrl}");
                return null;
            }

            var result = new Result();
            foreach (var package in csproj.ItemGroups.SelectMany(ig => ig.PackageReferences))
            {
                var uri = $"https://api.nuget.org/v3-flatcontainer/{package.Include}/index.json";

                var nuget = await _jsonClient.GetAsync<Nuget>(uri);
                if (nuget == null)
                {
                    _log.LogInformation($"Could not locate {uri} on NuGet");
                    return null;
                }

                var latestStable = GetLatestStableVersion(nuget.Versions);
                result.Packages.Add(new PackageResult
                {
                    Name = package.Include,
                    CurrentVersion = package.Version,
                    LatestVersion = nuget.Versions.Last(),
                    LatestStableVersion = latestStable
                });
            }

            return result;
        }

        private string GetLatestStableVersion(List<string> versions)
        {
            SemVersion latest = "0.0";
            foreach (var version in versions)
            {
                var semVersion = SemVersion.Parse(version);
                if (semVersion > latest && semVersion.Prerelease == string.Empty)
                    latest = semVersion;
            }
            return latest.ToString();
        }

        public void Dispose()
        {
            _xmlClient.Dispose();
            _jsonClient.Dispose();
        }
    }
}
