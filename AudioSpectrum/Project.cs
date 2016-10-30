using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AudioSpectrum
{
    public class Project
    {
        public string ProjectName { get; set; }

        private string _projectPath;

        public RackArrayControl RackArrayControl { get; set; }

        public Project(string projectPath)
        {
            RackArrayControl = new RackArrayControl();
            _projectPath = projectPath;
            var sp = projectPath.Split('\\');
            ProjectName = sp.Length > 0 ? sp[sp.Length - 1].Split('.')[0] : "Invalid Project Name".Split('.')[0];
        }

        public void Close()
        {
            
        }
    }
}
