using System;
using System.Reflection;

namespace Infraestructure.Api.DapperDataAccess
{
    public class DapperSentenceGenerator<T>
    {
        SentenceStructurator<T> constructor;

        public void validateInsert(T model)
        {
            constructor.validateInsert(model);
        }

        public void validateUpdate(T model)
        {
            constructor.validateUpdate(model);
        }

        public void validateDelete(Filter[] filters)
        {
            constructor.validateDelete(filters);
        }

        public T SanitizeModel(T model)
        {
            PropertyInfo[] modelPropertyInfo;
            modelPropertyInfo = model.GetType().GetProperties();

            for (int i = 0; i < modelPropertyInfo.Length; i++)
            {
                if ((modelPropertyInfo[i].PropertyType).FullName.Equals((typeof(String)).FullName))
                {
                    if (modelPropertyInfo[i].GetValue(model) != null)
                        modelPropertyInfo[i].SetValue(model, modelPropertyInfo[i].GetValue(model).ToString());
                        //modelPropertyInfo[i].SetValue(model, modelPropertyInfo[i].GetValue(model).ToString().Sanitize());
                }
            }

            return model;
        }

        public Filter[] SanitizeFilters(Filter[] filters)
        {
            for (int i = 0; i < filters.Length; i++)
            {
                //filters[i].Operator = filters[i].Operator.Sanitize();
                //filters[i].Value = filters[i].Value.Sanitize();
                //filters[i].Field = filters[i].Field.Sanitize();
            }

            return filters;
        }

        //public string CreateDBQueueObject(string deleteObject, string name)
        //{
        //    return SqlTools.CreateDeleteObject(deleteObject, name);
        //}

        //public string CreateInsertQueueCommand(string queueObject)
        //{
        //    return SqlTools.CreateInsertQueueCommand(queueObject);
        //}

        public DapperSentenceGenerator(DPSentenceExtension<T> extension)
        {
            constructor = new SentenceStructurator<T>(extension);
        }

        public DapperSentenceGenerator()
        {
            
        }

        public string GetDeleteEntityName()
        {
            return constructor.GetEntityName(SentenceType.Delete);
        }

        public bool ValidateEnqueue(string actionName)
        {
            return constructor.ValidateEnqueue(actionName);
        }

        public string CreateSelecObjectByFilter(Filter[] conditions)
        {
            return string.Format("SELECT * FROM {0} {1}", GetDeleteEntityName(), constructor.CreateFilters(conditions));
        }

        public string CreateSelect(Filter[] conditions)
        {
          var preSql = "SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;";
          var sql = string.Format(preSql + "SELECT {0} FROM {1} {2} ", constructor.CreateSelectFields(),
                                             constructor.GetEntityName(SentenceType.Select),
                                             constructor.CreateFilters(conditions));
          return sql;
        }

        public string CreateUpdate(Filter[] conditions, T model)
        {
            return string.Format("UPDATE {0} SET{1} {2}", constructor.GetEntityName(SentenceType.Update)
                                                                                               , constructor.CreateSetFields(model)
                                                                                               , constructor.CreateFilters(conditions));
        }

        public string CreateInsertInto(T model)
        {
            string s = string.Format("INSERT INTO {0} ({1}) VALUES ({2}); SELECT CAST(SCOPE_IDENTITY() AS INT)", constructor.GetEntityName(SentenceType.Insert)
                                                                                                            , constructor.CreateInsertFields(model)
                                                                                                            , constructor.CreateInsertValues(model));
            return s;
        }

        public string CreateDelete(Filter[] conditions)
        {
            return string.Format("DELETE {0} {1}", constructor.GetEntityName(SentenceType.Delete)
                                                   , constructor.CreateFilters(conditions));

        }

        public static string CreateExist(string tableName, string columnName)
        {
          var preSql = "SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;";
            return string.Format(preSql + "SELECT COUNT(1) FROM {0} WHERE {1} = @value", tableName, columnName);
        }

        public static string NextVal(string tableName, string columnName)
        {
          var preSql = "SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;";
            return string.Format(preSql + "SELECT {0}.NEXTVAL FROM {1}", columnName, tableName);
        }
    }
}
