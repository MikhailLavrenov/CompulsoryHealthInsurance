using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CHI.Services
{
    public interface ICredential
    {
         string Login { get; }
         string Password { get; }
    }
}
