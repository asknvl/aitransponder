using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace botplatform.Models.diagnostics
{
    public interface IDiagnosticsObservable
    {
        void Add(IDiagnosticsResulter observer);
        void Remove(IDiagnosticsResulter observer);
    }
}
