namespace Infraestructure.Api.DapperDataAccess
{
    public class Filter
    {
        public string Field { get; set; }
        public string Operator { get; set; }
        public string Value { get; set; }
        public bool HasQuotes { get; set; }
    }
}
