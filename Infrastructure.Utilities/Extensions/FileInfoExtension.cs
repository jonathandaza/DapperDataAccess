using System.IO;
using System;

namespace F2X.Interoperabilidad.Operador.Infrastructure.Helper.Utilities.Extensions
{
    public static class FileInfoExtension
    {
        public static string RelativePath(this FileInfo fileInfo, string valueToReplace)
        {
            return fileInfo.FullName.Replace(valueToReplace, String.Empty);
        }
    }
}
