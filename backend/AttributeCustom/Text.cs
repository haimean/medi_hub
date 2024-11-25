using System.Diagnostics.CodeAnalysis;

namespace QAQCApi.AttributeCustom
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class Text : Attribute
    {
        /// <summary>
        ///     The name of the column the property is mapped to.
        /// </summary>
        public string? Name { get; }

        public Text()
        {
        }

        public Text(string name)
        {
            Name = name;
        }
    }
}
