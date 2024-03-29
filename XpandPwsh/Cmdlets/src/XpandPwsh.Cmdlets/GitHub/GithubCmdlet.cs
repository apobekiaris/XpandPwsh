﻿using System.Management.Automation;
using System.Threading.Tasks;
using Octokit;
using XpandPwsh.CmdLets;

namespace XpandPwsh.Cmdlets.GitHub{
    public abstract class GitHubCmdlet:XpandCmdlet{
        
        [Parameter(Mandatory = true)]
        public string Owner{ get; set; } 
        [Parameter(Mandatory = true)]
        public string Organization{ get; set; } 
        [Parameter(Mandatory = true)]
        public string Pass{ get; set; }

        protected override Task BeginProcessingAsync(){
            var task = base.BeginProcessingAsync();
            GitHubClient = OctokitExtensions.CreateClient(Owner, Pass, ActivityName);
            return task;
        }

        public GitHubClient GitHubClient{ get; private set; }
    }
}