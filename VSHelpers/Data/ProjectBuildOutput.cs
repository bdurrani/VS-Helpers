using EnvDTE;
using System.Diagnostics;

namespace BD.VSHelpers.Data
{
    [DebuggerDisplay("Exe = {BuildExePath}")]
    public class ProjectBuildOutput
    {
        public Project Project { get; private set; }
        public string BuildExePath { get; private set; }

        public ProjectBuildOutput(Project project, string buildOutput)
        {
            Project = project;
            BuildExePath = buildOutput;
        }
    }
}
