/*
 * Limada 
 *
 * This code is free software; you can redistribute it and/or modify it
 * under the terms of the GNU General Public License version 2 only, as
 * published by the Free Software Foundation.
 * 
 * Author: Lytico
 * Copyright (C) 2010-2017 Lytico
 *
 * http://www.limada.org
 * 
 */

using System.Collections.Generic;
using System.Data;

namespace Limaki.Data {

    public interface IDbProvider {

        /// <summary>
        /// Name of provider
        /// </summary>
        string Name { get; }

        string ConnectionString (Iori iori);
        IDbConnection GetConnection (Iori iori);

        bool CreateDatabase (Iori iori);
        bool DropDatabase (Iori iori);
        bool DataBaseExists (Iori iori);

        bool CloseEverything ();

    }

    public class DbProviderPool {

        protected Dictionary<string, IDbProvider> _providers = new Dictionary<string, IDbProvider> ();

        public void Add (IDbProvider fbProvider) {
            _providers [fbProvider.Name] = fbProvider;
        }

        public IDbProvider Get (string name) {
            IDbProvider result = null;
            _providers.TryGetValue (name ?? "", out result);
            return result;
        }
    }
}
