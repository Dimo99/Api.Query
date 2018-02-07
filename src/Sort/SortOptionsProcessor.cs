using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Api.Query.CustomAttributes.Sortable;
using Api.Query.Helpers;

namespace Api.Query.Sort
{
    public class SortOptionsProcessor<T, TEntity>
    {
        private readonly string[] orderBy;

        public SortOptionsProcessor(string[] orderBy)
        {
            this.orderBy = orderBy;
        }

        public IEnumerable<SortTerm> GetAllTerms()
        {
            if (orderBy == null)
            {
                yield break;
            }

            foreach (var s in orderBy)
            {
                if (string.IsNullOrEmpty(s)) continue;
                var tokens = s.Split(' ');
                if (tokens.Length == 0)
                {
                    yield return new SortTerm() { Name = s };
                    continue;
                }

                var descending = tokens.Length > 1 && tokens[1].Equals("desc", StringComparison.OrdinalIgnoreCase);
                yield return new SortTerm() { Name = tokens[0], Descending = descending };
            }
        }

        public IEnumerable<SortTerm> GetValidTerms()
        {
            var queryTerms = GetAllTerms().ToArray();
            if(!queryTerms.Any()) yield break;
            var declaredTerms = GetTermsFromModel();
            foreach (var queryTerm in queryTerms)
            {
                var declaredTerm = declaredTerms.SingleOrDefault(x => x.Name.ToLower() == queryTerm.Name.ToLower());
                if (declaredTerm == null)
                {
                    continue;
                }
                yield return new SortTerm()
                {
                    Name = declaredTerm.Name,
                    Descending = queryTerm.Descending,
                    Default = declaredTerm.Default
                };
            }
        }
        private static IEnumerable<SortTerm> GetTermsFromModel()
        {
            return typeof(T).GetTypeInfo().DeclaredProperties.Where(p =>
                p.CustomAttributes.Any(x => x.AttributeType == typeof(SortableAttribute)))
                .Select(p=>new SortTerm(){Name = p.Name,Default = p.GetCustomAttribute<SortableAttribute>().Default});
        }

        public IQueryable<TEntity> Apply(IQueryable<TEntity> query)
        {
            var terms = GetValidTerms().ToArray();
            if (!terms.Any())
            {
                terms = GetTermsFromModel().Where(t => t.Default).ToArray();
            }

            if (!terms.Any())
            {
                return query;
            }
            var modifiedQuerry = query;
            var useThenBy = false;
            foreach (var term in terms)
            {
                var propertyInfo = ExpressionHelper.GetPropertyInfo<TEntity>(term.Name);
                var obj = ExpressionHelper.Parameter<TEntity>();
                var key = ExpressionHelper.GetPropertyExpression(obj, propertyInfo);
                var keySelector = ExpressionHelper.GetLambda(typeof(TEntity), propertyInfo.PropertyType, obj, key);

                modifiedQuerry = ExpressionHelper.CallOrderByOrThenBy(modifiedQuerry, useThenBy, term.Descending,
                    propertyInfo.PropertyType, keySelector);
                useThenBy = true;
            }

            return modifiedQuerry;
        }
    }
}