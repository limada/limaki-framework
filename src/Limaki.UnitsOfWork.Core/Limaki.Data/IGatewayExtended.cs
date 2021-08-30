/*
 * Limada 
 *
 * This code is free software; you can redistribute it and/or modify it
 * under the terms of the GNU General Public License version 2 only, as
 * published by the Free Software Foundation.
 * 
 * Author: Lytico
 * Copyright (C) 2006-2017 Lytico
 *
 * http://www.limada.org
 * 
 */

using System;
using System.Collections.Generic;

namespace Limaki.Data {

    public interface IGatewayExtended : IGateway {

        IEnumerable<string> Tables { get; }

        bool CreateTable<T> (string tableName = null);

        string CreateTableStatement<T> (string tableName = null, bool withIndices = false);

        string TableName<T> ();
        string TableName (Type entityType);

        void CreateIndex<T> (string columnName);

        bool SwitchIndices<T>(bool on);

        long BulkCopy<T> (IEnumerable<T> source, Func<long, DateTime, bool> rowsCopied = null) where T : class;
    }
}