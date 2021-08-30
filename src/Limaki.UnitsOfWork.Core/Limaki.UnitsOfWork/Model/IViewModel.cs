/*
 * Limaki 
 * 
 * This code is free software; you can redistribute it and/or modify it
 * under the terms of the GNU General Public License version 2 only, as
 * published by the Free Software Foundation.
 * 
 * Author: Lytico
 * Copyright (C) 2009 - 2019 Lytico
 *
 * http://www.limada.org
 * 
 */


namespace Limaki.UnitsOfWork {

    public interface IViewModel : ICheckable {

        Store Store { get; set; }
        bool ReadOnly { get; set; }
        string ToString (string format);

    }

    public interface IViewModel<T> : IViewModel {
        T Entity { get; }
    }
}