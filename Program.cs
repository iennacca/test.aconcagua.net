using System;
using System.Collections.Generic;
using System.Linq;
using aconcagua.server;

namespace test.aconcagua.console
{
    class Program
    {
        static void Main()
        {
            Console.WriteLine("Hello World!");

            var tester = new ServiceTester(new AconcaguaService(null));
            var version = tester.GetVersion();

            Console.WriteLine($"Version: {version}");

            foreach (var db in tester.GetDatabases())
                Console.WriteLine($"Database: {db}");

            tester.GetObservations();
        }
    }

    public class ServiceTester
    {
        private AconcaguaService Service { get; }

        public ServiceTester(AconcaguaService service)
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

        public void GetObservations()
        {
            var getObservationsRequest = new GetObservationsRequest();
            getObservationsRequest.Keys.Add(
                SourceSeriesKeyList.Create("dmxe://localhost:5001/brb_macrofw.dmxe", new[] { "316NGDP", "316NGDP_YOY" }));

            getObservationsRequest.Frequencies = "A";
            var getObservationsResponse = Service.GetObservations(getObservationsRequest, null).Result;

            foreach (var seriesdata in getObservationsResponse.Seriesdata)
            {
                Console.WriteLine(seriesdata.Key.Sourcename + "\t" + seriesdata.Key.Seriesname);
                foreach (var keyValue in seriesdata.Values)
                {
                    Console.WriteLine(keyValue.Key + "\t" + keyValue.Value);
                }
            }
        }
    }

    public interface ISeries
    {
        public string SeriesCode { get; }
        IEnumerable<KeyValuePair<string, float>> Observations { get; }
    }

    public static class SourceSeriesKeyList
    {
        public static IEnumerable<SourceSeriesKey> Create(string sourceName, IEnumerable<string> _seriesList)
        {
            return _seriesList.Select(s => new SourceSeriesKey() { Sourcename = sourceName, Seriesname = s }).ToList();
        }
    }

}