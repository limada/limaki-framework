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

using System;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Limaki.Common;
using Limaki.Common.Linqish;
using Limaki.Common.Reflections;
using Limaki.Data;
using Limaki.Repository;

namespace Limaki.UnitsOfWork.Repository
{

    /// <summary>
    /// implementation of usecase to fullfill service contract
    /// using a <see cref="IDomainQuore"/>
    /// decouples usecases from service layer
    /// </summary>
    public class RepositoryOrganizer<D> : IDisposable where D : IDomainQuore {

        ILog _log = null;
        public virtual ILog Log => _log ??= Registry.Pool.TryGetCreate<Logger>().Log(GetType());

        public Func<D> CreateQuoreFunc { get; set; }

        public virtual D CreateQuore() {
            var quore = CreateQuoreFunc();
            var gateway = quore.Quore.Gateway;
            var db = IoriInfo(gateway.Iori);
            Log.Debug($"{nameof(CreateQuore)} {db}");
            return quore;
        }

        public string IoriInfo(Iori iori) => iori.AsSettingsKey();

        public string QuoreInfo<Q>(Q quore) where Q : IDomainQuore {

            string msg<E, M>(E q, Expression<Func<E, M>> member, bool addValue = false) => ExpressionUtils.NullMember(q, member, addValue);
            var iori = quore?.Quore?.Gateway?.Iori;
            if (iori == null)
                return $"{msg(quore?.Quore, q => q.Gateway)}.{msg(quore?.Quore?.Gateway, q => q.Iori)}";

            return IoriInfo(iori);

        }

        protected virtual bool CheckQuore<Q>(Q quore) where Q : IDomainQuore {

            string msg(string asset) => $"{quore.GetType().Name}.{asset} == null";

            if (quore.Quore == null) {
                Log.Error(msg(nameof(IDomainQuore.Quore)));
                return false;
            }
            if (quore.Quore?.Gateway == null) {
                Log.Error(msg(nameof(IDomainQuore.Quore.Gateway)));
                return false;
            }
            if (quore.Quore?.Gateway?.Iori == null) {
                Log.Error(msg(nameof(IDomainQuore.Quore.Gateway.Iori)));
                return false;
            }
            if (quore.Quore?.Gateway?.Iori?.Provider == null) {
                Log.Error(msg(nameof(Iori.Provider)));
                return false;
            }
            if (quore.Quore.Gateway is IDbGateway dbGateway) {
                var amsg = $"{nameof(IDbProvider)}.{nameof(IDbProvider.GetConnection)}";
                try {
                    if (dbGateway.Provider.GetConnection(quore.Quore.Gateway.Iori) is System.Data.IDbConnection conn) {
                        if (conn?.Database == default) {
                            Log.Error(msg(amsg));
                            return false;
                        }
                        conn?.Close();
                        conn?.Dispose();
                    }
                } catch (Exception ex) {
                    Log.Error(ex.ExceptionMessage(amsg));
                    return false;
                }
            }
            return true;
        }

        protected virtual bool CheckTable<Q, T>(Q quore, Expression<Func<Q, IQueryable<T>>> member) where Q : IDomainQuore {
            var table = quore.Quore.GetQuery<T>();

                try {
                    return table!=null | table?.Any() ?? false ;
                } catch(Exception ex) {
                    Log.Error(ex.ExceptionMessage($"{table.GetType().FriendlyClassName()}", false));
                    return false;
                }


        }

        public IFactory DtoFactory { get; set; }

        public virtual void Catch(Exception ex) {
            Log.Error($"", ex);
            throw ex;
        }

        public void Dispose() { }

    }

}
