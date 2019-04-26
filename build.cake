#tool nuget:?package=NUnit.ConsoleRunner&version=3.4.0
//////////////////////////////////////////////////////////////////////
// ARGUMENTS
//////////////////////////////////////////////////////////////////////

var target = Argument("target", "Default");
var tag = Argument("tag", "cake");
var configuration = Argument("configuration", "Release");

Information($"Running target {target} in configuration {configuration}");

//////////////////////////////////////////////////////////////////////
// PREPARATION
//////////////////////////////////////////////////////////////////////

// Define directories.
var distDirectory = Directory("./dist");

//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

Task("Clean")
    .Does(() =>
    {
        CleanDirectory(distDirectory);
    });

Task("Restore")
    .Does(() =>
    {
        DotNetCoreRestore();
    });

Task("Build")
    .Does(() =>
    {
      DotNetCoreBuild(".",
            new DotNetCoreBuildSettings()
            {
                Configuration = configuration,
//                ArgumentCustomization = args => args.Append("--no-restore"),
            });
    });

Task("Test")
    .Does(() =>
    {
        var projects = GetFiles("./test/**/*.csproj");
        foreach(var project in projects)
        {
            Information("Testing project " + project);
            DotNetCoreTest(
                project.ToString(),
                new DotNetCoreTestSettings()
                {
                    Configuration = configuration,
                    NoBuild = true,
                    ArgumentCustomization = args => args.Append("--no-restore"),
                });
        }
    });
    
Task("PublishService")
  .Does(() =>
  {
      var settings = new DotNetCorePublishSettings
      {
          Configuration = configuration,
          OutputDirectory = distDirectory,
          ArgumentCustomization = args => args.Append("--no-restore"),
          VersionSuffix = tag
      };

      DotNetCorePublish("./ReferenceApp.csproj", settings);
  });

//////////////////////////////////////////////////////////////////////
// TASK TARGETS
//////////////////////////////////////////////////////////////////////

Task("Default")
    .IsDependentOn("Clean")
    .IsDependentOn("Restore")
    .IsDependentOn("Build")
    .IsDependentOn("Test");

Task("Publish")
    .IsDependentOn("Default")
    .IsDependentOn("PublishService");

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);
