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
using Limaki.Common;
using Limaki.Common.Reflections;
using Limaki.Common.Collections;
using Limaki.UnitsOfWork.Service;
using System.Text;

namespace Limaki.UnitsOfWork.SanityCheck
{

    public abstract class SanityCheckService : ISanityCheckService
    {
        ILog _log = null;
        public virtual ILog Log => _log ?? (_log = Registry.Pool.TryGetCreate<Logger>().Log(this.GetType()));

        public virtual GuidFlags Flags(params Guid[] flags) => SanityCheckFlags.__().With(flags);

        public SanityCheckResult CheckService<S>(params Guid[] flags) where S : IServiceBase
        {
            var status = SanityCheckFlags.Ok;
            var result = new SanityCheckResult
            {
                Flags = Flags(flags),
                Status = status
            };
            var message = new StringBuilder($"{typeof(S).Namespace}.{typeof(S).FriendlyClassName()}");
            try {
                var service = Registry.Create<S>();
                result.Status = SanityCheckFlags.Ok;
                message.Append($"\tcreated={service != null}");
                if (service == null) {
                    result.Status = SanityCheckFlags.Error;
                } else {
                    message.Append($"\tVersion={service.ServerVersion()}");
                    message.Append($"\tInfo={service.ServiceInfo()}");
                    var ping = service.Ping();
                    if (!ping) {
                        result.Status = SanityCheckFlags.Error;
                        message.Append($"\t{nameof(service.Ping)} failed");
                    }
                    // if ping fails, try to connect anyway to get the error
                    var tryConnect = service.TryConnect();
                    message.Append($"\t{nameof(service.TryConnect)}={tryConnect}");
                }


                result.Message = message.ToString();

            } catch (Exception ex) {
                result.Message = ex.ExceptionMessage($"{nameof(CheckService)} failed for:\t<> ... {message.ToString()}");
                result.Status = SanityCheckFlags.Error;
            }
            return result;
        }

        public abstract IEnumerable<SanityCheckResult> SanityChecks(IEnumerable<ICriterias> criterias);

    }
}