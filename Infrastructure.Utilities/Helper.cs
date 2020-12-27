﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.OleDb;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Utilities
{
    public class Helper
    {
        const string FILE_XLS = ".xls";
        const string FILE_XLSX = ".xlsx";
        const string FILE_CSV = ".csv";


        /// <summary>
        /// Gets the object description of type <see cref="Enum"/>
        /// </summary>
        /// <param name="enumValue">Enum value</param>
        /// <returns>Returns object description <see cref="Enum"/></returns>
        public static string GetEnumDescripcion(Enum enumValue)
        {
            FieldInfo fi = enumValue.GetType().GetField(enumValue.ToString());
            var atributos = (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);

            if (atributos != null && atributos.Length > 0)
                return atributos[0].Description;

            return enumValue.ToString();
        }

        /// <summary>
        /// Gets the value from enum description of type <see cref="Enum"/>
        /// </summary>
        /// <param name="description">Enum description</param>
        /// <returns>Returns the enum value, type: <see cref="Enum"/></returns>
        public static T GetDescriptionValue<T>(string description)
        {
            MemberInfo[] fis = typeof(T).GetFields();

            foreach (var fi in fis)
            {
                var atributos = (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);

                if (atributos != null && atributos.Length > 0 && atributos[0].Description == description)
                    return (T)Enum.Parse(typeof(T), fi.Name);
            }

            throw new Exception("Description not found");
        }

        /// <summary>
        /// Converts <see cref="DateTime"/> object from a date format and string.
        /// </summary>
        /// <param name="date"> A string that contains a date and time to convert. </param>
        /// <param name="format">A format specifier that defines the required format of date.</param>
        /// <returns>Returns an object that is equivalent to the date and time contained in date, as specified by format.</returns>
        public static DateTime GetDateTime(string date, string format)
        {
            return DateTime.ParseExact(date, format, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Reads a file specifically with extension such as: .csv, .xls, .xlsx, and then convert it to <see cref="DataTable"/> object
        /// </summary>
        /// <param name="filePath">The file to open for reading.</param>
        /// <returns>Returns a <see cref="DataTable"/> object containing the whole information of the file</returns>
        public static DataTable ReadFile(string filePath)
        {
            DataTable returnedDt = null;

            if (filePath.EndsWith(FILE_CSV))
                returnedDt = ReadCSVToTable(filePath);

            if (filePath.EndsWith(FILE_XLS) || filePath.EndsWith(FILE_XLSX))
                returnedDt = ReadExcelSheet(filePath);

            return returnedDt;
        }

        /// <summary>
        /// Reads a CSV file, and then convert it to <see cref="DataTable"/> object
        /// </summary>
        /// <param name="filePath">The file to open for reading.</param>
        /// <returns>Returns a <see cref="DataTable"/> object containing the whole information of the file</returns>
        static DataTable ReadCSVToTable(string filePath)
        {
            const char comma = ',';
            const char semicolon = ';';
            var dt = new DataTable();

            try
            {
                string[] csvRows = File.ReadAllLines(filePath, Encoding.GetEncoding("ISO-8859-1"));

                int columComma = csvRows[0].Split(comma).Count();
                int columSemicolon = csvRows[0].Split(semicolon).Count();

                char targetSplitChar = columComma > columSemicolon ? comma : semicolon;

                string[] arrColumns = csvRows[0].Split(targetSplitChar);

                foreach (string arrColumn in arrColumns)
                {
                    dt.Columns.Add(new DataColumn
                    {
                        DataType = typeof(string),
                        ColumnName = arrColumn.Trim(),
                        AllowDBNull = true
                    });
                }

                for (int i = 1; i < csvRows.Length; i++)
                {
                    string[] fields = csvRows[i].Split(targetSplitChar);
                    DataRow row = dt.NewRow();
                    row.ItemArray = fields;
                    dt.Rows.Add(row);
                }
            }
            catch (Exception)
            {
                dt.Clear();
                throw;
            }

            return dt;
        }

        /// <summary>
        /// Reads a XLSX or XLSX file, and then convert it to <see cref="DataTable"/> object
        /// It is necessary to download a driver in order to the method works fine 
        ///     https://www.microsoft.com/en-nz/download/details.aspx?id=13255        
        /// </summary>
        /// <param name="excelFile">The file to open for reading.</param>
        /// <returns>Returns a <see cref="DataTable"/> object containing the whole information of the file</returns>
        static DataTable ReadExcelSheet(string excelFile)
        {
            string connectionString = String.Empty;

            if (excelFile.Trim().EndsWith(FILE_XLSX))
            {
                connectionString = String.Format("Provider=Microsoft.ACE.OLEDB.12.0;Data Source={0};Extended Properties=\"Excel 12.0 Xml;HDR=YES;IMEX=1\";", excelFile);
            }
            else if (excelFile.Trim().EndsWith(FILE_XLS))
            {
                connectionString = String.Format("Provider=Microsoft.Jet.OLEDB.4.0;Data Source={0};Extended Properties=\"Excel 8.0;HDR=Yes;IMEX=1\";", excelFile);
            }
            try
            {
                using (var connection = new OleDbConnection(connectionString))
                {
                    connection.Open();

                    var dtTable = connection.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, new Object[] { null, null, null, "TABLE" });

                    if (dtTable != null)
                    {
                        var sheet1 = dtTable.Rows[0]["TABLE_NAME"].ToString();

                        string strSQL = "SELECT * FROM [" + sheet1 + "]";

                        var cmd = new OleDbCommand(strSQL, connection);
                        var ds = new DataSet();
                        var da = new OleDbDataAdapter(cmd);
                        da.Fill(ds);

                        return ds.Tables[0];
                    }
                }
            }
            catch
            {
                throw;
            }
            return null;
        }
    }
}
