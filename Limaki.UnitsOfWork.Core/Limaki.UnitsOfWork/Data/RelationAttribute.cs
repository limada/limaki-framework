using System;
using System.Collections.Generic;

namespace System.ComponentModel.DataAnnotations {

    /// <summary>
    /// replaces System.ComponentModel.DataAnnotations.AssociatinAttribute as this is marked as obsolete
    /// Used to mark an Entity member as an association
    /// </summary>
    [AttributeUsage (AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true, Inherited = true)]
    public sealed class RelationAttribute : Attribute {


        public RelationAttribute (Type other, string otherKey) {
            OtherKey = otherKey;
            OtherType = other;
        }

        /// <summary>
        /// Full form of constructor
        /// </summary>
        /// <param name="thisKey">property name of assoicated's member key, eg. an Id</param>
        /// <param name="otherKey">property name of assoicated's member key, eg. an Id, on the other side of association</param>
        /// <param name="other">type of the associated class</param>
        public RelationAttribute (string thisKey, Type other, string otherKey) {
            ThisKey = thisKey;
            OtherKey = otherKey;
            OtherType = other;
        }


        /// <summary>
        /// property name of assoicated's member key, eg. an Id
        /// on this side of the association
        /// </summary>
        public string ThisKey { get; }

        /// <summary>
        /// property name of assoicated's member key, eg. an Id
        /// on the other side of the association
        /// </summary>
        public string OtherKey { get; }

        /// <summary>
        /// type of the associated class
        /// </summary>
        /// <value>The type of the other.</value>
        public Type OtherType { get; }



    }
}
