﻿using System.Linq;
using System.Management.Automation;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using Octokit;
using XpandPwsh.CmdLets;

namespace XpandPwsh.Cmdlets.GitHub.UpdateGitHubIssue{
    [CmdletBinding(SupportsShouldProcess = true)]
    [Cmdlet(VerbsData.Update, "GitHubIssue",SupportsShouldProcess = true)]
    [OutputType(typeof(Issue))]
    public class UpdateGitHubIssue : GitHubCmdlet{
        [Parameter(Mandatory = true,ValueFromPipeline = true)]
        public int IssueNumber{ get; set; }
        [Parameter(Mandatory = true)]
        public string Repository{ get; set; }
        [Parameter]
        public string MileStoneTitle{ get; set; }
        [Parameter]
        public ItemState? State{ get; set; }
        [Parameter]
        public string[] Labels{ get; set; }

        protected override async Task ProcessRecordAsync(){
            var issueUpdate = new IssueUpdate();
            var repository = await GitHubClient.Repository.GetForOrg(Organization, Repository);
            if (MileStoneTitle!=null){
                var milestone = await GitHubClient.Issue.Milestone.GetAllForRepository(repository.Id).ToObservable().SelectMany(list => list).FirstAsync(_ => _.Title==MileStoneTitle);
                issueUpdate.Milestone = milestone.Number;
            }

            if (State.HasValue){
                issueUpdate.State=State;
            }

            if (Labels != null){
                var issue = await GitHubClient.Repository.GetForOrg(Organization, Repository)
                    .SelectMany(_ => GitHubClient.Issue.Get(_.Id, IssueNumber));
                foreach (var label in Labels.Concat(issue.Labels.Select(label => label.Name))){
                    issueUpdate.AddLabel(label);        
                }
            }
            
            await GitHubClient.Issue.Update(repository.Id, IssueNumber, issueUpdate)
                .ToObservable()
                .WriteObject(this)
                .HandleErrors(this);
        }



    }

}