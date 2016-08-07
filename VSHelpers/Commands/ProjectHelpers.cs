using BD.VSHelpers.Data;
using EnvDTE;
using EnvDTE80;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BD.VSHelpers.Commands
{
    public class ProjectHelpers
    {
        /// <summary>
        /// Get build outputs associated with a project
        /// </summary>
        /// <param name="project"></param>
        /// <returns></returns>
        public static IEnumerable<string> GetBuildOutputs(Project project)
        {
            var result = Enumerable.Empty<string>();
            if (project == null)
            {
                return result;
            }

            var configurationManager = project.ConfigurationManager; 
            if (configurationManager == null)
            {
                return result;
            }

            var outputGroups = configurationManager.ActiveConfiguration.OutputGroups.OfType<EnvDTE.OutputGroup>();
            var builtGroup = outputGroups.First(x => x.CanonicalName == "Built");

            result = ((object[])builtGroup.FileURLs).OfType<string>();
            return result;
        }

        /// <summary>
        /// Recursively get all projects associated with a project
        /// </summary>
        /// <param name="project">root project</param>
        /// <param name="result">list of all projects</param>
        public static void GetAllProjectsFromProject(Project project, List<Project> result)
        {
            if (project == null)
            {
                return;
            }

            result.Add(project);

            if (project.ProjectItems == null)
            {
                return;
            }

            foreach (ProjectItem item in project.ProjectItems)
            {
                if (item != null)
                {
                    GetAllProjectsFromProject(item.Object as Project, result);
                }
            }
        }

        public static List<ProjectBuildOutput> GetBuildExecutablesForSolution(EnvDTE80.DTE2 dte)
        { 
            SolutionBuild2 sb = (SolutionBuild2)dte.Solution.SolutionBuild;

            var results = new List<Project>();
            foreach (EnvDTE.Project project in dte.Solution.Projects)
            {
                ProjectHelpers.GetAllProjectsFromProject(project, results);
            }

            List<ProjectBuildOutput> exes = new List<ProjectBuildOutput>();
            foreach (var project in results)
            {
                project.ToDebugPrint();
                if(string.IsNullOrEmpty(project.Name))
                {
                    continue;
                }

                // try to figure out the build outputs of the project
                var fileUrls = ProjectHelpers.GetBuildOutputs(project);
                var executables = fileUrls.Where(x => x.EndsWith(".exe", StringComparison.OrdinalIgnoreCase)).Select(x => new ProjectBuildOutput(project, x));
                exes.AddRange(executables);
            }
            return exes;
        }
    }
}
