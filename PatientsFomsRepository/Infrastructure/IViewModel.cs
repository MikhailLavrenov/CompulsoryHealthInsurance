using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PatientsFomsRepository.Infrastructure
    {
    /// <summary>
    /// Общие свойства для всех ViewModel
    /// </summary>
    public interface IViewModel
        {
        #region Properties
        string ShortCaption { get; set; }
        string FullCaption { get; set; }
        #endregion
        }
    }
