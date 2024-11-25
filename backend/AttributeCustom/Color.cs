using System.Diagnostics.CodeAnalysis;

namespace QAQCApi.AttributeCustom
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class Color : Attribute
    {
        /// <summary>
        ///     The name of the column the property is mapped to.
        /// </summary>
        public string? Name { get; }

        public Color()
        {
        }

        public Color(string name)
        {
            Name = name;
        }
    }
}
