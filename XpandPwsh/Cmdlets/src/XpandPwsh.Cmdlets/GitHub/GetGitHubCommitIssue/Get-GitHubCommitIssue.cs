﻿using System;
using System.Linq;
using System.Management.Automation;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using ImpromptuInterface;
using Octokit;
using XpandPwsh.CmdLets;

namespace XpandPwsh.Cmdlets.GitHub.GetGitHubCommitIssue{
    [CmdletBinding]
    [Cmdlet(VerbsCommon.Get, "GitHubCommitIssue")]
    [OutputType(typeof(ICommitIssues))]
    public class GetGitHubCommitIssues : GitHubCmdlet{
        

        [Parameter(Mandatory = true)]
        public string Repository1{ get; set; } 
        [Parameter(Mandatory = true)]
        public string Repository2{ get; set; } 
        [Parameter]
        public string Branch{ get; set; } 
        [Parameter]
        public DateTimeOffset? Since{ get; set; }
        [Parameter]
        public DateTimeOffset? Until{ get; set; }
        [Parameter]
        public ItemStateFilter ItemStateFilter{ get; set; }
        protected override async Task BeginProcessingAsync(){
            await base.BeginProcessingAsync();
            if (!Since.HasValue){
                ItemStateFilter=ItemStateFilter.All;
                var release = await GitHubClient.Repository.GetForOrg(Organization, Repository1)
                    .SelectMany(repository => GitHubClient.Repository.Release.GetAll(repository.Id))
                    .Select(list => list.First(_ => !_.Draft));
                Since = release.PublishedAt;
                WriteVerbose($"Last {Repository1} release {release.Name} was at {release.PublishedAt}");
            }
        }

        protected override Task ProcessRecordAsync(){
            return GitHubClient.CommitIssues(Organization, Repository1, Repository2,Since,Branch,ItemStateFilter,Until)
                .Select(_ => _.commitIssues.Select(tuple => (_.repoTuple.repo1,_.repoTuple.repo2,tuple.commit,tuple.issues).ToClass().ActLike<ICommitIssues>()))
                .HandleErrors(this,Repository1)
                .WriteObject(this)
                .ToTask();
        }
        

        
    }
    public interface ICommitIssues{
        Repository Repository1{ get; }
        Repository Repository2{ get; }
        Issue[] Issues{ get; }
        GitHubCommit GitHubCommit{ get; set; }
    }
}