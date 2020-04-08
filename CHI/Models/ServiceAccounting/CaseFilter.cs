using System;

namespace CHI.Models.ServiceAccounting
{
    public class CaseFilter
    {
        public int Id { get; set; }
        public CaseFilterKind Kind { get; set; }
        public double Code { get; set; }
        public DateTime? ValidFrom { get; set; }
        public DateTime? ValidTo { get; set; }

        public CaseFilter()
        {
        }
    }
}
