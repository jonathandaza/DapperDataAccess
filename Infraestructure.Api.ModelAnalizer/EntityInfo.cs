using System.Collections.Generic;
using System.Linq;
using System;

namespace Infraestructure.Api.ModelAnalizer
{
    public enum ActionToEnqueue
    {
        Delete,
        Update,
        Insert
    }

    public class EntityInfo : Attribute
    {
        /// <summary>
        /// Indica a cual campo se mapea en la BD, cuando no se debe mapear pero el campo es requerido se debe asignar el valor unassigned
        /// </summary>
        public string MappTo { get; set; }

        public string Name { get; set; }

        public List<EntityInfo> Fields { get; internal set; }

        public bool IsKey { get; private set; }

        public string TrueValue { get; set; }

        public string FalseValue { get; set; }

        public bool IsIdentity { get; set; }

        public string LastUpdateField { get; set; }

        public bool AddToDBQueue { get; set; }

        public List<ActionToEnqueue> DbEnqueueActions { get; set; }

        public EntityInfo()
        {
            MappTo = string.Empty;
            Name = string.Empty;
            IsKey = false;
            IsIdentity = false;
            TrueValue = null;
            FalseValue = null;
        }

        /// <summary>
        /// Informacion para mapeo del objeto a la BD
        /// </summary>
        /// <param name="mappTo">Nombre de la tabla en la BD</param>
        public EntityInfo(string mappTo, string lastUpdateField)
        {
            MappTo = mappTo;
            LastUpdateField = lastUpdateField;
        }

        ///// <summary>
        ///// Informacion para mapeo del objeto a la BD
        ///// </summary>
        ///// <param name="mappTo">Nombre en la BD</param>
        ///// <param name="lastUpdateField">Campo para control de la ultima actualizacion</param>
        ///// <param name="dbEnqueueActions">Campo para encolar entidad</param>
        //public EntityInfo(string mappTo, string lastUpdateField, string dbEnqueueActions)
        //{
        //    MappTo = mappTo;
        //    LastUpdateField = lastUpdateField;

        //    ActionToEnqueue action;
        //    List<string> actionsDB = dbEnqueueActions.Split(new char[] { '|' }).ToList();
        //    List<ActionToEnqueue> dbActions = new List<ActionToEnqueue>();

        //    foreach (string element in actionsDB)
        //    {
        //        if (ActionToEnqueue.TryParse(element, out action))
        //        {
        //            dbActions.Add(action);
        //        }
        //    }

        //    DbEnqueueActions = dbActions;
        //}

        /// <summary>
        /// Informacion para mapeo en la BD
        /// </summary>
        /// <param name="mappTo">Nombre en la BD</param>
        public EntityInfo(string mappTo)
        {
            MappTo = mappTo;
        }

        /// <summary>
        /// Informacion del campo en la entidad
        /// </summary>
        /// <param name="isKey">Es llave</param>
        /// <param name="isIdentity">Es auto incremental</param>
        public EntityInfo(bool isKey = false, bool isIdentity = false)
        {
            MappTo = "";
            IsKey = isKey;
            IsIdentity = isIdentity;
        }

        /// <summary>
        /// Informacion del campo en la entidad
        /// </summary>
        /// <param name="mappTo">Nombre del campo en la tabla</param>
        /// <param name="isKey">Es llave</param>
        /// <param name="isIdentity">Es auto incremental</param>
        public EntityInfo(string mappTo, bool isKey = false, bool isIdentity = false)
        {
            MappTo = mappTo;
            IsKey = isKey;
            IsIdentity = isIdentity;
        }
    }
}