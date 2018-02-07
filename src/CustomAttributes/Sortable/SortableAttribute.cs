using System;

namespace Api.Query.CustomAttributes.Sortable
{
    [AttributeUsage(AttributeTargets.Property,AllowMultiple = false)]
    public class SortableAttribute : Attribute
    {
        public bool Default { get; set; }
    }
}