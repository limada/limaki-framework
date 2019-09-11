using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Limaki.Common.Reflections;

namespace System.ComponentModel.DataAnnotations {

    /// <summary>
    /// replaces System.ComponentModel.DataAnnotations.AssociatinAttribute as this is marked as obsolete
    /// Used to mark an Entity member as an association
    /// </summary>
    [AttributeUsage (AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true, Inherited = true)]
    public sealed class RelationAttribute : Attribute {

        public RelationAttribute (string thisKey, string thisCardinality, Type otherType, string otherKey, string otherCardinality) {
            ThisKey = thisKey;
            ThisCardinality = thisCardinality;
            OtherKey = otherKey;
            OtherCardinality = otherCardinality;
            OtherType = otherType;
        }

        public RelationAttribute (Type otherType, string otherKey) {
            OtherKey = otherKey;
            OtherType = otherType;
        }

        public RelationAttribute (string thisKey, string otherKey) {
            ThisKey = thisKey;
            OtherKey = otherKey;
        }

        public RelationAttribute (Type otherType, string otherKey, string otherCardinality) : this (otherType, otherKey) {
            OtherCardinality = otherCardinality;
        }

        public RelationAttribute (string thisKey, Type other, string otherKey) : this (thisKey, otherKey) {
            OtherType = other;
        }


        /// <summary>
        /// property name of assoicated's member key, eg. an Id
        /// on this side of the association
        /// </summary>
        public string ThisKey { get; protected set; }

        public string ThisCardinality { get; protected set; }

        /// <summary>
        /// property name of assoicated's member key, eg. an Id
        /// on the other side of the association
        /// </summary>
        public string OtherKey { get; }

        public string OtherCardinality { get; protected set; }

        /// <summary>
        /// type of the associated class
        /// </summary>
        /// <value>The type of the other.</value>
        public Type OtherType { get; protected set; }

        public RelationAttribute Adjust (PropertyInfo property) {

            if (ThisKey == default)
                ThisKey = property.Name;

            if (property.PropertyType != typeof (string) && typeof (Collections.IEnumerable).IsAssignableFrom (property.PropertyType)) {
                // TODO OtherType = T of IEnumerable
                if (OtherType == default && property.PropertyType.IsGenericType) {
                    OtherType = property.PropertyType.GetGenericArguments ()?.FirstOrDefault ();
                }
                if (OtherCardinality == default)
                    OtherCardinality = "*";
            }

            if (OtherType == default) {
                OtherType = property.PropertyType;

            }
            return this;
        }

    }

    public static class RelationAttributeExtensions {

        public static IEnumerable<RelationAttribute> RelationAttributes (this Type type) =>
            type.GetPublicProperties ().SelectMany (p => p.GetCustomAttributes<RelationAttribute> ().Select (r => r.Adjust (p)));

        public static IEnumerable<RelationAttribute> RelationAttributes (this Type type, Type otherType) =>
            type.RelationAttributes ().Where (c => c.OtherType == otherType);

        public static bool HasRelation (this Type type, Type otherType) =>
            type.RelationAttributes ().Any (c => c.OtherType == otherType);
    }
}
