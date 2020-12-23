using System;

namespace Infrastructure.Helper.Utilities.Extensions
{
    public static class DateTimeExtension
    {
        /// <summary>
        /// Convierte una fecha cualquiera a una fecha UTC, Si no tiene definida la zona horaria toma la fecha como si fuera UTC
        /// </summary>
        /// <param name="fecha">Fecha con o sin zona horaria definida</param>
        /// <returns>Retorna objeto <see cref="DateTime"/> con la Fecha en UTC </returns>
        public static DateTime ConvertirAZonaUtc(this DateTime fecha)
        {
            return fecha.Kind == DateTimeKind.Unspecified ?
                DateTime.SpecifyKind(fecha, DateTimeKind.Utc) :
                fecha.ToUniversalTime();
        }
    }
}
