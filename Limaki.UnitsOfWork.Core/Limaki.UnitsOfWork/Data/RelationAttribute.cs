using System;
using System.Collections.Generic;

namespace System.ComponentModel.DataAnnotations {

    /// <summary>
    /// replaces System.ComponentModel.DataAnnotations.AssociatinAttribute as this is marked as obsolete
    /// Used to mark an Entity member as an association
    /// </summary>
    [AttributeUsage (AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public sealed class RelationAttribute : Attribute {

        /// <summary>
        /// Full form of constructor
        /// </summary>
        /// <param name="name">The name of the association. For bi-directional associations,
        /// the name must be the same on both sides of the association</param>
        /// <param name="thisKey">Comma separated list of the property names of the key values
        /// on this side of the association</param>
        /// <param name="otherKey">Comma separated list of the property names of the key values
        /// on the other side of the association</param>
        public RelationAttribute (string name, string thisKey, string otherKey) {
            Name = name;
            ThisKey = thisKey;
            OtherKey = otherKey;
        }

        /// <summary>
        /// Gets the name of the association. For bi-directional associations, the name must
        /// be the same on both sides of the association
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets a comma separated list of the property names of the key values
        /// on this side of the association
        /// </summary>
        public string ThisKey { get; }

        /// <summary>
        /// Gets a comma separated list of the property names of the key values
        /// on the other side of the association
        /// </summary>
        public string OtherKey { get; }

        /// <summary>
        /// Gets or sets a value indicating whether this association member represents
        /// the foreign key side of an association
        /// </summary>
        public bool IsForeignKey { get; set; }

        /// <summary>
        /// Gets the collection of individual key members specified in the ThisKey string.
        /// </summary>
        public IEnumerable<string> ThisKeyMembers => GetKeyMembers (ThisKey);

        /// <summary>
        /// Gets the collection of individual key members specified in the OtherKey string.
        /// </summary>
        public IEnumerable<string> OtherKeyMembers => GetKeyMembers (OtherKey);

        /// <summary>
        /// Parses the comma delimited key specified
        /// </summary>
        /// <param name="key">The key to parse</param>
        /// <returns>Array of individual key members</returns>
        private static string[] GetKeyMembers (string key) => key.Replace (" ", string.Empty).Split (',');
    }
}
