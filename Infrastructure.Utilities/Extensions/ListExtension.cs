using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System;

namespace Infrastructure.Helper.Utilities.Extensions
{
    public static class ListExtension
    {
        /// <summary>
        /// Método para comprar dos listas
        /// </summary>
        /// <typeparam name="T">Párametro genérico</typeparam>
        /// <param name="listA">Primera lista</param>
        /// <param name="listB">Segunda lista</param>
        /// <returns>Si devuelve 0 items quiere decir que las lista son iguales</returns>
        public static List<Tuple<int, string>> Equatable<T>(this List<T> listA, List<T> listB)
        {
            if (listB == null) return null;

            var differences = (from t in listA where !listB.Contains(t) select new Tuple<int, string>(1, t.ToString())).ToList();
            differences.AddRange(from t in listB where !listA.Contains(t) select new Tuple<int, string>(2, t.ToString()));

            return differences;
        }

        /// <summary>
        /// Método para validar sí la lista viene nula o vacia
        /// </summary>
        /// <typeparam name="T">Tipo de dato de la lista</typeparam>
        /// <param name="source">Objeto de tipo lista</param>
        /// <returns>Devuelve true si la lista es nula o vacia</returns>
        public static bool IsNullOrEmpty<T>(this IEnumerable<T> source)
        {
            if ((source == null) || (!source.Any()))
                return true;

            return false;
        }

        /// <summary>
        /// Método para convertir una listas genercia a un objeto de tipo <see cref="DataTable"/>
        /// </summary>
        /// <typeparam name="T">Objeto de tipo <see cref="DataTable"/> convertido</typeparam>
        /// <param name="data">Lista generica a convertir</param>
        /// <returns></returns>
        public static DataTable AsDataTable<T>(this IEnumerable<T> data)
        {
            PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(typeof(T));
            var table = new DataTable();
            foreach (PropertyDescriptor prop in properties)
                table.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);

            foreach (T item in data)
            {
                DataRow row = table.NewRow();
                foreach (PropertyDescriptor prop in properties)
                    row[prop.Name] = prop.GetValue(item) ?? DBNull.Value;
                table.Rows.Add(row);
            }

            return table;
        }
    }
}
