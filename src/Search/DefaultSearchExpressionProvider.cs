using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Api.Query.Search
{
    public class DefaultSearchExpressionProvider : ISearchExpressionProvider
    {
        protected const string EqualOperator = "eq";
        public virtual IEnumerable<string> GetOperators()
        {
            yield return EqualOperator;
        }

        public virtual ConstantExpression GetValue(string input)
        {
            return Expression.Constant(input);
        }

        public virtual Expression GetComparison(MemberExpression left, string op, ConstantExpression right)
        {
            if (op.ToLower() != EqualOperator)
            {
                throw new ArgumentException($"Invalid operator '{op}'!");
            }
            return Expression.Equal(left, right);
        }
    }
}