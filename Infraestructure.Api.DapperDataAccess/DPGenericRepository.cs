using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Configuration;
using Dapper;
using System.Transactions;
using System.Linq;

namespace Infraestructure.Api.DapperDataAccess
{
    public class DPGenericRepository<T>
    {
        private string ConnectionName;

        DapperSentenceGenerator<T> queryConstructor;

        public DPGenericRepository(string connectionName)
        {
            this.ConnectionName = connectionName;
            queryConstructor = new DapperSentenceGenerator<T>(new EmptyDPSentenceExtension<T>());
        }

        public DPGenericRepository(DPSentenceExtension<T> extension)
        {
            queryConstructor = new DapperSentenceGenerator<T>(extension);
        }

        private async Task<T> WithConnection<T>(Func<IDbConnection, Task<T>> getData)
        {
            try
            {
                using (var connection = new SqlConnection(ConfigurationManager.ConnectionStrings[ConnectionName].ConnectionString))
                {
                    await connection.OpenAsync();
                    return await getData(connection);
                }
            }
            catch (TimeoutException ex)
            {
                throw new Exception(String.Format("{0}.WithConnection() experienced a SQL timeout", GetType().FullName));
            }
            catch (SqlException ex)
            {
                throw new Exception(String.Format("{0}.WithConnection() experienced a SQL exception (not a timeout)", GetType().FullName));
            }
        }

        public T ConnectExcecute<T>(Func<IDbConnection, T> getData)
        {
            try
            {
                using (var connection = new SqlConnection(ConfigurationManager.ConnectionStrings[ConnectionName].ConnectionString))
                {
                    connection.Open();
                    return getData(connection);
                }
            }
            catch (TimeoutException ex)
            {
                throw new Exception(String.Format("{0}.WithConnection() experienced a SQL timeout", GetType().FullName));
            }
            catch (SqlException ex)
            {
                throw new Exception(String.Format("{0}.WithConnection() experienced a SQL exception (not a timeout)", GetType().FullName));
            }
        }

        public T ConnectExcecute<T>(Func<IDbConnection, T> getData, string connectionName)
        {
            try
            {
                using (var connection = new SqlConnection(ConfigurationManager.ConnectionStrings[connectionName].ConnectionString))
                {
                    connection.Open();
                    return getData(connection);
                }
            }
            catch (TimeoutException ex)
            {
                throw new Exception(String.Format("{0}.WithConnection() experienced a SQL timeout", GetType().FullName));
            }
            catch (SqlException ex)
            {
                throw new Exception(String.Format("{0}.WithConnection() experienced a SQL exception (not a timeout)", GetType().FullName));
            }
        }

        public IEnumerable<T> GetTest(string command, string connectionName)
        {
            return ConnectExcecute(c =>
            {
                return c.Query<T>(command);
            }, connectionName);
        }

        public IEnumerable<T> GetAll()
        {
            return ConnectExcecute(c =>
            {
                return c.Query<T>(queryConstructor.CreateSelect(new Filter[] { }));
            });
        }

        public async Task<IEnumerable<T>> GetAllAsync()
        {
            return await WithConnection(async c =>
            {
                return await c.QueryAsync<T>(queryConstructor.CreateSelect(new Filter[] { }));
            });
        }

        public async Task<IEnumerable<T>> GetByAsync(Filter[] filters)
        {
            filters = queryConstructor.SanitizeFilters(filters);
            return await WithConnection(async c =>
            {
                return c.Query<T>(queryConstructor.CreateSelect(filters));
            });
        }

        public IEnumerable<T> GetBy(Filter[] filters)
        {
            filters = queryConstructor.SanitizeFilters(filters);
            return ConnectExcecute(c =>
            {
                return c.Query<T>(queryConstructor.CreateSelect(filters));
            });
        }

        public async Task<IEnumerable<int>> UpdateAsync(Filter[] filters, T model)
        {
            filters = queryConstructor.SanitizeFilters(filters);
            model = queryConstructor.SanitizeModel(model);
            queryConstructor.validateUpdate(model);
            return await WithConnection(async c =>
            {
                return await c.QueryAsync<int>(queryConstructor.CreateUpdate(filters, model), model);
            });
        }

        public IEnumerable<int> Update(Filter[] filters, T model)
        {
            filters = queryConstructor.SanitizeFilters(filters);
            model = queryConstructor.SanitizeModel(model);
            queryConstructor.validateUpdate(model);
            return ConnectExcecute(c =>
            {
                return c.Query<int>(queryConstructor.CreateUpdate(filters, model), model);
            });
        }

        public async Task<IEnumerable<int>> CreateAsync(T model)
        {
            model = queryConstructor.SanitizeModel(model);
            queryConstructor.validateInsert(model);
            return await WithConnection(async c =>
            {
                return await c.QueryAsync<int>(queryConstructor.CreateInsertInto(model), model);
            });
        }

        public static void StoreProcedure(string name, object parameters, string ConnectionName)
        {
            using (var connection = new SqlConnection(ConfigurationManager.ConnectionStrings[ConnectionName].ConnectionString))
            {
                connection.Open();

                connection.Query(name, parameters, commandType: CommandType.StoredProcedure);
            }

        }

        public IEnumerable<int> Create(T model)
        {
            model = queryConstructor.SanitizeModel(model);
            queryConstructor.validateInsert(model);
            return ConnectExcecute(c =>
            {
                IEnumerable<int?> executionResult = c.Query<int?>(queryConstructor.CreateInsertInto(model), model);
                List<int> returnResult = new List<int>() { 0 };
                if (executionResult != null)
                {
                    return returnResult;
                }
                return returnResult;
            });
        }

        public void Create(T model, out int id)
        {
            model = queryConstructor.SanitizeModel(model);
            queryConstructor.validateInsert(model);
            int? executionResult;

            executionResult = ConnectExcecute(c =>
            {
                return c.Query<int?>(queryConstructor.CreateInsertInto(model), model).FirstOrDefault();
            });

            id = executionResult ?? 0;
        }

        public async Task<IEnumerable<int>> DeleteAsync(Filter[] filters)
        {
            filters = queryConstructor.SanitizeFilters(filters);
            queryConstructor.validateDelete(filters);
            return await WithConnection(async c =>
            {
                return await c.QueryAsync<int>(queryConstructor.CreateDelete(filters));
            });
        }

        public IEnumerable<int> Delete(Filter[] filters)
        {
            filters = queryConstructor.SanitizeFilters(filters);
            queryConstructor.validateDelete(filters);


            using (TransactionScope scope = new TransactionScope())
            {
                List<T> selectedObject = GetBy(filters).ToList<T>();

                return ConnectExcecute(c =>
                {
                    string commandoProvisional = queryConstructor.CreateSelecObjectByFilter(filters);

                    object esteObjeto = c.Query<object>(commandoProvisional).FirstOrDefault();
                    //Dictionary<string,object> listadoObjetos = esteObjeto.ToDictionary<string, object>();
                    string este = esteObjeto.ToString().Replace("DapperRow, ", "\"");
                    este = este.Replace(" = ", "\" : ");
                    este = este.Replace(", ",", \"");

                    //string deleteQueueCommand = CreateAddToDeleteQueueCommand(este);

                    //if (queryConstructor.ValidateEnqueue("Delete"))
                    //    c.Query<int>(deleteQueueCommand);

                    IEnumerable<int> result;
                    result = c.Query<int>(queryConstructor.CreateDelete(filters));

                    scope.Complete();
                    return result;
                });
            }
        }

        //public string CreateAddToDeleteQueueCommand(string deleteObject)
        //{
        //    string queueObject = string.Empty;
        //    //Dictionary<string, string> DBObjectData = new Dictionary<string, string>();
        //    //foreach(var element in filters)
        //    //{
        //    //    DBObjectData.Add(element.Field, element.Value);
        //    //}

        //    string objectJson = string.Empty;
        //    objectJson = queryConstructor.CreateDBQueueObject(deleteObject, queryConstructor.GetDeleteEntityName());

        //    return queryConstructor.CreateInsertQueueCommand(objectJson);
        //}

        public static Z ExecCommand<Z>(string command, string ConnectionName)
        {
            using (var connection = new SqlConnection(ConfigurationManager.ConnectionStrings[ConnectionName].ConnectionString))
            {
                connection.Open();
                return connection.ExecuteScalar<Z>(command);
            }
        }

        //public static Z ExecCommand<Z>(string command, string stringName)
        //{
        //    using (var connection = new SqlConnection(ConfigurationManager.ConnectionStrings[stringName].ConnectionString))
        //    {
        //        connection.Open();
        //        return connection.ExecuteScalar<Z>(command);
        //    }
        //}

        public bool Exist(string tableName, string columnName, string value)
        {
            try
            {
                using (var connection = new SqlConnection(ConfigurationManager.ConnectionStrings[ConnectionName].ConnectionString))
                {
                    connection.Open();
                    return connection.ExecuteScalar<int>(DapperSentenceGenerator<string>.CreateExist(tableName, columnName), new { value = value }) > 0;
                }
            }
            catch (Exception e)
            {
                throw new Exception(String.Format("Exist method experienced a SQL exception"));
            }

        }

        public int NextVal(string tableName, string columnName)
        {
            try
            {
                using (var connection = new SqlConnection(ConfigurationManager.ConnectionStrings[ConnectionName].ConnectionString))
                {
                    connection.Open();
                    return connection.ExecuteScalar<int>(DapperSentenceGenerator<string>.NextVal(tableName, columnName));
                }
            }
            catch (Exception e)
            {
                throw new Exception(String.Format("Exist method experienced a SQL exception"));
            }

        }

    }
}
