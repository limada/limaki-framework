/*
 * Limaki 
 * 
 * This code is free software; you can redistribute it and/or modify it
 * under the terms of the GNU General Public License version 2 only, as
 * published by the Free Software Foundation.
 * 
 * Author: Lytico
 * Copyright (C) 2009-2012 Lytico
 *
 * http://www.limada.org
 * 
 */

using System.Runtime.Serialization;
using Limaki.Common;

namespace Limaki.UnitsOfWork {

    [DataContract]
    public class Paging {
        [DataMember]
        public int Skip { get; set; }
        [DataMember]
        public int Take { get; set; }
        [DataMember]
        public int Count { get; set; }

        [DataMember]
        public bool CountRequired { get; set; }
        [DataMember]
        public bool DataRequired { get; set; }

        /// <summary>
        /// if count <= limit, 
        /// then Take Count entities
        ///  </summary>
        [DataMember]
        public int Limit { get; set; }

        public override string ToString() {
            return string.Format("Count {0:#,0} Skip {1} Take {2} Limit {3}", Count, Skip, Take, Limit);
        }

        public virtual void CopyTo(Paging other) {
            var copier = new Copier<Paging>();
            copier.Copy(this, other);
        }
    }

}