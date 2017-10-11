public class BuildVersion
{
    public GitVersion GitVersionInfo { get; private set; }
    public string Milestone {get;private set;}
    public string CakeVersion { get; private set; }

    public static BuildVersion Calculate(ICakeContext context, BuildParameters parameters)
    {
        if (context == null)
        {
            throw new ArgumentNullException("context");
        }

        context.Information("Calculating Semantic Version");
        if (!parameters.IsLocalBuild)
        {
            context.GitVersion(new GitVersionSettings{
                OutputType = GitVersionOutput.BuildServer
            });
        }

        GitVersion assertedVersions = context.GitVersion(new GitVersionSettings
        {
            OutputType = GitVersionOutput.Json,
        });

        context.Information("Calculated Semantic Version: {0}", assertedVersions.LegacySemVerPadded);
        
        var cakeVersion = typeof(ICakeContext).Assembly.GetName().Version.ToString();

        return new BuildVersion
        {
            GitVersionInfo = assertedVersions,
            Milestone = string.Concat("v", assertedVersions.NuGetVersion),
            CakeVersion = cakeVersion
        };
    }

    public bool IsDevelopment
    {
        get { return GitVersionInfo.BranchName.Equals("develop", StringComparison.CurrentCultureIgnoreCase); }
    }

    public bool IsBeta
    {
        get { return GitVersionInfo.BranchName.StartsWith("release", StringComparison.CurrentCultureIgnoreCase); }
    }

    public bool IsProduction
    {
        get { return GitVersionInfo.BranchName.Equals("master", StringComparison.CurrentCultureIgnoreCase); }
    }
}