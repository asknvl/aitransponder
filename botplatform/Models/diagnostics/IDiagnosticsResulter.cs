using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace botplatform.Models.diagnostics
{
    public interface IDiagnosticsResulter
    {
        Task<DiagnosticsResult> GetDiagnosticsResult();
    }

    public class DiagnosticsResult
    {
        public string Geotag { get; set; }
        public bool isOk { get; set; } = true;
        public List<string> errorsList { get; set; } = new();

        public string GetErrorsDescription()
        {
            string res = string.Empty;
            if (!isOk && errorsList.Count > 0)
            {
                foreach (var error in errorsList)
                {
                    res += $"{error}\n";
                }
            }
            return res;

        }

    }
}
