using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PatientsFomsRepository.Infrastructure
{
    public class ActiveViewModel : BindableBase, IActiveViewModel
    {
        private string header;
        private string status;

        public string Header { get => header; set => SetProperty(ref header, value); }
        public string Status { get => status; set => SetProperty(ref status, value); }
    }
}
