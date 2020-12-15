using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DiabNet.Domain;

namespace DiabNet.Nightscout
{
    public interface INightscoutApi
    {
        Task<IList<Sgv>> GetEntries(DateTimeOffset from, DateTimeOffset to);
    }
}
