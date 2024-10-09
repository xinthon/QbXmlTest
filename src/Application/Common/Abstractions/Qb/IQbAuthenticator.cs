using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Common.Abstractions.Qb;

internal interface IQbAuthenticator
{
    void SaveTicket(string ticket);
}
