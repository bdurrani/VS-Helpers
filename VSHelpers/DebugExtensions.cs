using EnvDTE;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace BD.VSHelpers
{
    public static class DebugExtensions
    {
        [Conditional("DEBUG")]
        public static void ToDebugPrint(this Document document)
        {
            var name = document.Name;
            var fullname = document.FullName;
            var activeDocType = document.Type;
            var res = string.Format("name: {0} fullname: {1} doctype: {2}", name, fullname, activeDocType);
            Debug.WriteLine(res);
        }

        [Conditional("DEBUG")]
        public static void ToDebugPrint(this CodeElement codeElement)
        {
            string name = codeElement.Name;
            var startPoint = codeElement.StartPoint;
            string startPointString = startPoint.ToDebugPrint();
            var kind = codeElement.Kind;
            var location = codeElement.InfoLocation;
            var res = string.Format("name: {0} kind: {1} startpoint:{2} location: {3}", name, kind, startPointString, location);
            Debug.WriteLine(res);
        }

        private static string ToDebugPrint(this TextPoint vpoint)
        {
            int line = vpoint.Line;
            int linelenght = vpoint.LineLength;
            return string.Format("line: {0} linelenth:{1}", line, linelenght);
        }

        [Conditional("DEBUG")]
        public static void ToDebugPrint(this Documents documents)
        {
            Debug.WriteLine("documents count: " + documents.Count);
            for (int i = 1; i < documents.Count + 1; i++)
            {
                var currentDoc = documents.Item(i);
                Debug.WriteLine("document name:" + currentDoc.Name);
            }
        }

        /// <summary>
        /// Print debug into for a collection of projects
        /// </summary>
        /// <param name="projects">projet collection</param>
        [Conditional("DEBUG")]
        public static void ToDebugPrint(this IEnumerable<Project> projects)
        {
            foreach (Project item in projects)
            {
                item.ToDebugPrint();
            }
        }

        /// <summary>
        /// Print debug info for project
        /// </summary>
        /// <param name="project">project to debug</param>
        [Conditional("DEBUG")]
        public static void ToDebugPrint(this EnvDTE.Project project)
        {
            try
            {
                string output;
                if (string.IsNullOrEmpty(project.FullName))
                {
                    output = "Empty project name";
                }
                else
                {
                    // full name: <fully qualified path to project>.csproj
                    // language will be a guid
                    var sb = new StringBuilder();
                    sb.AppendFormat("project name: {0} ", project.FullName);
                    sb.AppendFormat("name: {0} ", project.Name);
                    sb.AppendFormat("code model: {0} ", project.CodeModel != null ? project.CodeModel.Language : "<no codemodel>");
                    sb.AppendFormat("unique name: {0} ", project.UniqueName);

                    var configurationManager = project.ConfigurationManager;
                    if (configurationManager != null)
                    {
                        var activeConfiguration = configurationManager.ActiveConfiguration;
                        var configurationName = activeConfiguration.ConfigurationName;
                        sb.AppendFormat("configuraton name: {0} ", configurationName); 
                    }
                    output = sb.ToString();
                }
                Debug.WriteLine(output);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("exception {0} for project unique name {1}", ex.Message, project.UniqueName);
            }
        }

        [Conditional("DEBUG")]
        public static void ToDebugPrint(this EnvDTE.OutputGroup outputGroup)
        {
            Debug.WriteLine("canonicalname: " + outputGroup.CanonicalName + " description: " + outputGroup.Description + " displayname: " + outputGroup.DisplayName);
        }

        [Conditional("DEBUG")]
        public static void TestCodeElementAtCurrentPoint(VirtualPoint activePoint)
        {
            foreach (vsCMElement item in Enum.GetValues(typeof(vsCMElement)))
            {
                try
                {
                    Debug.WriteLine("testing " + item.ToString());
                    var function = activePoint.CodeElement[item] as CodeFunction;
                    if (function != null)
                    {
                        Debug.WriteLine("found something @: " + item.ToString());
                    }
                }
                catch (Exception)
                {

                }
            }
        }
    }
}