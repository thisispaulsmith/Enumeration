// Install addins
#addin "Cake.Git"

// Install tools
#tool "nuget:?package=GitVersion.CommandLine"
#tool "nuget:?package=OctopusTools"
//#tool "nuget:?package=gitlink"

// Load other scripts.
#load "./build-parameters.cake";

using Path = System.IO.Path;

//////////////////////////////////////////////////////////////////////
// PARAMETERS
//////////////////////////////////////////////////////////////////////

BuildParameters parameters = BuildParameters.GetParameters(Context);

var outputDirectory = "./output/";
var solutionPath = "./NetSmith.Enumeration.sln";
var testFolder = "./tests";

///////////////////////////////////////////////////////////////////////////////
// SETUP / TEARDOWN
///////////////////////////////////////////////////////////////////////////////

Setup(context =>
{
	parameters.Initialize(context);
});

//////////////////////////////////////////////////////////////////////
// PRIVATE TASKS
//////////////////////////////////////////////////////////////////////

Task("__Clean")
	.Does(() =>
	{
		if (DirectoryExists(outputDirectory))
	    {
			DeleteDirectory(outputDirectory, recursive:true);
	    }
	    CreateDirectory(outputDirectory);
	});

Task("__Restore")
    .Does(() => 
	{
	    DotNetCoreRestore();
	});

Task("__Build")
    .Does(() => 
	{
        DotNetCoreBuild(solutionPath, new DotNetCoreBuildSettings
		{
			Configuration = parameters.Configuration,
			ArgumentCustomization = args => args.Append("/p:SemVer=" + parameters.Version.GitVersionInfo.NuGetVersion)
		});
    });

Task("__Test")
    .Does(() => 
	{
		var files = GetFiles(Path.Combine(testFolder, "*/*.csproj"));

		foreach(var file in files)
		{
			DotNetCoreTest(file.FullPath, new DotNetCoreTestSettings
			{
				Configuration = parameters.Configuration,
			});
		}
    });

Task("__GitLink")
    .Does(() => 
	{
		//GitLink("./");
	});

Task("__Pack")
	.IsDependentOn("__GitLink")
    .Does(() => 
	{
		DotNetCorePack("./src/NetSmith.Enumeration/NetSmith.Enumeration.csproj", new DotNetCorePackSettings
        {
			Configuration = parameters.Configuration,
            OutputDirectory = outputDirectory,
            NoBuild = true,
			ArgumentCustomization = args => args.Append("/p:SemVer=" + parameters.Version.GitVersionInfo.NuGetVersion)
        });
    });

Task("__PublishMyGet")
	.WithCriteria(() => parameters.ShouldPublishToMyGet)
    .Does(() => 
	{
		// Resolve the API key.
		var apiKey = EnvironmentVariable("MYGET_API_KEY");
	    if(string.IsNullOrEmpty(apiKey)) {
	        throw new InvalidOperationException("Could not resolve MyGet API key.");
	    }
	
	    // Resolve the API url.
	    var apiUrl = EnvironmentVariable("MYGET_API_URL");
	    if(string.IsNullOrEmpty(apiUrl)) {
	        throw new InvalidOperationException("Could not resolve MyGet API url.");
	    }

		PublishPackages(apiUrl, apiKey);
	});

Task("__Publish")
	.WithCriteria(() => parameters.ShouldPublish)
    .Does(() => 
	{
		// Resolve the API key.
		var apiKey = EnvironmentVariable("NUGET_API_KEY");
	    if(string.IsNullOrEmpty(apiKey)) {
	        throw new InvalidOperationException("Could not resolve NuGet API key.");
	    }
	
	    // Resolve the API url.
	    var apiUrl = EnvironmentVariable("NUGET_API_URL");
	    if(string.IsNullOrEmpty(apiUrl)) {
	        throw new InvalidOperationException("Could not resolve NuGet API url.");
	    }

		PublishPackages(apiUrl, apiKey);
	})
	.OnError(exception =>
	{
    	Information("Publish to NuGet failed, but continuing with next Task...");
	});;

Task("__Tag")
	.WithCriteria(() => parameters.Version.IsProduction)
    .Does(() =>
    {
        GitTag(".", parameters.Version.Milestone);
        GitPushRef(".", parameters.GitUser, "", "origin", 
			parameters.Version.Milestone); 
    })
	.OnError(exception =>
	{
    	Information("Tagging failed, but continuing with next Task...");
	});

//////////////////////////////////////////////////////////////////////
// HELPER
//////////////////////////////////////////////////////////////////////

private void PublishPackages(string url, string key)
{
	foreach (var file in GetFiles(outputDirectory + "**/*"))
	{
		AppVeyor.UploadArtifact(file.FullPath);

		// Push the package.
		NuGetPush(file.FullPath, new NuGetPushSettings {
			ApiKey = key,
			Source = url
		});
	}
}

//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

Task("Default")
	.IsDependentOn("__Clean")
	.IsDependentOn("__Restore")
	.IsDependentOn("__Build")
	.IsDependentOn("__Test")
	.IsDependentOn("__Pack")
	.IsDependentOn("__PublishMyGet")
	.IsDependentOn("__Publish")
	.IsDependentOn("__Tag");

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(parameters.Target);