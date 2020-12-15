using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infraestructure.Api.DapperDataAccess
{
    public class EmptyDPSentenceExtension<T>: DPSentenceExtension<T>
    {
        public override string MainTableName { get { return string.Empty; } }

        public override List<ColumnExtender> GetPropertiesExtensions()
        {
            return new List<ColumnExtender>();
        }

        public override Dictionary<SentenceType, string> GetJoin()
        {
            return new Dictionary<SentenceType, string>();
        }

        public override List<ColumnExtender> ValuesExtender(T model)
        {
            return new List<ColumnExtender>();
        }
    }
}
