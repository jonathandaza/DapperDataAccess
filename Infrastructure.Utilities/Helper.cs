using Ionic.Zip;
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

        public static T ReadFileTxt<T>(string path)
        {
            var fileInfo = new FileInfo(path);
            if (!fileInfo.Exists)
                throw new FileNotFoundException($"No se encontró archivo para ser deserializado a JSON. Archivo: {fileInfo.FullName}.");
            T result;

            using (StreamReader r = new StreamReader(path))
            {
                string json = r.ReadToEnd();
                result = JsonConvert.DeserializeObject<T>(json);
            }

            return result;
        }

        public static string GetIpAddress()
        {
            string ipAddress = null;
            string hostname = Environment.MachineName; IPHostEntry host = Dns.GetHostEntry(hostname);

            foreach (var ip in host.AddressList.Where(ip => ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork))
            {
                ipAddress = Convert.ToString(ip);
            }
            return ipAddress;
        }

        public static TResult DeserializarJson<TResult>(string json)
        {
            //return JsonConvert.DeserializeObject<TResult>(json);
            throw new NotImplementedException();
        }

        public static string SerializarAJson<T>(T source)
        {
            //return JsonConvert.SerializeObject(source);
            throw new NotImplementedException();
        }

        public static bool CrearImagenDeBytes(string rutaArchivo, byte[] bytesImagen)
        {
            if (string.IsNullOrWhiteSpace(rutaArchivo) || bytesImagen == null || bytesImagen.Length == 0)
                return false;

            if (!Directory.Exists(Path.GetPathRoot(rutaArchivo)))
                return false;

            var rutaDirectorio = Path.GetDirectoryName(rutaArchivo);
            if (!Directory.Exists(rutaDirectorio))
                Directory.CreateDirectory(rutaDirectorio);

            using (var fs = new FileStream(rutaArchivo, FileMode.Create))
            {
                fs.Write(bytesImagen, 0, bytesImagen.Length);
                fs.Close();
            }
            return true;
        }

        public static DateTime? ConvertStringToDate(string fechaEmision, string formato)
        {
            DateTime fechaConvertida;
            if (!DateTime.TryParseExact(fechaEmision, formato, CultureInfo.InvariantCulture, DateTimeStyles.None, out fechaConvertida))
            {
                return null;
            }
            return fechaConvertida;
        }

        public static DateTime? ConvertStringToDateUtc(string fechaEmision, string formato)
        {
            DateTime fechaConvertida;
            if (!DateTime.TryParseExact(fechaEmision, formato, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeLocal, out fechaConvertida))
            {
                return null;
            }
            return fechaConvertida;
        }
    }

    public static class Zip
    {
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="path"></param>
        /// <returns></returns>
        public static T DecompressSnapshot<T>(string path)
        {
            FileInfo fileInfo = null;
            T result;

            try
            {
                fileInfo = new FileInfo(path);
                if (!fileInfo.Exists)
                    throw new FileNotFoundException($"File does not exist. {path}");

                using (var zip = ZipFile.Read(fileInfo.FullName))
                {
                    zip.ExtractAll(Path.GetTempPath(), ExtractExistingFileAction.OverwriteSilently);
                }

                fileInfo = new FileInfo(Path.Combine(Path.GetTempPath(), $"{Path.GetFileNameWithoutExtension(fileInfo.Name)}.txt"));
                if (!fileInfo.Exists)
                    throw new FileNotFoundException($"The zip-file was not found. {fileInfo.FullName}");

                result = Helper.ReadFileTxt<T>(fileInfo.FullName);
            }
            finally
            {
                if (!fileInfo.Exists)
                    fileInfo.Delete();
            }

            return result;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <param name="pathExtractTo"></param>
        /// <returns></returns>
        public static FileInfo DecompressSnapshot(string path, string pathExtractTo)
        {
            FileInfo fileInfo = null;
            FileInfo result;
            DirectoryInfo directoryInfo = null;

            try
            {
                fileInfo = new FileInfo(path);
                if (!fileInfo.Exists)
                    throw new FileNotFoundException($"File does not exist. {path}");

                directoryInfo = new DirectoryInfo(pathExtractTo);
                if (!directoryInfo.Exists)
                    directoryInfo.Create();

                using (var zip = ZipFile.Read(fileInfo.FullName))
                {
                    zip.ExtractAll(pathExtractTo, ExtractExistingFileAction.OverwriteSilently);
                }

                result = new FileInfo(Path.Combine(pathExtractTo, $"{Path.GetFileNameWithoutExtension(fileInfo.Name)}.txt"));
            }
            finally
            {
                if (!fileInfo.Exists)
                    fileInfo.Delete();
            }

            if (result.Exists) return result;

            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="path"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public static T DecompressSnapshot<T>(string path, string password)
        {
            var fileInfo = new FileInfo(path);
            if (!fileInfo.Exists)
                throw new FileNotFoundException($"File does not exist. {path}");

            using (var zip = ZipFile.Read(fileInfo.FullName))
            {
                zip.Password = password;
                zip.ExtractAll(Path.GetTempPath(), ExtractExistingFileAction.OverwriteSilently);
            }

            fileInfo = new FileInfo(Path.Combine(Path.GetTempPath(), $"{Path.GetFileNameWithoutExtension(fileInfo.Name)}.txt"));
            if (!fileInfo.Exists)
                throw new FileNotFoundException($"The zip-file was not found. {fileInfo.FullName}");

            T result = Helper.ReadFileTxt<T>(fileInfo.FullName);

            if (!fileInfo.Exists)
                fileInfo.Delete();

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="fileInfo"></param>
        /// <returns></returns>
        public static T DecompressSnapshot<T>(FileInfo fileInfo)
        {
            using (var zip = ZipFile.Read(fileInfo.FullName))
            {
                zip.ExtractAll(Path.GetTempPath(), ExtractExistingFileAction.OverwriteSilently);
            }

            fileInfo = new FileInfo(Path.Combine(Path.GetTempPath(), $"{Path.GetFileNameWithoutExtension(fileInfo.Name)}.txt"));
            if (!fileInfo.Exists)
                throw new FileNotFoundException($"No se encontró el archivo descomprimido {fileInfo.FullName}.");

            T result = Helper.ReadFileTxt<T>(fileInfo.FullName);

            if (!fileInfo.Exists)
                fileInfo.Delete();

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="fileInfo"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public static T DecompressSnapshot<T>(FileInfo fileInfo, string password)
        {
            using (var zip = ZipFile.Read(fileInfo.FullName))
            {
                zip.Password = password;
                zip.ExtractAll(Path.GetTempPath(), ExtractExistingFileAction.OverwriteSilently);
            }

            fileInfo = new FileInfo(Path.Combine(Path.GetTempPath(), $"{Path.GetFileNameWithoutExtension(fileInfo.Name)}.txt"));
            if (!fileInfo.Exists)
                throw new FileNotFoundException($"No se encontró el archivo descomprimido {fileInfo.FullName}.");

            T result = Helper.ReadFileTxt<T>(fileInfo.FullName);

            if (!fileInfo.Exists)
                fileInfo.Delete();

            return result;
        }
    }
}

