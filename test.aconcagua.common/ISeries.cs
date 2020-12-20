using System.Collections.Generic;

namespace test.aconcagua.common
{
    public interface ISeries
    {
        string Database { get; }
        string SeriesCode { get; }
        IReadOnlyDictionary<string, double> Observations { get; }
    }
}
