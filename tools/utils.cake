using System.Diagnostics;

public void StartProcess(string executablePath, string arguments, string standardInput)
{
    var args = new ProcessArgumentBuilder().Append(arguments);
    using (var process = new Process                              
    {
             StartInfo = new ProcessStartInfo(executablePath, args.Render())
                {
                    RedirectStandardInput = true,
                    UseShellExecute = false
                }
    })
    {
        process.Start();
        process.StandardInput.WriteLine(standardInput);
        process.StandardInput.Close();
        process.WaitForExit();
    }
}

public void ClearProjectDependenciesForProject(string project)
{
    try 
    {
        XmlPoke(project, "//*[local-name()='ProjectReference']", null);
    } 
    catch (Exception e)
    {
        Information(e.Message);
    }
}

public void ZipDirectory(string sourceDirectoryPath, string sourceDirectoryName, string zipFilePath)
{
    var tempDirectory = Directory($"temp{new Random().Next()}");
    CreateDirectory(tempDirectory);
    MoveDirectory(sourceDirectoryPath, $"{tempDirectory.Path}/{sourceDirectoryName}");
    Zip(tempDirectory, zipFilePath);
    DeleteDirectory(tempDirectory, new DeleteDirectorySettings { Recursive = true, Force = true });
}