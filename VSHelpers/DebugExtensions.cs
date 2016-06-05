using EnvDTE;
using System;
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