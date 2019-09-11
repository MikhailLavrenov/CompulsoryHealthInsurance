using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PatientsFomsRepository.Infrastructure
{
    public class StatusBar : BindableBase, IStatusBar
    {
        private string statusText;

        public string StatusText { get => statusText; set => SetProperty(ref statusText, value); }
    }
}
