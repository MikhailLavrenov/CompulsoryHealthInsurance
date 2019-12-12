using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CHI.Licensing
{
    public enum ClaimKey
    {
        [Description("Не выбрано")] None = 0,
        [Description("Дата")] Date = 1,
        [Description("Код")] Code = 2,
    }
}
