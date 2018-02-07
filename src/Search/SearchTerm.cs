namespace Api.Query.Search
{
    public class SearchTerm
    {
        public string Name { get; set; }
        public string Operator { get; set; }
        public string Value { get; set; }
        public bool IsValid { get; set; }
        public ISearchExpressionProvider ExpressionProvider { get; set; }
    }
}