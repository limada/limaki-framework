/*
 * Limaki 
 * 
 * This code is free software; you can redistribute it and/or modify it
 * under the terms of the GNU General Public License version 2 only, as
 * published by the Free Software Foundation.
 * 
 * Author: Lytico
 * Copyright (C) 2018 Lytico
 *
 * http://www.limada.org
 * 
 */

using Limaki.Common.IOC;
using Limaki.UnitsOfWork.Usecases;

namespace Limaki.UnitsOfWork {

    public class UnitsOfWorkContextResourceLoader : ContextResourceLoader {

        public UnitsOfWorkContextResourceLoader (IContextResourceLoader resourceLoader) {
            this.ResourceLoader = resourceLoader;
        }

        public UnitsOfWorkContextResourceLoader () {
        }

        protected IContextResourceLoader ResourceLoader { get; set; }

        protected static bool Applied { get; set; }

        public override void ApplyResources (IApplicationContext context) {

            if (Applied)
                return;

            ResourceLoader?.ApplyResources (context);

            context.Factory.Add<ICompressionService, CompressionService> ();


            Applied = true;
        }
    }

   
}