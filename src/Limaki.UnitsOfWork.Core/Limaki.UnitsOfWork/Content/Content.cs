/*
 * Limada
 * 
 * This code is free software; you can redistribute it and/or modify it
 * under the terms of the GNU General Public License version 2 only, as
 * published by the Free Software Foundation.
 * 
 * Author: Lytico
 * Copyright (C) 2006-2019 Lytico
 *
 * http://www.limada.org
 * 
 */

using System;
using Limaki.UnitsOfWork.Usecases;

namespace Limaki.UnitsOfWork.Content {

    public class Content {

        public Guid Compression { get; set; } = CompressionTypes.None;
        public Guid ContentType { get; set; } = ContentTypes.Unknown;
        public object Description { get; set; }
        public object Source { get; set; }
    }

    public class Content<T> : Content {

        public T Data { get; set; }

        public Content () { }
        public Content (T data) {
            Data = data;
        }

        public Content (T data, Guid compression) {
            Data = data;
            Compression = compression;
        }

        public Content (T data, Guid compression, Guid streamType) : this (data, compression) {
            ContentType = streamType;
        }

        public Content (Content source) {
            if (source != null) {
                Description = source.Description;
                Source = source.Source;
                Compression = source.Compression;
                ContentType = source.ContentType;
                if (source is Content<T> sourceT)
                    Data = sourceT.Data;
            }
        }
    }

    public class Content<TKey, TData> : Content<TData> {

        public TKey Id { get; set; }

        public Content (Content source) : base (source) {
            if (source is Content<TKey, TData> sourceT)
                Id = sourceT.Id;
        }

    }

}
