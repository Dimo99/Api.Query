using System;
using Api.Query.Search;

namespace Api.Query.CustomAttributes.Searchable
{
    [AttributeUsage(AttributeTargets.Property,AllowMultiple = false)]
    public class SearchableAttribute : Attribute
    {
        public ISearchExpressionProvider ExpressionProvider { get; set; } = new DefaultSearchExpressionProvider();
    }
}