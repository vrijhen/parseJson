using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;
using System.Linq;

namespace FundaTest
{
    class Program
    {
        public List<FundaResult>    FundaResults    = new List<FundaResult>();
        public Stopwatch            Sw              = new Stopwatch();
        public long                 StopBytes       = 0;
        public long                 StartBytes      = 0;

        static void Main()
        {
            FirstQuestion();
            SecondQuestion();
        }

        private FundaPagingResponse GetPagesOfFirstQuestion()
        {
            string url = "http://partnerapi.funda.nl/feeds/Aanbod.svc/json/005e7c1d6f6c4f9bacac16760286e3cd/?type=koop&zo=/amsterdam/&page=1&pagesize=25";
            var result = new WebClient().DownloadString(url);
            return JsonConvert.DeserializeObject<FundaPagingResponse> (result);
        }

        private FundaPagingResponse GetPagesOfSecondQuestion()
        {
            string url = "http://partnerapi.funda.nl/feeds/Aanbod.svc/json/005e7c1d6f6c4f9bacac16760286e3cd/?type=koop&zo=/amsterdam/tuin/&page={0}&pagesize=25";
            var result = new WebClient().DownloadString(url);
            FundaPagingResponse jsonFundaResponse = JsonConvert.DeserializeObject <FundaPagingResponse> (result);
            return JsonConvert.DeserializeObject<FundaPagingResponse>(result);
        }

        private static void FirstQuestion()
        {
            string url = "http://partnerapi.funda.nl/feeds/Aanbod.svc/json/005e7c1d6f6c4f9bacac16760286e3cd/?type=koop&zo=/amsterdam/&page={0}&pagesize={1}";

            Program fundaTest = new Program();

            var pageInfo = fundaTest.GetPagesOfFirstQuestion();

            fundaTest.StartMeassuring(fundaTest);
            Parallel.For(0, pageInfo.Paging.AantalPaginas, i =>
            {
                fundaTest.NavigateAndParse(url, i, 25);
            });
            fundaTest.StopMeassuring(fundaTest);

            fundaTest.SortTop10MakelaarByMostObjects();
            fundaTest.Exit();
        }

        private static void SecondQuestion()
        {
            string url = "http://partnerapi.funda.nl/feeds/Aanbod.svc/json/005e7c1d6f6c4f9bacac16760286e3cd/?type=koop&zo=/amsterdam/tuin/&page={0}&pagesize={1}";

            Program fundaTest = new Program();

            var pageInfo = fundaTest.GetPagesOfSecondQuestion();

            fundaTest.StartMeassuring(fundaTest);
            Parallel.For(0, pageInfo.Paging.AantalPaginas, i =>
            {
                fundaTest.NavigateAndParse(url, i, 25);
            });
            fundaTest.StopMeassuring(fundaTest);

            fundaTest.SortTop10MakelaarByMostObjectsWithGarden();
            fundaTest.Exit();
        }

        private void Exit()
        {
            Console.WriteLine("");
            Console.WriteLine("Type something followed by an Enter to exit");
            Console.WriteLine("");
            Console.ReadLine();
        }

        private void StartMeassuring(Program fundaTest)
        {
            var before = Process.GetCurrentProcess().VirtualMemorySize64;
            StartBytes = GC.GetTotalMemory(true);
            Console.WriteLine("Memory before processing: " + StartBytes);
            fundaTest.Sw.Start();
        }

        private void StopMeassuring(Program fundaTest)
        {
            fundaTest.Sw.Stop();
            StopBytes = GC.GetTotalMemory(true);
            var diffTime = fundaTest.Sw.Elapsed; 
            var processingMemory = StopBytes - StartBytes; 
            Console.WriteLine("");
            Console.WriteLine("Memory after processing: " + StopBytes);
            Console.WriteLine("Memory used: " +  StartBytes);
            Console.WriteLine("Time elapsed: " + diffTime);
            Console.WriteLine("");
        }

        private void NavigateAndParse(string urlStr, int pageNr, int pageSize )
        {
            var uri = new Uri(string.Format(urlStr, pageNr, pageSize));
            var result = new WebClient().DownloadString(uri);
            FundaResponse jsonFundaResponse = JsonConvert.DeserializeObject<FundaResponse>(result);
            FundaResults.AddRange(jsonFundaResponse.Objects);        
        }

        private void SortTop10MakelaarByMostObjects()
        {
            Console.WriteLine("Top 10 makelaars with most objects for sale in Amsterdam:");
            Console.WriteLine("");

            foreach (var line in FundaResults.Where(e=>e.MakelaarNaam!=null)
                .GroupBy(info => info.MakelaarNaam)
                .Select(group => new
                {
                    MakelaarNaam = group.Key,
                    Count = group.Count()
                })
                .OrderByDescending(x => x.Count).Take(10))
            {
                Console.WriteLine("{0} {1}", line.MakelaarNaam, line.Count);
            }
            FundaResults.Clear();
        }

        private void SortTop10MakelaarByMostObjectsWithGarden()
        {
            Console.WriteLine("Top 10 makelaars with most objects with Garden for sale in Amsterdam:");
            Console.WriteLine("");

            foreach (var line in FundaResults.GroupBy(info => info.MakelaarNaam)
                .Select(group => new
                {
                    MakelaarNaam = group.Key,
                    Count = group.Count()
                })
                .OrderByDescending(x => x.Count).Take(10))
            {
                Console.WriteLine("{0} {1}", line.MakelaarNaam, line.Count);
            }
            FundaResults.Clear();
        }
    }    
}
