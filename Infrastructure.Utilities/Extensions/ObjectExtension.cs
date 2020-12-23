namespace F2X.Interoperabilidad.Operador.Infrastructure.Helper.Utilities.Extensions
{
    public static class ObjectExtension
    {
        public static T Create<T>(this T @this) where T : class, new()
        {
            return Utility<T>.Create();
        }
    }
}
