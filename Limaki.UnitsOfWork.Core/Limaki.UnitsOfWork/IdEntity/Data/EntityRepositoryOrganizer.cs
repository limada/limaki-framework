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
using System.Collections.Generic;
using System.Linq;
using Limaki.Common.Collections;
using Limaki.Common.Tridles;
using Limaki.UnitsOfWork.Data;
using Limaki.UnitsOfWork.IdEntity.Model;
using Limaki.UnitsOfWork.Model;
using Limaki.UnitsOfWork.Tridles.Data;
using Limaki.UnitsOfWork.Tridles.Model;

namespace Limaki.UnitsOfWork.IdEntity.Data {

    public enum RepositoryChangeAction { Created, Updated, Upserted, Removed }

    public abstract class EntityRepositoryOrganizer<Q> : RepositoryOrganizer<Q> where Q : IEntityQuore {

        public EntityRepositoryOrganizer () => Init ();

        protected abstract void Init ();

        public abstract void CheckVersion ();

        public virtual void CheckVersion (Guid currentVersion, MetaId<Guid> schemaVersion) {
            try {
                Log.Info ($"{nameof (CheckVersion)} for {currentVersion}");

                using (var aquore = CreateQuore ()) {

                    if (aquore is ITridleQuore quore) {

                        Log.Info ($"{nameof(TridleRepository)} check: {TridleRepository.Ensure (quore)}");

                        IStringTridle version = quore.StringTridles.FirstOrDefault (
                            t => t.Key == schemaVersion.Type &&
                            t.Member == schemaVersion.TypeMember
                        );
                        if (version == null) {
                            Log.Info ($"no version found. adding {currentVersion}");
                            version = DtoFactory.Create<IStringTridle> ();
                            version.Key = schemaVersion.Type;
                            version.Member = schemaVersion.TypeMember;
                            version.Value = currentVersion.ToString ();
                            aquore.Upsert (new IStringTridle[] { version });
                        } else {
                            if (version.Value != currentVersion.ToString ()) {
                                var msg = $"wrong version found: {version.Value}";
                                Log.Error (msg);
                                throw new ArgumentException (msg);
                            }
                            Log.Info ($"{nameof (currentVersion)} : {version.Value}");
                        }
                    }
                }
            } catch (Exception ex) {
                Catch (ex);

            }
        }

        // TODO: this is a mockup
        // the idea is to separate the change history from entities
        // in a separate table

        public class ChangeHistory {

            public Guid EntityId { get; set; }
            public Guid UserId { get; set; }
            public RepositoryChangeAction Action { get; set; }
        }

        protected List<ChangeHistory> Changes { get; set; } = new List<ChangeHistory> ();
        protected abstract IDictionary<string, Guid> ModelGuids { get; }

        public Guid GetModelGuid (Type type) {
            // TODO: make use of TypeGuidAttribute
            var name = type.Name;
            if (type.IsInterface && name.StartsWith ("I"))
                name = name.Remove (0, 1);
            if (!ModelGuids.TryGetValue (name, out var modelGuid)) {
                Log.Error ($"{nameof (ModelGuids)} {name} not found");
            }
            return modelGuid;
        }

        public T CreateRepository<T> () where T : EntityRepository, new() {
            var result = new T { Log = Log, Factory = DtoFactory, ProveError = Log.Error, ModelGuid = GetModelGuid };

            return result;
        }

        public bool IsLocked (Guid key, Guid member) {
            using (var q = CreateQuore ()) {
                if (q is ILockQuore lg) {
                    return lg.Locks.Any (l => l.Key == key && l.Member == member);
                }
            }
            return false;
        }

        public bool Lock (ILock @lock) {
            using (var q = CreateQuore ()) {
                if (q is ILockQuore lg) {
                    q.Upsert (new[] { @lock });
                    return true;
                }
            }
            return false;
        }

        public bool UnLock (ILock @lock) {
            using (var q = CreateQuore ()) {
                if (q is ILockQuore lg) {
                    var lr = lg.Locks.Where (l => l.Id == @lock.Id).ToArray ();
                    q.Remove (lr);
                    return true;
                }
            }
            return false;
        }

        public bool UnLock (Guid key, Guid member) {
            using (var q = CreateQuore ()) {
                if (q is ILockQuore lg) {
                    q.Remove (lg.Locks.Where (l => l.Key == key && l.Member == member));
                    return true;
                }
            }
            return false;
        }

        public IEnumerable<ILock> Locks (Guid key, Guid member) {
            using (var q = CreateQuore ()) {
                if (q is ILockQuore lg) {
                    return lg.Locks.Where (l => l.Key == key && l.Member == member).ToArray ();
                }
            }
            return new ILock[0];
        }

        protected virtual bool ChooseContainer (QueryPredicates predicates, GuidFlags resolve) 
            => resolve.HasFlag(predicates.Resolve) && resolve.HasFlag (predicates.MainQuery);
    }

}
