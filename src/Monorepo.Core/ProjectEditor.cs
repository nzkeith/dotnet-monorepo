using System;
using System.Linq;
using System.Xml;
using System.Xml.XPath;

namespace Monorepo.Core
{
    public class ProjectEditor
    {
        private readonly string _projectPath;

        public ProjectEditor(string projectPath)
        {
            _projectPath = projectPath;
        }

        public void SetVersion(string version)
        {
            var document = new XmlDocument
            {
                PreserveWhitespace = true
            };
            document.Load(_projectPath);

            var navigator = document.CreateNavigator();
            if (navigator == null)
            {
                throw new MonorepoException($"{nameof(navigator)} is unexpectedly null");
            }

            var nodes = navigator.Select("/Project/PropertyGroup/Version").OfType<XPathNavigator>();
            var node = nodes.FirstOrDefault();
            if (node == null)
            {
                throw new MonorepoException($"Version property not found in project {_projectPath}");
            }

            node.SetValue(version);

            document.Save(_projectPath);
        }
    }
}
