using EnvDTE;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace BD.VSHelpers
{
    public static class DebugExtensions
    {
        public static string ToDebugPrint(this Document document)
        {
            var name = document.Name;
            var fullname = document.FullName;
            var activeDocType = document.Type;
            return string.Format("name: {0} fullname: {1} doctype: {2}", name, fullname, activeDocType);
        }

        public static string ToDebugPrint(this CodeElement codeElement)
        {
            string name = codeElement.Name;
            var startPoint = codeElement.StartPoint;
            string startPointString = startPoint.ToDebugPrint();
            var kind = codeElement.Kind;
            var location = codeElement.InfoLocation;

            return string.Format("name: {0} kind: {1} startpoint:{2} location: {3}", name, kind, startPointString, location);
        }

        public static string ToDebugPrint(this TextPoint vpoint)
        {
            int line = vpoint.Line;
            int linelenght = vpoint.LineLength;
            return string.Format("line: {0} linelenth:{1}", line, linelenght);
        }

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
        public static void ToDebugPrint(this EnvDTE.Project project)
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
                output = string.Format("project name: {0} name: {1} code model: {2} unique name: {3}", project.FullName, project.Name, project.CodeModel.Language, project.UniqueName);
            }
            Debug.WriteLine(output);
        }

        public static void ToDebugPrint(this EnvDTE.OutputGroup outputGroup)
        {
            Debug.WriteLine("canonicalname: " + outputGroup.CanonicalName + " description: " + outputGroup.Description + " displayname: " + outputGroup.DisplayName);
        }

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