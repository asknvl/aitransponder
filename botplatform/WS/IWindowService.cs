using botplatform.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace botplatform.WS
{
    public interface IWindowService
    {
        void ShowWindow(LifeCycleViewModelBase vm);
        void ShowDialog(LifeCycleViewModelBase vm);
    }
}
