/*
 * Limaki 
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

#if WCF
using System.ServiceModel;

namespace Limaki.UnitsOfWork {

    [ServiceContract]
    public interface IServiceBase {
        /// <summary>
        /// gives back true
        /// if service can be pinged
        /// </summary>
        /// <returns></returns>
        [OperationContract, FaultContract (typeof (string))]
        bool Ping ();

        /// <summary>
        /// executes tests on the service
        /// eg. database connection
        /// </summary>
        /// <returns>null if everything ok, else error-string</returns>
        [OperationContract, FaultContract (typeof (string))]
        string Test ();

        [OperationContract, FaultContract (typeof (string))]
        byte [] Resource (string name);

        [OperationContract, FaultContract (typeof (string))]
        string ServerVersion ();
    }
}
#else
namespace Limaki.UnitsOfWork.Service {

    public interface IServiceBase {
        /// <summary>
        /// gives back true
        /// if service can be pinged
        /// </summary>
        /// <returns></returns>
        bool Ping ();

        /// <summary>
        /// executes tests on the service
        /// eg. database connection
        /// </summary>
        /// <returns>null if everything ok, else error-string</returns>
        string Test ();

        byte [] Resource (string name);

        string ServerVersion ();
    }
}
#endif
