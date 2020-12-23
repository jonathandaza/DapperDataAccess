﻿using System;
using System.Collections;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace F2X.Interoperabilidad.Operador.Infrastructure.Helper.Utilities.Extensions
{
    public static class SerializerDeserializerExtensions
    {

        // why is this Hashtable? due to the threading semantics!
        //private static readonly Hashtable serializerCache = new Hashtable();

        /// <summary>
        /// Convierte un objeto a un array de bytes, interfaces de colecciones no las puede convertir como IEnumerable, ICollection, IList, etc.
        /// </summary>
        /// <typeparam name="T">Tipo de dato a ser convertido</typeparam>
        /// <param name="object">Valor a convertir array de bytes</param>
        /// <returns>Devuelve el respectivo array de bytes, en caso que njo se pueda convertir, retorna un default(byte[])</returns>
        public static byte[] Serializer<T>(this T @object)
        {
            if (@object == null)
            {
                throw new ArgumentNullException();
            }

            //var defaultNamespace = Guid.NewGuid().ToString("N");

            //Type type = @object.GetType();
            //var cacheKey = new { Type = type, Name = defaultNamespace };
            //XmlSerializer xmlSerializer = (XmlSerializer)serializerCache[cacheKey];
            
            //if (xmlSerializer == null)
            //{
            //    lock (serializerCache)
            //    { // double-checked
            //        xmlSerializer = (XmlSerializer)serializerCache[cacheKey];
            //        if (xmlSerializer == null)
            //        {
            //            xmlSerializer = new XmlSerializer(type, new XmlRootAttribute(defaultNamespace));
            //            serializerCache.Add(cacheKey, xmlSerializer);
            //        }
            //    }
            //}

            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(T), Guid.NewGuid().ToString("N"));

                using (MemoryStream memoryStream = new MemoryStream())
                {
                    using (XmlWriter xmlWriter = XmlWriter.Create(memoryStream))
                    {
                        serializer.Serialize(xmlWriter, @object);

                        return memoryStream.ToArray();
                    }
                }
            }
            catch
            {
                throw;
            }
        }
                
        /// <summary>
        /// Convierte un array de byte a un objeto en especifico
        /// </summary>
        /// <typeparam name="T">Tipo de datos a ser retornado</typeparam>
        /// <param name="byteArray">Array de bytes</param>
        /// <returns>Devuelve una instancia del tipo enviado en T</returns>       

        public static T Deserializer<T>(this byte[] @byte)
        {
            try
            {
                if (@byte == null || @byte.Length == 0)
                {
                    throw new InvalidOperationException();
                }

                XmlSerializer serializer = new XmlSerializer(typeof(T));

                using (MemoryStream memoryStream = new MemoryStream(@byte))
                {
                    using (XmlReader xmlReader = XmlReader.Create(memoryStream))
                    {
                        return (T)serializer.Deserialize(xmlReader);
                    }
                }

            }
            catch
            {
                throw;
            }
        }


        
    }
}
