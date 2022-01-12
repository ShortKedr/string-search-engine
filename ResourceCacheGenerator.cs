using System;
using System.Collections.Generic;
using Renword.StringSearchEngine.Exceptions;

namespace Renword.StringSearchEngine {
    public class ResourceCacheGenerator {
        
        private const int ResourceBucket = 16;
            
        public const int MinPossibleSearchLength = 3;
        private int _minSearchLength = 3;
        
        public ResourceCacheGenerator() {
        }

        public ResourceCacheGenerator(int minSearchLength) {
            _minSearchLength = minSearchLength;
            if (_minSearchLength < MinPossibleSearchLength) {
                _minSearchLength = MinPossibleSearchLength;
            }
        }

        public Dictionary<string, List<Resource>> GenerateSearchCache(Resource[] resources) {
            Dictionary<string, List<Resource>> searchCache = new Dictionary<string, List<Resource>>(resources.Length);
            
            for (int i = 0; i < resources.Length; i++) {
                if (resources[i] == null) continue; 
                string[] resourceSearchCache = GenerateSearchCacheForResource(resources[i]);
                for (int j = 0; j < resourceSearchCache.Length; j++) {
                    if (searchCache.ContainsKey(resourceSearchCache[j])) {
                        searchCache[resourceSearchCache[j]].Add(resources[i]);
                    } else {
                        searchCache.Add(resourceSearchCache[j], new List<Resource>(ResourceBucket));
                        searchCache[resourceSearchCache[j]].Add(resources[i]);
                    }
                }
            }

            return searchCache;
        }

        public void AppendResourceToSearchCache(Dictionary<string, List<Resource>> searchCache, Resource resource) {
            string[] resourceSearchCache = GenerateSearchCacheForResource(resource);
            for (int j = 0; j < resourceSearchCache.Length; j++) {
                if (searchCache.ContainsKey(resourceSearchCache[j])) {
                    searchCache[resourceSearchCache[j]].Add(resource);
                } else {
                    searchCache.Add(resourceSearchCache[j], new List<Resource>(ResourceBucket));
                    searchCache[resourceSearchCache[j]].Add(resource);
                }
            }
        }

        public string[] GenerateSearchCacheForResource(Resource resource) {
            if (resource == null) {
                throw new NullReferenceException("Resource reference is Null");
            }
            
            string resourceSearchString = resource.SearchString;

            if (resourceSearchString.Length < _minSearchLength) {
                throw new InvalidResourceLengthException("Resource length is smaller than minimum search length");
            }
            
            List<string> cacheEntries = new List<string>();
            
            string[] resourceTerms = resource.SearchString.Split(' ');
            for (int k = 0; k < resourceTerms.Length; k++) {
                if (resourceTerms[k].Length < _minSearchLength) {
                    cacheEntries.Add(resourceTerms[k]);
                    continue;
                }
                int levelCount = resourceTerms[k].Length - 2;
                for (int i = 0; i < levelCount; i++) {
                    int cacheEntryLength = resourceTerms[k].Length - i;
                    for (int j = 0; j <= i; j++) {
                        cacheEntries.Add(resourceTerms[k].Substring(j, cacheEntryLength).ToLower());
                    }
                }
            }

            return cacheEntries.ToArray();
        }
    }
}