using Infraestructure.Api.ModelAnalizer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;


namespace Infraestructure.Api.DapperDataAccess
{
    public class SentenceStructurator<T>
    {
        TypeInterpolator<T> interpolator = new TypeInterpolator<T>();
        DPSentenceExtension<T> _extender;

        public SentenceStructurator()
        {
            _extender = new EmptyDPSentenceExtension<T>();
        }

        public SentenceStructurator(DPSentenceExtension<T> extender)
        {
            _extender = extender;
        }

        //public void validateSelect(T model)
        //{
        //    string validationResult = _extender.ValidateSelect(model);
        //    if (!string.IsNullOrEmpty(validationResult))
        //        throw new Exception(validationResult);
        //}

        public void validateInsert(T model)
        {
            string validationResult = _extender.ValidateInsert(model);
            if (!string.IsNullOrEmpty(validationResult))
                throw new DapperDataAccessException(validationResult);
        }

        public void validateUpdate(T model)
        {
            string validationResult = _extender.ValidateUpdate(model);
            if (!string.IsNullOrEmpty(validationResult))
                throw new DapperDataAccessException(validationResult);
        }

        public void validateDelete(Filter[] filters)
        {
            string validationResult = _extender.ValidateDelete(filters);
            if (!string.IsNullOrEmpty(validationResult))
                throw new DapperDataAccessException(validationResult);
        }


        public string CreateSelectFields()
        {
            string result = string.Empty;
            foreach (EntityInfo field in interpolator.Entity.Fields)
            {
                result = string.Format("{0} {1}", result, CreateField(field));
            }
            return result.TrimEnd(new char[] { ',', ' ' });
        }

        internal string CreateField(EntityInfo field)
        {
            List<ColumnExtender> columnExtensions = _extender.GetPropertiesExtensions();

            if (!string.IsNullOrEmpty(field.TrueValue)
                && !string.IsNullOrEmpty(field.FalseValue)
                && !columnExtensions.Exists(x => x.PropertyName.Equals(field.Name)))
            {
                return BooleanValues(field);
            }
            else
            {
                if (columnExtensions.Exists(x => x.PropertyName.Equals(field.Name)))
                {
                    ColumnExtender columnExtender = _extender.GetPropertiesExtensions().First<ColumnExtender>(x => x.PropertyName.Equals(field.Name));
                    string extension = columnExtender.Extension;
                    string alias = columnExtender.PropertyName;
                    string columnName = columnExtender.ColumnName;
                    string columnBody = columnName;

                    if (!string.IsNullOrEmpty(extension))
                        columnBody = extension;

                    return string.Format("{0} AS {1}, ", columnBody, alias);
                }
                else
                {
                    if (string.IsNullOrEmpty(field.MappTo))
                        return string.Format("{0}, ", field.Name);
                    else
                        return string.Format("{0} AS {1}, ", field.MappTo, field.Name);
                }
            }
        }

        internal string BooleanValues(EntityInfo field)
        {
            if (string.IsNullOrEmpty(field.MappTo))
                return string.Format("CASE {0} WHEN {1} THEN 1 ELSE 0 END {0}, ", field.Name, field.TrueValue);
            else
                return string.Format("CASE {0} WHEN {1} THEN 1 ELSE 0 END AS {2}, ", field.MappTo, field.TrueValue, field.Name);
        }

        internal string CreateSetFields(T model)
        {
            string result = string.Empty;
            List<ColumnExtender> propertyExtensions = _extender.GetPropertiesExtensions();
            List<ColumnExtender> valuesExtended = _extender.ValuesExtender(model);
            foreach (EntityInfo field in interpolator.Entity.Fields)
            {
                if (_extender.GetPropertiesExtensions().Exists(x => x.PropertyName.Equals(field.Name)))
                {
                    ColumnExtender columnExtender = propertyExtensions.First<ColumnExtender>(x => x.PropertyName.Equals(field.Name));
                    field.IsIdentity = columnExtender.IsIdentity;
                    field.MappTo = columnExtender.ColumnName;
                }
                if (valuesExtended.Exists(x => x.PropertyName.Equals(field.Name)))
                    field.IsIdentity = true;
                if (!field.IsIdentity && string.IsNullOrEmpty(field.TrueValue) && string.IsNullOrEmpty(field.FalseValue))
                    result = string.Format("{0} {1} = @{2},", result, FieldMapp(field.Name), field.Name);
            }
            result = string.Format("{0} {1}", result, CreateSetFieldsMappedToBoolean(model));
            result = string.Format("{0} {1}", result, CreateSetExtendedFields(model));
            if (!string.IsNullOrEmpty(interpolator.Entity.LastUpdateField))
                result = string.Format("{0}{1}=GETDATE()", result, interpolator.Entity.LastUpdateField);

            return result.TrimEnd(new char[] { ',', ' ' });
        }

        internal string CreateInsertExtendedFields(T model)
        {
            string result = string.Empty;
            List<ColumnExtender> valuesExtended = _extender.ValuesExtender(model);
            foreach (EntityInfo field in interpolator.Entity.Fields)
            {
                if (valuesExtended.Exists(x => x.PropertyName.Equals(field.Name)))
                {
                    string extension = valuesExtended.First<ColumnExtender>(x => x.PropertyName.Equals(field.Name)).Extension;
                    string columnName = valuesExtended.First<ColumnExtender>(x => x.PropertyName.Equals(field.Name)).ColumnName;
                    result = string.Format("{0} {1},", result, columnName);
                }
            }
            return result.TrimEnd(new char[] { ',', ' ' });
        }

        internal string CreateInsertExtendedValues(T model)
        {
            string result = string.Empty;
            List<ColumnExtender> valuesExtended = _extender.ValuesExtender(model);
            foreach (EntityInfo field in interpolator.Entity.Fields)
            {
                if (valuesExtended.Exists(x => x.PropertyName.Equals(field.Name)))
                {
                    ColumnExtender columnExtender = valuesExtended.First<ColumnExtender>(x => x.PropertyName.Equals(field.Name));
                    string extension = columnExtender.Extension;
                    string columnName = columnExtender.ColumnName;
                    result = string.Format("{0} {1},", result, extension);
                }
            }
            return result.TrimEnd(new char[] { ',', ' ' });
        }

        internal string CreateSetExtendedFields(T model)
        {
            string result = string.Empty;
            List<ColumnExtender> valuesExtended = _extender.ValuesExtender(model);
            foreach (EntityInfo field in interpolator.Entity.Fields)
            {
                if (valuesExtended.Exists(x => x.PropertyName.Equals(field.Name)))
                {
                    string extension = valuesExtended.First<ColumnExtender>(x => x.PropertyName.Equals(field.Name)).Extension;
                    string columnName = valuesExtended.First<ColumnExtender>(x => x.PropertyName.Equals(field.Name)).ColumnName;
                    result = string.Format("{0} {1} = {2},", result, columnName, extension);
                }
            }
            return result.TrimEnd(new char[] { ',', ' ' });
        }

        internal string CreateSetFieldsExtended(T model)
        {
            string result = string.Empty;
            foreach (EntityInfo field in interpolator.Entity.Fields)
            {
                if (!field.IsIdentity && !string.IsNullOrEmpty(field.TrueValue) && !string.IsNullOrEmpty(field.TrueValue))
                    result = string.Format("{0} {1} = {2},", result, FieldMapp(field.Name), GetMappedValue(field, model));
            }
            return result.TrimEnd(new char[] { ',', ' ' });
        }

        internal string CreateSetFieldsMappedToBoolean(T model)
        {
            string result = string.Empty;
            foreach (EntityInfo field in interpolator.Entity.Fields)
            {
                if (!field.IsIdentity && !string.IsNullOrEmpty(field.TrueValue) && !string.IsNullOrEmpty(field.TrueValue))
                    result = string.Format("{0} {1} = {2},", result, FieldMapp(field.Name), GetMappedValue(field, model));
            }
            return result.TrimEnd(new char[] { ',', ' ' });
        }

        internal string GetEntityName(SentenceType sentenceType)
        {
            if (string.IsNullOrEmpty(interpolator.Entity.MappTo))
                return interpolator.Entity.Name;
            else
            {
                string entityName = string.Empty;

                if (!string.IsNullOrEmpty(_extender.MainTableName))
                    entityName = _extender.MainTableName;
                else
                    entityName = interpolator.Entity.MappTo;

                string join = string.Empty;
                if (_extender.GetJoin().ContainsKey(sentenceType))
                    join = _extender.GetJoin()[sentenceType];

                return string.Format("{0} {1}", entityName, join).TrimEnd(new char[] {' '});
            }
        }

        internal string CreateInsertFields(T model)
        {
            string result = string.Empty;
            List<ColumnExtender> propertyExtensions = _extender.GetPropertiesExtensions();
            List<ColumnExtender> valuesExtended = _extender.ValuesExtender(model);
            foreach (EntityInfo field in interpolator.Entity.Fields)
            {
                if (propertyExtensions.Exists(x => x.PropertyName.Equals(field.Name)))
                {
                    ColumnExtender columnExtender = propertyExtensions.First<ColumnExtender>(x => x.PropertyName.Equals(field.Name));
                    field.IsIdentity = columnExtender.IsIdentity;
                    field.MappTo = columnExtender.ColumnName;
                }
                if (valuesExtended.Exists(x => x.PropertyName.Equals(field.Name)))
                    field.IsIdentity = true;
                if (!field.IsIdentity && string.IsNullOrEmpty(field.TrueValue) && string.IsNullOrEmpty(field.FalseValue))
                    result = string.Format("{0} {1},", result, FieldMapp(field.Name));
            }
            result = string.Format("{0}{1}", result, FieldsMappedToBoolean());
            result = string.Format("{0}{1}", result, CreateInsertExtendedFields(model));
            if (!string.IsNullOrEmpty(interpolator.Entity.LastUpdateField))
                result = string.Format("{0} {1}", result, interpolator.Entity.LastUpdateField);
            return result.TrimEnd(new char[] { ',', ' ' });
        }

        internal bool ValidateEnqueue(string actionName)
        {
            bool result = interpolator.Entity.DbEnqueueActions.Exists(x => x.ToString() == actionName);
            return result;
        }

        internal string FieldsMappedToBoolean()
        {
            string result = string.Empty;
            foreach (EntityInfo field in interpolator.Entity.Fields)
            {
                if (!field.IsIdentity && !string.IsNullOrEmpty(field.TrueValue) && !string.IsNullOrEmpty(field.FalseValue))
                    result = string.Format("{0} {1},", result, FieldMapp(field.Name));
            }
            return result.TrimEnd(new char[] { ',', ' ' });
        }

        internal object CreateInsertValues(T model)
        {
            string result = string.Empty;
            foreach (EntityInfo field in interpolator.Entity.Fields)
            {
                if (!_extender.GetPropertiesExtensions().Exists(x => x.PropertyName.Equals(field.Name) && x.IsIdentity))
                {
                    if (!field.IsIdentity && string.IsNullOrEmpty(field.TrueValue) && string.IsNullOrEmpty(field.FalseValue))
                        result = string.Format("{0} @{1},", result, field.Name, model);
                }
            }

            result = string.Format("{0}{1}", result, InsertValuesMappedToBoolean(model));
            result = string.Format("{0}{1}", result, CreateInsertExtendedValues(model));
            if(!string.IsNullOrEmpty(interpolator.Entity.LastUpdateField))
                result = string.Format("{0} GETDATE() ", result);

            return result.TrimEnd(new char[] { ',', ' ' });
        }

        internal object InsertValuesMappedToBoolean(T model)
        {
            string result = string.Empty;
            foreach (EntityInfo field in interpolator.Entity.Fields)
            {
                if (!field.IsIdentity && !string.IsNullOrEmpty(field.TrueValue) && !string.IsNullOrEmpty(field.FalseValue))
                    result = string.Format("{0} {1},", result, GetMappedValue(field, model));
            }
            return result.TrimEnd(new char[] { ',', ' ' });
        }

        internal string GetMappedValue(EntityInfo field, T model)
        {
            string mappedValue = field.FalseValue;
            PropertyInfo info = typeof(T).GetProperty(field.Name);
            if ((bool)info.GetValue(model))
                mappedValue = field.TrueValue;

            return mappedValue;
        }

        internal string CreateFilters(Filter[] filters)
        {
            try
            {
                string result = string.Empty;
                for (int i = 0; i < filters.Length; i++)
                {                    
                    result += (i == 0) ? " WHERE " : " AND ";

                    filters[i].Value = filters[i].Value.Replace("'", string.Empty);

                    if (filters[i].HasQuotes || !IsNumeric(filters[i].Value))
                        result += string.Format("{0} {1} '{2}'", FieldMapp(filters[i].Field), filters[i].Operator, filters[i].Value);
                    else
                        result += string.Format("{0} {1} {2}", FieldMapp(filters[i].Field), filters[i].Operator, filters[i].Value);
                }
                return result;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        internal bool IsNumeric(string _value)
        { 
            decimal _decimal = 0;
            return Decimal.TryParse(_value, out _decimal);
        }

        internal string FieldMapp(string fieldName)
        {
            List<ColumnExtender> propertiesExtended = _extender.GetPropertiesExtensions();
            if (propertiesExtended.Exists(x => x.PropertyName.Equals(fieldName)))
                return propertiesExtended.Find(x => x.PropertyName.Equals(fieldName)).ColumnName;

            string result = string.Empty;
            if (interpolator.Entity.Fields.Exists(x => x.Name.Equals(fieldName, StringComparison.InvariantCultureIgnoreCase)))
                return interpolator.Entity.Fields.Find(x => x.Name.Equals(fieldName, StringComparison.InvariantCultureIgnoreCase)).MappTo;
            else
                if (interpolator.Entity.Fields.Exists(x => x.MappTo.Equals(fieldName, StringComparison.InvariantCultureIgnoreCase)))
                    return interpolator.Entity.Fields.Find(x => x.MappTo.Equals(fieldName, StringComparison.InvariantCultureIgnoreCase)).MappTo;
                else
                    throw new ArgumentNullException(string.Format("No existe el campo {0} en el modelo", fieldName));
        }
    }
}
