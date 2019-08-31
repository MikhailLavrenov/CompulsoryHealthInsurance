using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PatientsFomsRepository.Models
{
    /// <summary>
    /// Перечисление кто может видеть учетные данные
    /// </summary>
    public enum CredentialScope : byte
    {
        [Description("Текущий пользователь")]     ТекущийПользователь = 0,
        [Description("Пользователи компьютера")]  ПользователиКомпьютера = 1,
        [Description("Все (не безопасно)")]       Все = 2,
    }
}
