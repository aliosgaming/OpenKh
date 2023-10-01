using Octokit;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace OpenKh.Tools.ModsManager.Services
{
    public class OpenkhUpdateCheckerService
    {
        public record CheckResult(bool HasUpdate, string CurrentVersion, string NewVersion, string DownloadZipUrl);

        private static readonly Regex _validTag = new Regex("^release2-(?<build>\\d+)$");

        public async Task<CheckResult> CheckAsync(CancellationToken cancellation)
        {
            var gitClient = new GitHubClient(new ProductHeaderValue("OpenKh.Tools.ModsManager"));
            var releases = gitClient.Repository.Release.GetLatest(owner: "aliosgaming", name: "OpenKh").Result;
            if (releases != null)
            {
                var remoteReleaseTag = releases.TagName;

                var localReleaseTagFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "openkh-release");
                var localReleaseTag = File.Exists(localReleaseTagFile)
                    ? File.ReadAllText(localReleaseTagFile)
                    : "(Unknown version)";

                if (localReleaseTag != remoteReleaseTag)
                {
                    return new CheckResult(
                        HasUpdate: true,
                        CurrentVersion: localReleaseTag,
                        NewVersion: remoteReleaseTag,
                        DownloadZipUrl: releases.Url
                    );
                }
            }
            return new CheckResult(
                HasUpdate: false,
                CurrentVersion: "v3.1.0",
                NewVersion: "",
                DownloadZipUrl: ""
            );
        }
    }
}
