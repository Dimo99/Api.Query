using System;
using System.Linq.Expressions;

namespace Api.Query.Search
{
    public class DecimalToIntSearchExpressionProvider : ComparableSearchExpressionProvider
    {
        public override ConstantExpression GetValue(string input)
        {
            if (!decimal.TryParse(input, out var dec))
                throw new ArgumentException("Invalid search value.");
            return Expression.Constant((int)(dec * 100));
        }

       
    }
}