using System;
using System.Collections.Generic;
using NUnit.Framework;
using RenwordDigital.StringSearchEngine;

namespace Tests {
    public class Tests {

        [Test]
        public void Test1() {

            string[] expectedResult = new[] {
                "TimeAndProgressManager",
                "TimeHandler",
                "TimeRequestHandler",
                "TimeUpdateHandler",
                "TimeFuckHandler",
                "TimeKekHandler"
            };
            
            Resource[] resources = {
                new ("TimeManager"),
                new ("RateUsSettings"),
                new ("TimeManagerSettings"),
                new ("TimeAndProgressManager"),
                new ("TimeHandler"),
                new ("TimeRequestHandler"),
                new ("TimeUpdateHandler"),
                new ("TimeFuckHandler"),
                new ("TimeKekHandler"),
            };

            SearchIndex searchIndex = new SearchIndex(resources);

            string searchString = "and";
            List<Resource> searchResult = searchIndex.GetSearchResult(searchString);

            Console.WriteLine($"Search result for '{searchString}':");
            List<string> result = new List<string>();
            for (int i = 0; i < searchResult.Count; i++) {
                Console.WriteLine(searchResult[i].Name);
                result.Add(searchResult[i].Name);
            }

            if (result.Count != expectedResult.Length) {
                Assert.Fail();
                return;
            }
            
            for (int i = 0; i < expectedResult.Length; i++) {
                if (!result.Contains(expectedResult[i])) {
                    Assert.Fail();
                }
            }
            Assert.Pass();
        }
    }
}