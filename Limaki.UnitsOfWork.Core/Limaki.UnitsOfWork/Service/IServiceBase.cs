/*
 * Limaki 
 * 
 * This code is free software; you can redistribute it and/or modify it
 * under the terms of the GNU General Public License version 2 only, as
 * published by the Free Software Foundation.
 * 
 * Author: Lytico
 * Copyright (C) 2008-2012 Lytico
 *
 * http://www.limada.org
 * 
 */

using System.ServiceModel;

namespace Limaki.Common.Services {
    [ServiceContract]
    public interface IServiceBase {
        /// <summary>
        /// gives back true
        /// if service can be pinged
        /// </summary>
        /// <returns></returns>
        [OperationContract, FaultContractAttribute(typeof(string))]
        bool Ping();

        /// <summary>
        /// executes tests on the service
        /// eg. database connection
        /// </summary>
        /// <returns>null if everything ok, else error-string</returns>
        [OperationContract, FaultContractAttribute(typeof(string))]
        string Test();

        [OperationContract, FaultContractAttribute(typeof(string))]
        byte[] Resource(string name);

        [OperationContract, FaultContractAttribute(typeof(string))]
        string ServerVersion();
    }
}