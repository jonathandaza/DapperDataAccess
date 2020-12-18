using System;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;

namespace Infraestructure.Api.ModelAnalizer
{
    public class TypeInterpolator<T>
    {
        Type currentType = typeof(T);
        public EntityInfo Entity { get; set; }

        public TypeInterpolator()
        {
            List<EntityInfo> modelSpecifications = currentType.GetCustomAttributes<EntityInfo>().ToList<EntityInfo>();
            if (modelSpecifications.Count > 0)
                Entity = currentType.GetCustomAttributes<EntityInfo>().ToList<EntityInfo>().First<EntityInfo>();
            else
                Entity = new EntityInfo();

            if (string.IsNullOrEmpty(Entity.MappTo))
                Entity.MappTo = currentType.Name;

            Entity.Name = currentType.Name;
            Entity.Fields = new List<EntityInfo>();
            MappFields();
        }

        private void MappFields()
        {
            PropertyInfo[] properties = currentType.GetProperties();
            foreach (var property in properties)
            {
                EntityInfo field = new EntityInfo();
                List<EntityInfo> modelSpecifications = property.GetCustomAttributes<EntityInfo>().ToList<EntityInfo>();
                if (modelSpecifications.Count > 0)
                    field = property.GetCustomAttributes<EntityInfo>().ToList<EntityInfo>().First<EntityInfo>();

                if (string.IsNullOrEmpty(field.MappTo))
                    field.MappTo = property.Name;

                BooleanConversion booleanConversion = MappBooleanConversions(property.Name);
                if (booleanConversion != null)
                {
                    field.TrueValue = booleanConversion.TrueValue;
                    field.FalseValue = booleanConversion.FalseValue;
                }

                field.Name = property.Name;

                if (!field.MappTo.Equals("unassigned", StringComparison.InvariantCultureIgnoreCase))
                    Entity.Fields.Add(field);
            }
        }

        private BooleanConversion MappBooleanConversions(string propertyName)
        {
            BooleanConversion booleanConversion = null;
            PropertyInfo property = currentType.GetProperty(propertyName);
            List<BooleanConversion> conversions = property.GetCustomAttributes<BooleanConversion>().ToList<BooleanConversion>();
            if (conversions.Count > 0)
            {
                booleanConversion = conversions.First<BooleanConversion>();
            }
            return booleanConversion;
        }
    }
}

