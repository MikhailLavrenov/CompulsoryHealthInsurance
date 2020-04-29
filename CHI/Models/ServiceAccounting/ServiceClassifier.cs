using System;
using System.Collections.Generic;

namespace CHI.Models.ServiceAccounting
{
    public class ServiceClassifier
    {
        public int Id { get; set; }
        public DateTime ValidFrom { get; set; }
        public DateTime ValidTo { get; set; }
        public List<ServiceClassifierItem> ServiceClassifierItems { get; set; }

    }
}
