using System;
using Api.Query.Search;

namespace Api.Query.CustomAttributes.Searchable
{
    [AttributeUsage(AttributeTargets.Property,AllowMultiple = false)]
    public class SearchableDecimalAttribute : SearchableAttribute
    {
        public SearchableDecimalAttribute()
        {
            ExpressionProvider = new DecimalToIntSearchExpressionProvider();
        }
    }
}