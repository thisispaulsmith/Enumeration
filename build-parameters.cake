#load "./build-version.cake"

public class BuildParameters
{
	public string Target { get; private set; }
	public string Configuration { get; private set; }
	public bool IsLocalBuild { get; private set; }
    public string GitUser { get; private set; }
    public string GitPassword { get; private set;  }
	public BuildVersion Version { get; private set; }

	public bool ShouldPublish
    {
        get
        {
            return !IsLocalBuild && (Version.IsProduction || Version.IsBeta);
        }
    }

	public bool ShouldPublishToMyGet
    {
        get
        {
            return !IsLocalBuild && (Version.IsProduction || Version.IsBeta || Version.IsDevelopment);
        }
    }

	public void Initialize(ICakeContext context)
    {
		Version = BuildVersion.Calculate(context, this);
	}

	public static BuildParameters GetParameters(ICakeContext context)
    {
		if (context == null)
        {
            throw new ArgumentNullException("context");
        }

		var target = context.Argument("target", "Default");
        var buildSystem = context.BuildSystem();

		return new BuildParameters {
            Target = target,
            Configuration = context.Argument("configuration", "Release"),
            IsLocalBuild = buildSystem.IsLocalBuild,
            GitUser = context.EnvironmentVariable("GITHUB_USERNAME")
		};
	}
}