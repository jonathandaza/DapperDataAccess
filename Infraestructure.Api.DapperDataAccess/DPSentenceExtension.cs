using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infraestructure.Api.DapperDataAccess;


namespace Infraestructure.Api.DapperDataAccess
{
    public enum SentenceType
    {
        Select,
        Update,
        Delete,
        Insert
    }

    public abstract class DPSentenceExtension<T>
    {
        protected bool Exist(string entityName, string columnName, string value, string ConnectionName)
        {
            return new DPGenericRepository<string>(ConnectionName).Exist(entityName, columnName, value);
        }

        protected Z ExecCommand<Z>(string command, string ConnectionName)
        {
            return DPGenericRepository<bool>.ExecCommand<Z>(command, ConnectionName);
        }

        protected int NextValue(string entityName, string columnName, string ConnectionName)
        {
            return new DPGenericRepository<string>(ConnectionName).NextVal(entityName, columnName);
        }

        public virtual string MainTableName { get { return string.Empty;} }

        public virtual List<ColumnExtender> GetPropertiesExtensions()
        {
            return new List<ColumnExtender>();
        }

        public virtual Dictionary<SentenceType, string> GetJoin()
        {
            return new Dictionary<SentenceType, string>();
        }

        public virtual List<ColumnExtender> ValuesExtender(T model)
        {
            return new List<ColumnExtender>();
        }

        //public virtual string ValidateSelect(T model)
        //{
        //    return string.Empty;
        //}

        public virtual string ValidateInsert(T model)
        {
            return string.Empty;
        }

        public virtual string ValidateUpdate(T model)
        {
            return string.Empty;
        }

        public virtual string ValidateDelete(Filter[] filters)
        {
            return string.Empty;
        }
    }

    public class ValueExtender
    {
        public string PropertyName { get; set; }

        public string Extension { get; set; }
    }

    public class ColumnExtender
    {
        public bool IsKey { get; set; }

        public bool IsIdentity { get; set; }

        public string ColumnName { get; set; }

        public string PropertyName { get; set; }

        public string Extension { get; set; }
    }
}
