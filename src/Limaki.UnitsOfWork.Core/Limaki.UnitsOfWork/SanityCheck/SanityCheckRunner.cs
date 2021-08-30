/*
 * Limaki 
 * 
 * This code is free software; you can redistribute it and/or modify it
 * under the terms of the GNU General Public License version 2 only, as
 * published by the Free Software Foundation.
 * 
 * Author: Lytico
 * Copyright (C) 2018 - 2019 Lytico
 *
 * http://www.limada.org
 * 
 */

using System;
using System.Collections.Generic;
using System.Linq;
using Limaki.Common;
using Limaki.Common.Collections;
using Limaki.Common.Linqish;

namespace Limaki.UnitsOfWork.SanityCheck
{

    public class SanityCheckRunner : IDisposable
    {
        ILog _log = null;
        public virtual ILog Log {
            get => _log ?? (_log = new Logger { LogLevel = LogLevel.All & ~LogLevel.Debug }.Log(this.GetType()));
            set { _log = value; }
        }

        public Func<bool> Break { get; set; }

        IList<(Type service, GuidFlags usecases, IEnumerable<ICriterias> criterias)> _services = new List<(Type service, GuidFlags useCases, IEnumerable<ICriterias> criterias)>();
        public void RegisterChecks<S>(GuidFlags useCases, params ICriterias[] criterias) where S : ISanityCheckService, IDisposable, new()
        {
            _services.Add((typeof(S), useCases, criterias));
        }

        public IEnumerable<ICriterias> With(IEnumerable<ICriterias> criterias, params Guid[] add) => criterias.OnEach(c => c.Resolve.Add(add));

        public IEnumerable<SanityCheckResult> Run<S>(params ICriterias[] criterias) where S : ISanityCheckService, IDisposable, new()
        {
            using (var service = new S()) return Run(service, criterias);
        }

        void Logging(SanityCheckResult check, string usecases)
        {
            var message = check.Message;
            var newLine = message != default && message.Length > 100;
            if (!newLine) {
                message = message?.Trim('\n');
            }
            Log.Raw($"{usecases}\t | {check.Flags} | {SanityCheckFlags.Instance.NameOf(check.Status)} | {(newLine ? "" : message)}");
            if (newLine) {
                Log.Raw(message);
            }
        }

        protected IEnumerable<SanityCheckResult> Run(ISanityCheckService service, IEnumerable<ICriterias> criterias, string usecases = "SanityCheck", Action<ICriterias, string> onStart = default, Action<SanityCheckResult, string> onCheck = default)
        {
            if (onCheck == default)
                onCheck = Logging;

            foreach (var crit in criterias) {

                onStart?.Invoke(crit, usecases);

                foreach (var check in service.SanityChecks(new[] { crit })) {

                    onCheck(check, usecases);

                    yield return check;

                    if (Break?.Invoke() ?? false)
                        break;
                }
            }

        }

        public IEnumerable<SanityCheckResult> RunRegisteredChecks(GuidFlags usecases, Action<ICriterias, string> onStart = default, Action<SanityCheckResult, string> onCheck = default, params Guid[] additionalFlags)
        {
            var result = new List<SanityCheckResult>();
            Log.Info($"{usecases}");
            foreach (var registered in _services) {

                var service = Activator.CreateInstance(registered.service) as ISanityCheckService;

                if (registered.usecases.Overlaps(usecases)) {
                    result.AddRange(Run(service, With(registered.criterias, additionalFlags).ToArray(), usecases.ToString(),onStart, onCheck));
                }

                if (service is IDisposable d) d.Dispose();
            }

            return result;
        }

        public static SanityCheckRunner FromRunner(SanityCheckRunner other)
        {
            var result = new SanityCheckRunner();
            result._services.AddRange(other._services);
            return result;
        }

        private bool disposedValue = false;
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue) {
                if (disposing) {
                    _services = null;
                }
                disposedValue = true;
            }
        }

        public void Dispose() => Dispose(true);


    }
}