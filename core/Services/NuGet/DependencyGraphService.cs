﻿using Newtonsoft.Json.Linq;
using NuGet.ProjectModel;
using System.Collections.Generic;
using NuGet.LibraryModel;
using DotnetStatus.Core.Models;
using Core.Services;

namespace DotnetStatus.Core.Services.NuGet
{
    public class DependencyGraphService : IDependencyGraphService
    {
        private readonly IPackageStatusStore _packageStatusStore;
        private readonly ITypedReader _typedReader;

        public DependencyGraphService(
            IPackageStatusStore packageStatusStore, 
            ITypedReader typedReader)
        {
            _packageStatusStore = packageStatusStore;
            _typedReader = typedReader;
        }

        public List<ProjectResult> GetProjectResults(string dependencyGraphPath)
        {
            var dg = GetDependencyGraph(dependencyGraphPath);

            var projectResults = new List<ProjectResult>();
            foreach (var proj in dg.Projects)
                projectResults.Add(GetProjectResult(proj));

            return projectResults;
        }

        private DependencyGraphSpec GetDependencyGraph(string path)
        {
            var dg = _typedReader.ReadAt<JObject>(path);
            return new DependencyGraphSpec(dg);
        }

        private ProjectResult GetProjectResult(PackageSpec proj)
        {
            var projectResult = new ProjectResult
            {
                Name = proj.Name
            };

            var sources = proj.RestoreMetadata.Sources;
            _packageStatusStore.ReloadSources(sources);

            foreach (var fw in proj.TargetFrameworks)
                projectResult.Frameworks.Add(GetFrameworkResult(fw));

            return projectResult;
        }

        private FrameworkResult GetFrameworkResult(TargetFrameworkInformation fw)
        {
            var frameworkResult = new FrameworkResult
            {
                Name = fw.FrameworkName.Framework
            };

            foreach (var dep in fw.Dependencies)
                frameworkResult.Packages.Add(GetPackageResult(dep));

            return frameworkResult;
        }

        private PackageResult GetPackageResult(LibraryDependency dep)
        {
            var vr = dep.LibraryRange.VersionRange;
            var nugetVersion = _packageStatusStore.GetStatus(dep.Name);
            var result = new PackageResult
            {
                Name = dep.Name,
                CurrentVersion = vr.OriginalString,
                LatestStableVersion = new PackageVersion
                {
                    Source = nugetVersion.LatestStableSource,
                    Version = nugetVersion.LatestStable.ToString()
                },
                LatestVersion = new PackageVersion
                {
                    Source = nugetVersion.LatestSource,
                    Version = nugetVersion.Latest.ToString()
                },
                AutoReferenced = dep.AutoReferenced
            };

            var resolved = vr.FindBestMatch(nugetVersion.AllVersions);

            if (resolved == nugetVersion.Latest)
            {
                result.IsUpToDate = true;
                result.ResolvedVersion = result.LatestVersion;
            }
            else if (resolved == nugetVersion.LatestStable)
            {
                result.IsUpToDate = true;
                result.ResolvedVersion = result.LatestStableVersion;
            }
            else
            {
                result.IsUpToDate = false;
            }

            return result;
        }
    }
}
