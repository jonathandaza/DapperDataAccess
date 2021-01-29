
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.OleDb;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;

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

        /// <summary>
        /// Creates CSV file from a <see cref="DataTable"/> object
        /// </summary>
        /// <param name="dtTable">Data fro being converted to CSV file</param>
        /// <param name="filePath">file path for being created.</param>
        public static void CreateCSVFile(DataTable dtTable, string filePath)
        {
            const string semicolon = ";";

            using (var outfile = new StreamWriter(filePath, true, Encoding.GetEncoding("ISO-8859-1")))
            {                
                var arrColumnas = new string[dtTable.Columns.Count];
                var sbHead = new StringBuilder();

                for (int i = 0; i < dtTable.Columns.Count; i++)
                {
                    arrColumnas[i] = dtTable.Columns[i].ColumnName;
                }

                sbHead.Append(String.Join(semicolon, arrColumnas));
                outfile.WriteLine(sbHead.ToString());
                
                foreach (DataRow row in dtTable.Rows)
                {
                    var sb = new StringBuilder();
                    sb.Append(String.Join(semicolon, row.ItemArray));
                    outfile.WriteLine(sb.ToString());
                }
            }
        }

        /// <summary>
        /// Creates CSV file from a collection (<see cref="IEnumerable<T>"/>)
        /// </summary>
        /// <typeparam name="T">Type of object contained in the collection</typeparam>
        /// <param name="separator">chacter which each line is going to be separed</param>
        /// <param name="objectlist">Collection containing the whole information to be created to CSV file </param>
        /// <param name="fullPath">file path (including file name) where the file will be created.</param>
        public static void CreateCSVFile<T>(string separator, IEnumerable<T> objectlist, string fullPath)
        {
            const string fileExtension = ".csv";

            Type t = typeof(T);
            PropertyInfo[] fields = t.GetProperties();

            string header = String.Join(separator, fields.Select(f => f.Name).ToArray());

            StringBuilder csvdata = new StringBuilder();
            csvdata.AppendLine(header);

            foreach (var o in objectlist)
                csvdata.AppendLine(ToCsvFields(separator, fields, o));

            using (StreamWriter file = File.CreateText($"{(fullPath.Contains(fileExtension) ? fullPath : fullPath + fileExtension)}"))
            {
                file.WriteLine(csvdata.ToString());
            }
        }

        /// <summary>
        /// Creates CSV file from a collection (<see cref="IEnumerable<T>"/>)
        /// </summary>
        /// <typeparam name="T">Type of object contained in the collection</typeparam>
        /// <param name="separator">chacter which each line is going to be separed</param>
        /// <param name="objectlist">Collection containing the whole information to be created to CSV file </param>
        /// <param name="folder">Folder path where the file will be created</param>
        /// <param name="fileName">File name (without extension)</param>
        /// <param name="pathName">Contains the whole path where the file will be written</param>
        public static void CreateCSVFile<T>(string separator, IEnumerable<T> objectlist, string folder, string fileName, out string pathName)
        {
            const string fileExtension = ".csv";
            try
            {                
                Type t = typeof(T);
                PropertyInfo[] fields = t.GetProperties();

                string header = string.Join(separator, fields.Select(f => f.Name).ToArray());

                StringBuilder csvdata = new StringBuilder();
                csvdata.AppendLine(header);

                foreach (var o in objectlist)
                    csvdata.AppendLine(ToCsvFields(separator, fields, o));

                pathName = Path.Combine(folder, $"{(fileName.Contains(fileExtension) ? fileName : fileName + fileExtension)}");

                var directoryInfo = new DirectoryInfo(folder);
                if (!directoryInfo.Exists)
                    directoryInfo.Create();

                using (StreamWriter file = File.CreateText($"{pathName}"))
                {
                    file.WriteLine(csvdata.ToString());
                }
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Reads each field over "o" object and, will convert it to string
        /// </summary>
        /// <param name="separator">Chacter which each line is going to be separed</param>
        /// <param name="fields">Fields to be found on "o"</param>
        /// <param name="o">Object values to be written</param>
        /// <returns>Returns a <see cref="String"/> object containing the object values "o"</returns>
        public static string ToCsvFields(string separator, PropertyInfo[] fields, object o)
        {
            StringBuilder linie = new StringBuilder();

            foreach (var f in fields)
            {
                if (linie.Length > 0)
                    linie.Append(separator);

                var x = f.GetValue(o);

                if (x != null)
                    linie.Append(x.ToString());
            }

            return linie.ToString();
        }

        /// <summary>
        /// Creates TXT file from a <see cref="T"/> object
        /// </summary>
        /// <typeparam name="T">Type of object containing the data in order to be written</typeparam>
        /// <param name="source">Object containing the data in order to be written</param>
        /// <param name="folder">Folder path where the file will be created</param>
        /// <param name="fileName">File name (without extension)</param>
        /// <param name="pathName">Contains the whole path where the file will be written</param>
        public static void CreateTXTFile<T>(T source, string folder, string fileName, out string pathName)
        {
            const string fileExtension = ".txt";
            try
            {
                pathName = Path.Combine(folder, $"{(fileName.Contains(fileExtension) ? fileName : fileName + fileExtension)}");

                var directoryInfo = new DirectoryInfo(folder);
                if (!directoryInfo.Exists)
                    directoryInfo.Create();

                using (StreamWriter file = File.CreateText(pathName))
                {
                    JsonSerializer serializer = new JsonSerializer();

                    //serialize object directly into file stream
                    serializer.Serialize(file, source);
                }
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Reads a JSON file from a path
        /// </summary>
        /// <typeparam name="T">Type of object to deserialize the JSON</typeparam>
        /// <param name="path">file path (including file name) where the file will be read.</param>
        /// <returns>Returns a <see cref="T"/> object containing the object already deserialized</returns>
        public static T ReadFileJson<T>(string path)
        {
            var fileInfo = new FileInfo(path);
            if (!fileInfo.Exists)
                throw new FileNotFoundException($"The JSON file was not found . Path: {fileInfo.FullName}.");
            T result;

            using (StreamReader r = new StreamReader(path))
            {
                string json = r.ReadToEnd();
                result = JsonConvert.DeserializeObject<T>(json);
            }

            return result;
        }

        /// <summary>
        /// Gets the Ip address of the host
        /// </summary>
        /// <returns>Returns host's ip address where the program is running</returns>
        public static string GetIpAddress()
        {
            string ipAddress = null;
            string hostname = Environment.MachineName;
            IPHostEntry host = Dns.GetHostEntry(hostname);

            foreach (var ip in host.AddressList.Where(ip => ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork))
            {
                ipAddress = Convert.ToString(ip);
            }
            return ipAddress;
        }

        /// <summary>
        /// Creates a file as of its bytes
        /// </summary>
        /// <param name="pathName">Contains the whole path where the file will be written</param>
        /// <param name="bytes">Bytes to write the file</param>
        /// <returns>Resturns whether the file was created or not</returns>
        public static bool CreateFileFromItsBytes(string pathName, byte[] bytes)
        {
            if (string.IsNullOrWhiteSpace(pathName) || bytes == null || bytes.Length == 0)
                return false;

            if (!Directory.Exists(Path.GetPathRoot(pathName)))
                return false;

            var directory = Path.GetDirectoryName(pathName);
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            using (var fs = new FileStream(pathName, FileMode.Create))
            {
                fs.Write(bytes, 0, bytes.Length);
                fs.Close();
            }
            return true;
        }

        /// <summary>
        /// Converts a string to object <see cref="DateTime"/>
        /// </summary>
        /// <param name="date">string of the date</param>
        /// <param name="format">Format to convert the 'date' string </param>
        /// <returns>Returns the date (<see cref="DateTime"/>) already converted, if it returns null, it was not possible to convert date</returns>
        public static DateTime? ConvertStringToDate(string date, string format)
        {
            if (!DateTime.TryParseExact(date, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime convertedDate))
            {
                return null;
            }

            return convertedDate;
        }

        /// <summary>
        /// Converts a string to object <see cref="DateTime"/> UTC
        /// </summary>
        /// <param name="date">string of the date</param>
        /// <param name="format">Format to convert the 'date' string </param>
        /// <returns>Returns the date (<see cref="DateTime"/>) already converted, if it returns null, it was not possible to convert date</returns>
        public static DateTime? ConvertStringToDateUtc(string date, string format)
        {
            if (!DateTime.TryParseExact(date, format, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeLocal, out DateTime convertedDate))
            {
                return null;
            }
            return convertedDate;
        }
    }   
}

