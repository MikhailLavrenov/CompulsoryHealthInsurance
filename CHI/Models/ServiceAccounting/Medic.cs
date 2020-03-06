using System;
using System.Collections.Generic;
using System.Text;

namespace CHI.Models.ServiceAccounting
{
    public class Medic
    {
        public int Id { get; set; }
        public string FomsId { get; set; }
        public string FullName { get; set; }

        public static Medic CreateUnknown(string fomsId)
        {
            return new Medic
            {
                FomsId = fomsId,
                FullName = "Неизвестно"
            };
        }
    }
}
