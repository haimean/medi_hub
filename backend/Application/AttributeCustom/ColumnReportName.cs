namespace DashboardApi.Application.AttributeCustom
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class ColumnReportName : Attribute
    {
        /// <summary>
        ///     The name of the column the property is mapped to.
        /// </summary>
        public string? Name { get; }

        public ColumnReportName()
        {
        }

        public ColumnReportName(string name)
        {
            Name = name;
        }
    }
}
