using System.Collections.Generic;

namespace RenwordDigital.StringSearchEngine {
    public class SearchIndex {
        
        public const int SearchLength = 3;
        
        private List<Resource> _resources = new List<Resource>(16);
        private HashSet<Resource> _resourcesCache = new HashSet<Resource>();
        
        private Dictionary<string, List<Resource>> _searchCache = new Dictionary<string, List<Resource>>();
        
        private ResourceCacheGenerator _cacheGenerator = new ResourceCacheGenerator(SearchLength);

        private int _searchBucket = 128;
        
        public SearchIndex() {
        }

        public SearchIndex(Resource[] resources, int searchBucket = 128) {
            _resources.AddRange(resources);
            RebuildResourceCache();
            
            _searchBucket = searchBucket;
            RebuildSearchCache();
        }

        public void SetResources(Resource[] resources) {
            _resources.Clear();
            _resources.AddRange(resources);
            RebuildResourceCache();
            RebuildSearchCache();
        }

        public void AddResource(Resource resource) {
            if (!_resourcesCache.Contains(resource)) {
                _resources.Add(resource);
                _resourcesCache.Add(resource);
                _cacheGenerator.AppendResourceToSearchCache(_searchCache, resource);
            }
        }

        public void RemoveResource(Resource resource) {
            if (_resourcesCache.Contains(resource)) {
                _resources.Remove(resource);
                _resourcesCache.Remove(resource);
                RebuildSearchCache();
            }
        }

        private void RebuildResourceCache() {
            _resourcesCache = new HashSet<Resource>(_resources);
        }
        
        private void RebuildSearchCache() {
            if (_cacheGenerator == null) _cacheGenerator = new ResourceCacheGenerator(SearchLength);
            _searchCache = _cacheGenerator.GenerateSearchCache(_resources.ToArray());
        }

        public List<Resource> GetSearchResult(string searchString) {
            List<Resource> foundResources = new List<Resource>(_searchBucket);
            string[] searchEntries = searchString.Split(' ');

            for (int i = 0; i < searchEntries.Length; i++) {
                if (_searchCache.ContainsKey(searchEntries[i])) {
                    foundResources.AddRange(_searchCache[searchEntries[i]]);
                }
            }

            return foundResources;
        }
    }
}