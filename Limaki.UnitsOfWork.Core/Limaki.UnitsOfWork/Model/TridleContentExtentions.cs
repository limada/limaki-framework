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
using System.IO;
using Limaki.Common;
using Limaki.UnitsOfWork.Model;

namespace Limaki.UnitsOfWork {

    public static class TridleContentExtentions {

        static ICompressionWorker _compressionWorker = null;
        public static ICompressionWorker CompressionWorker {
            get => _compressionWorker ?? (_compressionWorker = Registry.Pooled<ICompressionWorker> ());
            set { _compressionWorker = value; }
        }

        public static Stream GetDeCompressed (this IByteArrayTridle it, Guid compression) {
            if (it == null)
                return null;
            var stream = CompressionWorker.DeCompress (new MemoryStream (it.Value), compression);
            stream.Position = 0;
            return stream;

        }

        public static Stream GetCompressed (this IByteArrayTridle it, Guid compression) {
            if (it == null)
                return null;
            var stream = CompressionWorker.Compress (new MemoryStream (it.Value), compression);
            stream.Position = 0;
            return stream;

        }

        public static Stream GetCompressed (this Stream it, Guid compression) {
            if (it == null)
                return null;
            var stream = CompressionWorker.Compress (it, compression);
            stream.Position = 0;
            return stream;

        }

        public static IByteArrayTridle WithStream (this IByteArrayTridle it, Stream stream, Guid compression) 
            => it.WithStream (stream.GetCompressed (compression));

        public static IByteArrayTridle WithStream (this IByteArrayTridle it, Stream stream) {
            if (it == null)
                return it;
            it.Value = stream.GetBuffer ();
            return it;
        }

        public static IByteArrayTridleMemento Memento (this IByteArrayTridle it) => new Model.Dto.ByteArrayTridleMemento {
            Id = it.Id,
            CreatedAt = it.CreatedAt,
            UpdatedAt = it.UpdatedAt,
            Key = it.Key,
            Member = it.Member,
        };
    }

}
