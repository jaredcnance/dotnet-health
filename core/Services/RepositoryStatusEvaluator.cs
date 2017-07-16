﻿using DotnetStatus.Core.Configuration;
using DotnetStatus.Core.Data;
using DotnetStatus.Core.Models;
using DotnetStatus.Core.Services.NuGet;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;

namespace DotnetStatus.Core.Services
{
    public class RepositoryStatusEvaluator : IRepositoryStatusEvaluator
    {
        private readonly ITransientGitService _gitService;
        private readonly IRestoreService _restoreService;
        private readonly IDependencyGraphService _dependencyGraphService;
        private readonly string _dgFileName;
        private readonly IRepositoryResultPersistence _repository;

        public RepositoryStatusEvaluator(
            ITransientGitService transientGitService,
            IRestoreService restoreService,
            IDependencyGraphService dependencyGraphService,
            IOptions<WorkerConfiguration> options,
            IRepositoryResultPersistence repository)
        {
            _gitService = transientGitService;
            _restoreService = restoreService;
            _dependencyGraphService = dependencyGraphService;
            _dgFileName = options.Value.DependencyGraphFileName;
            _repository = repository;
        }

        public async Task<RepositoryResult> EvaluateAsync(string repositoryUrl)
        {
            var repoPath = _gitService.GetSource(repositoryUrl);
            var status = _restoreService.Restore(repoPath);

            if (status.Success == false)
                return await GetFailedResultAsync(repositoryUrl, status);

            var dependencyGraphPath = $"{repoPath}/{_dgFileName}";
            var projectResults = _dependencyGraphService.GetProjectResults(dependencyGraphPath);

            var result = new RepositoryResult(repositoryUrl, status, projectResults);

            await _repository.SaveAsync(result);

            return result;
        }

        private async Task<RepositoryResult> GetFailedResultAsync(string repositoryUrl, RestoreStatus status)
        {
            var failedResult = new RepositoryResult(repositoryUrl, status);

            await _repository.SaveAsync(failedResult);

            return failedResult;
        }
    }
}