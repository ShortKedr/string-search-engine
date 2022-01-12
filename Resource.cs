namespace Renword.StringSearchEngine {
    public class Resource {
        private string _name;
        private string _searchString;

        public string Name => _name;
        public string SearchString => _searchString;

        public Resource(string name) {
            _name = name;
            _searchString = name;
        }
        
        public Resource(string name, string searchString) {
            _name = name;
            _searchString = searchString;
        }
    }
}