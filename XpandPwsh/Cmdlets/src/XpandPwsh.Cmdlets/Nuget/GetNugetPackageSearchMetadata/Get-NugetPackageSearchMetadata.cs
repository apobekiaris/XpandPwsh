using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading;
using System.Threading.Tasks;
using NuGet.Common;
using NuGet.Configuration;
using NuGet.Protocol.Core.Types;

namespace XpandPwsh.Cmdlets.Nuget.GetNugetPackageSearchMetadata{
    [Cmdlet(VerbsCommon.Get, "NugetPackageSearchMetadata")]
    [CmdletBinding()]
    public  class GetNugetPackageSearchMetadata : NugetCmdlet{
        [Parameter(Position = 0, ValueFromPipeline = true)]
        public string Name{ get; set; }

        [Parameter(Position = 1,Mandatory = true)]
        public string Source{ get; set; } 

        [Parameter]
        public SwitchParameter IncludePrerelease{ get; set; } = new SwitchParameter(false);

        [Parameter]
        public SwitchParameter IncludeUnlisted{ get; set; } = new SwitchParameter(false);

        [Parameter]
        public SwitchParameter AllVersions{ get; set; } = new SwitchParameter(false);

        [Parameter]
        public string[] Versions{ get; set; }


        protected override async Task ProcessRecordAsync(){
            var listPackages = Name != null ? Observable.Return(Name):Providers.ListPackages(Source);
            var metaData = listPackages
                .SelectMany(package => SelectPackages(package, Providers))
                .Where(metadata => metadata!=null)
                .HandleErrors(this)
                .Distinct(new MetadataComparer()).Replay().AutoConnect();
            var searchMetadata = await metaData.DefaultIfEmpty();
            
            if (searchMetadata==null)
                return;
            var packageSearchMetadatas = metaData.ToEnumerable().ToArray()
                .OrderByDescending(_ => _.Identity.Version.Version).ToArray();
            if (!AllVersions && Versions == null){
                WriteObject(packageSearchMetadatas.OrderByDescending(metadata => metadata.Identity.Version.Version).FirstOrDefault());
                return;
            }
            foreach (var packageSearchMetadata in packageSearchMetadatas.Where(VersionMatch)){
                
                WriteObject(packageSearchMetadata);
            }
        }

        private bool VersionMatch(IPackageSearchMetadata metadata){
            if (Versions == null)
                return true;
            return Versions.Contains(metadata.Identity.Version.ToString());
        }

        public class MetadataComparer : IEqualityComparer<IPackageSearchMetadata>{
            public bool Equals(IPackageSearchMetadata x, IPackageSearchMetadata y){
                return x?.Identity.Id == y?.Identity.Id && x?.Identity.Version.Version==y?.Identity.Version.Version;
            }

            public int GetHashCode(IPackageSearchMetadata obj){
                return $"{obj.Identity.Id}{obj.Identity.Version.Version}".GetHashCode();
            }
        }

        private IObservable<IPackageSearchMetadata> SelectPackages(string s, List<Lazy<INuGetResourceProvider>> providers){
            return Source.Split(';').ToObservable().SelectMany(source => PackageSourceSearchMetadatas(source, s,providers));
        }

        

        private IObservable<IPackageSearchMetadata> PackageSourceSearchMetadatas(string source,string name,
            List<Lazy<INuGetResourceProvider>> providers){
            var metadatas = new SourceRepository(new PackageSource(source), providers)
                .GetResourceAsync<PackageMetadataResource>().ToObservable()
                .SelectMany(resource => resource
                    .GetMetadataAsync(name, IncludePrerelease, IncludeUnlisted, NullLogger.Instance,CancellationToken.None).ToObservable()
                    .SelectMany(enumerable => enumerable.ToArray())
                );
            return metadatas;
        }

    }
}