/*
 * Limada 
 *
 * This code is free software; you can redistribute it and/or modify it
 * under the terms of the GNU General Public License version 2 only, as
 * published by the Free Software Foundation.
 * 
 * Author: Lytico
 * Copyright (C) 2017 Lytico
 *
 * http://www.limada.org
 * 
 */

using System.Data;

namespace Limaki.Repository {

    public class QuoreDbTransaction : IQuoreTransaction {

        public IDbTransaction Transaction { get; protected set; }

        public QuoreDbTransaction (IDbTransaction transaction) {
            this.Transaction = transaction;
        }

        public void Commit () {
            Transaction.Commit ();
        }

        public void Dispose () {
            Transaction.Dispose ();
        }

        public void Rollback () {
            Transaction.Rollback ();
        }
    }
}