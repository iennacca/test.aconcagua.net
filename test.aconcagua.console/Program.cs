using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using aconcagua.server;
using XPlot.Plotly;

namespace test.aconcagua.console
{
    class Program
    {
        // static async Task Main(string[] args)
        // {
        //     var counter = 0;
        //     var max = args.Length != 0 ? Convert.ToInt32(args[0]) : -1;
        //     while (max == -1 || counter < max)
        //     {
        //         Console.WriteLine($"Counter: {++counter}");
        //         await Task.Delay(1000);
        //     }
        // }

        static async Task Main(string[] args)
        {
            Console.WriteLine("Task started");

            var tester = new ServiceTestHarness(new AconcaguaService(null));
            var version = tester.GetVersion();

            Console.WriteLine($"Version: {version}");

            foreach (var db in tester.GetDatabases())
                Console.WriteLine($"Database: {db}");

            var serieslist = tester.GetSeries("dmxe://localhost:5001/brb_macrofw.dmxe", new[] { "316NGDP", "316NGDP_YOY" });
            UITestHarness.ShowGraph(serieslist);

            Console.WriteLine("Task done.");

            var max = args.Length != 0 ? Convert.ToInt32(args[0]) : -1;
            while (true)
                await Task.Delay(1000);
        }
    }

    public class ServiceTestHarness
    {

        private static class SourceSeriesKeyList
        {
            public static IEnumerable<SourceSeriesKey> Create(string sourceName, IEnumerable<string> _seriesList)
            {
                return _seriesList.Select(s => new SourceSeriesKey() { Sourcename = sourceName, Seriesname = s }).ToList();
            }
        }

        private sealed class Series : ISeries
        {
            public string Database { get; }
            public string SeriesCode { get; }
            public IReadOnlyDictionary<string, double> Observations { get; }
            public Series(string database, string code, IReadOnlyDictionary<string, double> observations)
            {
                Database = database;
                SeriesCode = code;
                Observations = observations;
            }
        }

        private AconcaguaService Service { get; }

        public ServiceTestHarness(AconcaguaService service)
        {
            Service = service;
        }

        public string GetVersion()
        {
            var getVersionResponse = Service.GetVersion(null, null).Result;
            return getVersionResponse.Version;
        }

        public IEnumerable<string> GetDatabases()
        {
            var getDatabasesResponse = Service.GetDatabases(null, null).Result;

            foreach (var db in getDatabasesResponse.Sourcenames)
            {
                yield return db;
            }
        }

        public IEnumerable<ISeries> GetSeries(string database, string[] codes)
        {
            var getObservationsRequest = new GetObservationsRequest();
            getObservationsRequest.Keys.Add(
                SourceSeriesKeyList.Create(database, codes));

            getObservationsRequest.Frequencies = "A";
            var getObservationsResponse = Service.GetObservations(getObservationsRequest, null).Result;

            foreach (var seriesdata in getObservationsResponse.Seriesdata)
                yield return new Series(seriesdata.Key.Sourcename, seriesdata.Key.Seriesname, GetObservations(getObservationsResponse));
        }

        public IReadOnlyDictionary<string, double> GetObservations(GetObservationsResponse response)
        {
            var obs = new Dictionary<string, double>();

            foreach (var seriesdata in response.Seriesdata)
            {
                Console.WriteLine(seriesdata.Key.Sourcename + "\t" + seriesdata.Key.Seriesname);
                foreach (var keyValue in seriesdata.Values)
                {
                    Console.WriteLine(keyValue.Key + "\t" + keyValue.Value);
                    if (!obs.ContainsKey(keyValue.Key))
                        obs.Add(keyValue.Key, keyValue.Value);
                }
            }
            return obs;
        }
    }

    public static class UITestHarness
    {
        public static void ShowGraph(IEnumerable<ISeries> serieslist)
        {
            foreach (var series in serieslist)
            {
                var layout = new Layout.Layout() { title = series.SeriesCode };

                Chart.Plot(
                    new Graph.Scatter
                    {
                        x = series.Observations.Keys,
                        y = series.Observations.Values
                    }
                ).WithLayout(layout);
            }
        }
    }
}