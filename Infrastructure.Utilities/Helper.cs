using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Utilities
{
    public class Helper
    {
        /// <summary>
        /// Gets the object description of type <see cref="Enum"/>
        /// </summary>
        /// <param name="enumValue">Enum value</param>
        /// <returns>Return object description <see cref="Enum"/></returns>
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
        /// <returns>Return the enum value, type: <see cref="Enum"/></returns>
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
        /// <returns>An object that is equivalent to the date and time contained in date, as specified by format.</returns>
        public static DateTime GetDateTime(string date, string format)
        {
            return DateTime.ParseExact(date, format, CultureInfo.InvariantCulture);
        }
    }
}
