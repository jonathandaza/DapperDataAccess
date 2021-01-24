using Infrastructure.Helper.Utilities.Extensions;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text;
using System.Linq;
using System.IO;
using Ionic.Zip;
using System;

namespace Infrastructure.Utilities
{
    public static class Zip
    {
        const string FILE_JSON = ".json";
        const string FILE_DOES_NOT_EXIST = "File does not exist.";
        const string ZIP_FILE_NO_FOUND = "Zip file was not found";

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="path"></param>
        /// <returns></returns>
        public static T DecompressJsonFile<T>(string path)
        {
            FileInfo fileInfo = null;
            IEnumerable<ZipEntry> zipFile;
            T result;

            try
            {
                fileInfo = new FileInfo(path);
                if (!fileInfo.Exists)
                    throw new FileNotFoundException($"{FILE_DOES_NOT_EXIST}. {path}");

                using (var zip = ZipFile.Read(fileInfo.FullName))
                {
                    zip.ExtractAll(Path.GetTempPath(), ExtractExistingFileAction.OverwriteSilently);
                    zipFile = zip.Where(c => c.FileName.EndsWith(FILE_JSON));
                }

                if (zipFile.IsNullOrEmpty())
                    throw new FileNotFoundException("Any JSON file was found over the zip file");

                if (zipFile.Count() > 1)
                    throw new ArgumentOutOfRangeException(path, "There are more than one JSON file into the zip file");

                fileInfo = new FileInfo(Path.Combine(Path.GetTempPath(), $"{(zipFile.FirstOrDefault().FileName)}"));
                if (!fileInfo.Exists)
                    throw new FileNotFoundException($"{ZIP_FILE_NO_FOUND}. {fileInfo.FullName}");

                result = Helper.ReadFileJson<T>(fileInfo.FullName);
            }
            finally
            {
                if (!fileInfo.Exists)
                    fileInfo.Delete();
            }

            return result;
        }

        public static T DecompressJsonFile<T>(string path, string extensionFile)
        {
            FileInfo fileInfo = null;
            IEnumerable<ZipEntry> zipFile;
            T result;

            try
            {
                fileInfo = new FileInfo(path);
                if (!fileInfo.Exists)
                    throw new FileNotFoundException($"{FILE_DOES_NOT_EXIST}. {path}");

                using (var zip = ZipFile.Read(fileInfo.FullName))
                {
                    zip.ExtractAll(Path.GetTempPath(), ExtractExistingFileAction.OverwriteSilently);
                    zipFile = zip.Where(c => c.FileName.EndsWith(extensionFile));
                }

                if (zipFile.IsNullOrEmpty())
                    throw new FileNotFoundException("Any JSON file was found over the zip file");

                if (zipFile.Count() > 1)
                    throw new ArgumentOutOfRangeException(path, "There are more than one JSON file into the zip file");

                fileInfo = new FileInfo(Path.Combine(Path.GetTempPath(), $"{(zipFile.FirstOrDefault().FileName)}"));
                if (!fileInfo.Exists)
                    throw new FileNotFoundException($"{ZIP_FILE_NO_FOUND}. {fileInfo.FullName}");

                result = Helper.ReadFileJson<T>(fileInfo.FullName);
            }
            finally
            {
                if (!fileInfo.Exists)
                    fileInfo.Delete();
            }

            return result;
        }

        public static T DecompressJsonFileFromFileName<T>(string path, string fileName)
        {
            FileInfo fileInfo = null;
            IEnumerable<ZipEntry> zipFile;
            T result;

            try
            {
                fileInfo = new FileInfo(path);
                if (!fileInfo.Exists)
                    throw new FileNotFoundException($"{FILE_DOES_NOT_EXIST}. {path}");

                using (var zip = ZipFile.Read(fileInfo.FullName))
                {
                    zip.ExtractAll(Path.GetTempPath(), ExtractExistingFileAction.OverwriteSilently);
                    zipFile = zip.Where(c => c.FileName.Contains(fileName)).ToList();
                }

                if (zipFile.IsNullOrEmpty())
                    throw new FileNotFoundException($"Any file was found over the zip file with the name file {fileName}");

                if (zipFile.Count() > 1)
                    throw new ArgumentOutOfRangeException(path, $"There are more than one file which is called {fileName}");

                string zipFileName = zipFile.FirstOrDefault().FileName;

                fileInfo = new FileInfo(Path.Combine(Path.GetTempPath(), $"{zipFileName}"));
                if (!fileInfo.Exists)
                    throw new FileNotFoundException($"{ZIP_FILE_NO_FOUND}. {zipFileName}");

                result = Helper.ReadFileJson<T>(fileInfo.FullName);
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
        public static DirectoryInfo Decompress(string path, string pathExtractTo)
        {
            FileInfo fileInfo = null;
            DirectoryInfo result;
            DirectoryInfo directoryInfo = null;

            try
            {
                fileInfo = new FileInfo(path);
                if (!fileInfo.Exists)
                    throw new FileNotFoundException($"{FILE_DOES_NOT_EXIST}. {path}");

                directoryInfo = new DirectoryInfo(pathExtractTo);
                if (!directoryInfo.Exists)
                    directoryInfo.Create();

                using (var zip = ZipFile.Read(fileInfo.FullName))
                {
                    zip.ExtractAll(pathExtractTo, ExtractExistingFileAction.OverwriteSilently);
                }

                result = directoryInfo;
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
        public static T Decompress<T>(string path, string password)
        {
            var fileInfo = new FileInfo(path);
            if (!fileInfo.Exists)
                throw new FileNotFoundException($"{FILE_DOES_NOT_EXIST}. {path}");

            using (var zip = ZipFile.Read(fileInfo.FullName))
            {
                zip.Password = password;
                zip.ExtractAll(Path.GetTempPath(), ExtractExistingFileAction.OverwriteSilently);
            }

            fileInfo = new FileInfo(Path.Combine(Path.GetTempPath(), $"{Path.GetFileNameWithoutExtension(fileInfo.Name)}.txt"));
            if (!fileInfo.Exists)
                throw new FileNotFoundException($"{ZIP_FILE_NO_FOUND}. {fileInfo.FullName}");

            T result = Helper.ReadFileJson<T>(fileInfo.FullName);

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
        public static T Decompress<T>(FileInfo fileInfo)
        {
            using (var zip = ZipFile.Read(fileInfo.FullName))
            {
                zip.ExtractAll(Path.GetTempPath(), ExtractExistingFileAction.OverwriteSilently);
            }

            fileInfo = new FileInfo(Path.Combine(Path.GetTempPath(), $"{Path.GetFileNameWithoutExtension(fileInfo.Name)}.txt"));
            if (!fileInfo.Exists)
                throw new FileNotFoundException($"No se encontró el archivo descomprimido {fileInfo.FullName}.");

            T result = Helper.ReadFileJson<T>(fileInfo.FullName);

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
        public static T Decompress<T>(FileInfo fileInfo, string password)
        {
            using (var zip = ZipFile.Read(fileInfo.FullName))
            {
                zip.Password = password;
                zip.ExtractAll(Path.GetTempPath(), ExtractExistingFileAction.OverwriteSilently);
            }

            fileInfo = new FileInfo(Path.Combine(Path.GetTempPath(), $"{Path.GetFileNameWithoutExtension(fileInfo.Name)}.txt"));
            if (!fileInfo.Exists)
                throw new FileNotFoundException($"No se encontró el archivo descomprimido {fileInfo.FullName}.");

            T result = Helper.ReadFileJson<T>(fileInfo.FullName);

            if (!fileInfo.Exists)
                fileInfo.Delete();

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string Compress<T>(T source, string path)
        {
            string pathFileZip = String.Empty;

            var directoryInfoZip = new DirectoryInfo(Path.GetDirectoryName(path));
            if (!directoryInfoZip.Exists)
                directoryInfoZip.Create();

            var fileTxt = String.Format(Path.GetFileNameWithoutExtension(path));

            Helper.CreateTXTFile<T>(source, Path.GetTempPath(), fileTxt, out string pathName);

            if (String.IsNullOrEmpty(pathName))
                throw new FileNotFoundException($"No se encontró el arcvhivo en la ruta: { Path.Combine(Path.GetTempPath(), fileTxt) }");

            using (ZipFile zip = new ZipFile())
            {
                pathFileZip = Path.Combine(Path.GetDirectoryName(directoryInfoZip.FullName), $"{fileTxt}.zip");
                zip.AddFile(pathName, String.Empty);
                zip.Save(pathFileZip);
            }

            var fileInfo = new FileInfo(pathName);
            if (fileInfo.Exists)
                fileInfo.Delete();

            return pathFileZip;
        }

        public static string Compress<T>(T source, DirectoryInfo directoryInfoZip)
        {
            string pathFileZip = String.Empty;

            var fileTxt = String.Format(Path.GetFileNameWithoutExtension(directoryInfoZip.Name));

            Helper.CreateTXTFile<T>(source, Path.GetTempPath(), fileTxt, out string pathName);

            if (String.IsNullOrEmpty(pathName))
                throw new FileNotFoundException($"No se encontró el arcvhivo en la ruta: { Path.Combine(Path.GetTempPath(), fileTxt) }");

            using (ZipFile zip = new ZipFile())
            {
                pathFileZip = Path.Combine(Path.GetDirectoryName(directoryInfoZip.FullName), $"{fileTxt}.zip");
                zip.AddFile(pathName, String.Empty);
                zip.Save(pathFileZip);
            }

            var fileInfo = new FileInfo(pathName);
            if (fileInfo.Exists)
                fileInfo.Delete();

            return pathFileZip;
        }

        public static string Compress<T>(T source, DirectoryInfo directoryInfoZip, string password)
        {
            string pathFileZip = String.Empty;

            var fileTxt = String.Format(Path.GetFileNameWithoutExtension(directoryInfoZip.Name));

            Helper.CreateTXTFile<T>(source, Path.GetTempPath(), fileTxt, out string pathName);

            if (String.IsNullOrEmpty(pathName))
                throw new FileNotFoundException($"No se encontró el arcvhivo en la ruta: { Path.Combine(Path.GetTempPath(), fileTxt) }");

            using (ZipFile zip = new ZipFile())
            {
                pathFileZip = Path.Combine(Path.GetDirectoryName(directoryInfoZip.FullName), $"{fileTxt}.zip");
                zip.Password = password;
                zip.AddFile(pathName, String.Empty);
                zip.Save(pathFileZip);
            }

            var fileInfo = new FileInfo(pathName);
            if (fileInfo.Exists)
                fileInfo.Delete();

            return pathFileZip;
        }

        public static string Compress<T>(T source, string path, string password)
        {
            string pathFileZip = String.Empty;

            var directoryInfoZip = new DirectoryInfo(Path.GetDirectoryName(path));
            if (!directoryInfoZip.Exists)
                directoryInfoZip.Create();

            var fileTxt = String.Format(Path.GetFileNameWithoutExtension(path));

            Helper.CreateTXTFile<T>(source, Path.GetTempPath(), fileTxt, out string pathName);

            if (String.IsNullOrEmpty(pathName))
                throw new FileNotFoundException($"No se encontró el arcvhivo en la ruta: { Path.Combine(Path.GetTempPath(), fileTxt) }");

            using (ZipFile zip = new ZipFile())
            {
                pathFileZip = Path.Combine(Path.GetDirectoryName(directoryInfoZip.FullName), $"{fileTxt}.zip");
                zip.Password = password;
                zip.AddFile(pathName, String.Empty);
                zip.Save(pathFileZip);
            }

            var fileInfo = new FileInfo(pathName);
            if (fileInfo.Exists)
                fileInfo.Delete();

            return pathFileZip;
        }

        public static void Compress<T>(T source, DirectoryInfo directoryInfoZip, out FileInfo fileInfoFileZip)
        {
            string pathFileZip = String.Empty;

            var fileTxt = String.Format(Path.GetFileNameWithoutExtension(directoryInfoZip.Name));

            Helper.CreateTXTFile<T>(source, Path.GetTempPath(), fileTxt, out string pathName);

            if (String.IsNullOrEmpty(pathName))
                throw new FileNotFoundException($"No se encontró el arcvhivo en la ruta: { Path.Combine(Path.GetTempPath(), fileTxt) }");

            using (ZipFile zip = new ZipFile())
            {
                pathFileZip = Path.Combine(Path.GetDirectoryName(directoryInfoZip.FullName), $"{fileTxt}.zip");
                zip.AddFile(pathName, String.Empty);
                zip.Save(pathFileZip);
            }

            var fileInfo = new FileInfo(pathName);
            if (fileInfo.Exists)
                fileInfo.Delete();

            fileInfoFileZip = new FileInfo(pathFileZip);
        }

        public static void Compress<T>(T source, string path, string password, out FileInfo fileInfoFileZip)
        {
            string pathFileZip = String.Empty;

            var directoryInfoZip = new DirectoryInfo(Path.GetDirectoryName(path));
            if (!directoryInfoZip.Exists)
                directoryInfoZip.Create();

            var fileTxt = String.Format(Path.GetFileNameWithoutExtension(path));

            Helper.CreateTXTFile<T>(source, Path.GetTempPath(), fileTxt, out string pathName);

            if (String.IsNullOrEmpty(pathName))
                throw new FileNotFoundException($"No se encontró el arcvhivo en la ruta: { Path.Combine(Path.GetTempPath(), fileTxt) }");

            using (ZipFile zip = new ZipFile())
            {
                pathFileZip = Path.Combine(Path.GetDirectoryName(directoryInfoZip.FullName), $"{fileTxt}.zip");
                zip.Password = password;
                zip.AddFile(pathName, String.Empty);
                zip.Save(pathFileZip);
            }

            var fileInfo = new FileInfo(pathName);
            if (fileInfo.Exists)
                fileInfo.Delete();

            fileInfoFileZip = new FileInfo(pathFileZip);
        }

        public static void Compress<T>(T source, DirectoryInfo directoryInfoZip, string password, out FileInfo fileInfoFileZip)
        {
            string pathFileZip = String.Empty;

            var fileTxt = String.Format(Path.GetFileNameWithoutExtension(directoryInfoZip.Name));

            Helper.CreateTXTFile<T>(source, Path.GetTempPath(), fileTxt, out string pathName);

            if (String.IsNullOrEmpty(pathName))
                throw new FileNotFoundException($"No se encontró el archivo en la ruta: { Path.Combine(Path.GetTempPath(), fileTxt) }");

            using (ZipFile zip = new ZipFile())
            {
                pathFileZip = Path.Combine(Path.GetDirectoryName(directoryInfoZip.FullName), $"{fileTxt}.zip");
                zip.Password = password;
                zip.AddFile(pathName, String.Empty);
                zip.Save(pathFileZip);
            }

            var fileInfo = new FileInfo(pathName);
            if (fileInfo.Exists)
                fileInfo.Delete();

            fileInfoFileZip = new FileInfo(pathFileZip);
        }
    }
}
