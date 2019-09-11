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

namespace Limaki.UnitsOfWork.Model {

    public class UsecaseInfo {

        public string Title { get; set; }
        public string Version { get; set; }
        /// <summary>
        /// alpha, beta, debug, release etc.
        /// </summary>
        /// <value>The configuration.</value>
        public string Configuration { get; set; }
    }
}
