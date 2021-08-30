/*
 * Limaki 
 * 
 * This code is free software; you can redistribute it and/or modify it
 * under the terms of the GNU General Public License version 2 only, as
 * published by the Free Software Foundation.
 * 
 * Author: Lytico
 * Copyright (C) 2012 - 2016 Lytico
 *
 * http://www.limada.org
 * 
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Limaki.Common.Linqish;
using Limaki.Repository;
using LinqToDB;
using LinqToDB.Data;

namespace Limaki.Repository {

    public class LinqToDBQuoreDbTransaction : IQuoreTransaction {

        public DataConnection Connection { get; protected set; }

        public DataConnectionTransaction Transaction { get; protected set; }

        public LinqToDBQuoreDbTransaction (DataConnection connection) => this.Connection = connection;

        public void BeginTransaction () => Transaction = Connection.BeginTransaction ();

        public void Commit () => Transaction.Commit ();

        public void Dispose () => Transaction?.Dispose ();

        public void Rollback () => Transaction.Rollback ();
    }
}
