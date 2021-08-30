/*
 * Limaki 
 * 
 * This code is free software; you can redistribute it and/or modify it
 * under the terms of the GNU General Public License version 2 only, as
 * published by the Free Software Foundation.
 * 
 * Author: Lytico
 * Copyright (C) 2009-2012 Lytico
 *
 * http://www.limada.org
 * 
 */

using System;
using System.Linq;
using System.Collections.Generic;

namespace Limaki.UnitsOfWork {

    public class PagingList<T>:PagingCollection<T>, IList<T> {

        public int IndexOf(T item) {
            IncWait();
            var result = -1;
            if (object.Equals(default(T), item))
                return result;
            bool found = false;
            foreach (var page in base.Pages) {
                result = page.Value.IndexOf(item);
                if (result != -1) {
                    found = true;
                    break;
                }
            }
            Dec();

            if (!found)
                throw new IndexOutOfRangeException(string.Format("PagedList.IndexOf({0}) not found",item));

            return result;

        }

        public void Insert(int index, T item) {
            throw new NotImplementedException();
        }

        public void RemoveAt(int index) {
            throw new NotImplementedException();
        }

        public T this[int index] {
            get {
                return GetRow(index);
            }
            set {
                throw new NotImplementedException();
            }
        }


    }
}