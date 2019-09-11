/*
 * Limaki 
 * 
 * This code is free software; you can redistribute it and/or modify it
 * under the terms of the GNU General Public License version 2 only, as
 * published by the Free Software Foundation.
 * 
 * Author: Lytico
 * Copyright (C) 2017 - 2018 Lytico
 *
 * http://www.limada.org
 * 
 */

using Limaki.Common;
using Limaki.Data;
using System;
using System.IO;
using Limaki.LinqData;
using Limaki.UnitsOfWork.IdEntity.Data;

namespace Limaki.UnitsOfWork.Data {

    /// <summary>
    /// implementation of usecase to fullfill service contract
    /// using a Quore
    /// decouples usecases from service layer
    /// delegates requests to <see cref="EntityRepository"/>s
    /// </summary>
    public class RepositoryOrganizer<T> : IDisposable where T : IDomainQuore {

        ILog _log = null;
        public virtual ILog Log => _log ?? (_log = Registry.Pool.TryGetCreate<Logger> ().Log (this.GetType ()));
        
        public Func<T> CreateQuoreFunc { get; set; }

        public virtual T CreateQuore () { 
            var quore = CreateQuoreFunc ();
            var gateway = quore.Quore.Gateway;
            var db = string.IsNullOrEmpty (gateway.Iori.Server) ? Path.GetFullPath (gateway.Iori.ToFileName ()) : $"{gateway.Iori.Server}:{gateway.Iori.Port} {gateway.Iori.Name}";
            Log.Debug ($"{nameof (CreateQuore)} ({gateway.Iori.Provider}) {db}");
            return quore;
        }

        public IFactory DtoFactory { get; set; }

        public void Catch (Exception ex) {
            Log.Error ($"", ex);
            throw ex;
        }

        public void Dispose () { }

    }

}
