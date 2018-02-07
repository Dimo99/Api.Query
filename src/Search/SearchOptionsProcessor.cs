using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Api.Query.CustomAttributes.Searchable;
using Api.Query.Helpers;

namespace Api.Query.Search
{
    public class SearchOptionsProcessor<T,TEntity>
    {
        private readonly string[] searchOptions;

        public SearchOptionsProcessor(string[] searchOptions)
        {
            this.searchOptions = searchOptions;
        }

        public IEnumerable<SearchTerm> GetAllTerms()
        {
            if (searchOptions == null) yield break;
            foreach (var option in searchOptions)
            {
                if(string.IsNullOrEmpty(option)) continue;
                var tokens = option.Split(' ');
                if (tokens.Length < 3)
                {
                    yield return new SearchTerm()
                    {
                        Name = option,
                        IsValid = false
                    };
                    continue;
                }
                yield return new SearchTerm()
                {
                    IsValid = true,
                    Name = tokens[0],
                    Operator = tokens[1],
                    Value = string.Join(" ",tokens.Skip(2))

                };

            }
        }

        private static IEnumerable<SearchTerm> GetTermsFromModel()
        {
            return typeof(T).GetTypeInfo().DeclaredProperties
                .Where(p => p.GetCustomAttributes<SearchableAttribute>().Any())
                .Select(p => new SearchTerm()
                {
                    Name = p.Name,
                    ExpressionProvider = p.GetCustomAttribute<SearchableAttribute>().ExpressionProvider
                });
        }

        public IEnumerable<SearchTerm> GetValidTerms()
        {
            var queryTerms = GetAllTerms()
                .Where(x => x.IsValid)
                .ToArray();
            if (!queryTerms.Any())
            {
                yield break;
            }

            var declaredTerms = GetTermsFromModel();
            foreach (var term in queryTerms)
            {
                var declaredTerm = declaredTerms.SingleOrDefault(x =>
                    String.Equals(x.Name, term.Name, StringComparison.OrdinalIgnoreCase));
                if(declaredTerm == null) continue;
                yield return new SearchTerm()
                {
                    IsValid = term.IsValid,
                    Name = declaredTerm.Name,
                    Operator = term.Operator,
                    Value = term.Value,
                    ExpressionProvider = declaredTerm.ExpressionProvider
                };
            }
        }

        public IQueryable<TEntity> Apply(IQueryable<TEntity> query)
        {
            var terms = GetValidTerms().ToArray();
            if (!terms.Any()) return query;
            var modifiedQuery = query;
            foreach (var term in terms)
            {
                var properyInfo = ExpressionHelper.GetPropertyInfo<TEntity>(term.Name);
                var obj = ExpressionHelper.Parameter<TEntity>();
                var left = ExpressionHelper.GetPropertyExpression(obj, properyInfo);
                var right = term.ExpressionProvider.GetValue(term.Value);
                var comparisonExpression = term.ExpressionProvider.GetComparison(left,term.Operator,right);
                var lamda = ExpressionHelper.GetLambda<TEntity, bool>(obj, comparisonExpression);
                modifiedQuery = ExpressionHelper.CallWhere(modifiedQuery, lamda);
            }
            return modifiedQuery;
        }
    }
}