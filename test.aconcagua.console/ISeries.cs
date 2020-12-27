using System.Collections.Generic;

namespace test.aconcagua.console
{
    public interface ISeries
    {
        string Database { get; }
        string SeriesCode { get; }
        IReadOnlyDictionary<string, double> Observations { get; }
    }
}
