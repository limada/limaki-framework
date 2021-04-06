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
using ICSharpCode.SharpZipLib.BZip2;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;

namespace Limaki.UnitsOfWork.Usecases {

    public class CompressionService : ICompressionService {

        const int bufferSize = 1024 * 8;

        public virtual Stream Compress (Stream stream, Guid compression) {
            stream.Position = 0;
            if (!IsCompressed (compression)) return stream;
            
            var size = 0;
            var result = new MemoryStream ();
            if (compression == CompressionTypes.BZip2) {
                size = BZip2Compress (stream, result);
            } else if (compression == CompressionTypes.Zip) {
                size = ZipCompress (stream, result);
            }
            result.SetLength (size);

            return result;

        }

        public virtual Stream DeCompress (Stream stream, Guid compression) {
            stream.Position = 0;
            if (!IsCompressed (compression)) return stream;
            
            var result = new MemoryStream ();
            try {
                if (compression == CompressionTypes.BZip2) {

                    BZip2Decompress (stream, result);

                } else if (compression == CompressionTypes.Zip) {

                    ZipDecompress (stream, result);

                }
            } catch (Exception e) {
                throw e;
            }

            return result;
        }

        #region Compression

        public virtual int BZip2Compress (Stream uncompressed, Stream compressed) {
            uncompressed.Seek (0, SeekOrigin.Begin);
            var bZip2Stream = new BZip2OutputStream (compressed, 9) {
                IsStreamOwner = false
            };

            var position = 0;
            var length = uncompressed.Length;
            var buffer = new byte[bufferSize];
            while (position < length - 1) {
                var readByte = uncompressed.Read (buffer, 0, bufferSize); bZip2Stream.Write (buffer, 0, readByte);
                position += readByte;
            }

            bZip2Stream.Close ();
            return bZip2Stream.BytesWritten;
        }

        public virtual void BZip2Decompress (Stream compressed, Stream decompressed) {
            compressed.Seek (0, SeekOrigin.Begin);
            var bZip2Stream = new BZip2InputStream (compressed) {
                IsStreamOwner = false
            };

            var buffer = new byte[bufferSize];
            var position = 0;
            while (true) {
                var readByte = bZip2Stream.Read (buffer, 0, bufferSize); if (readByte <= 0) {
                    break;
                }
                decompressed.Write (buffer, 0, readByte);
                position += readByte;
            }

            decompressed.Flush ();
            bZip2Stream.Close ();
        }

        public virtual int ZipCompress (Stream uncompressed, Stream compressed) {
            uncompressed.Seek (0, SeekOrigin.Begin);
            var zipStream = new DeflaterOutputStream (compressed) {
                IsStreamOwner = false // compress wants to have compressed Stream open
            };

            var buffer = new byte[bufferSize];
            var position = 0;
            var length = uncompressed.Length;
            while (position < length - 1) {
                var readByte = uncompressed.Read (buffer, 0, bufferSize);
                zipStream.Write (buffer, 0, readByte);
                position += readByte;
            }

            zipStream.Finish ();
            zipStream.Flush ();
            var result = (int)compressed.Length;
            zipStream.Close ();
            return result;
        }

        public virtual void ZipDecompress (Stream compressed, Stream decompressed) {
            compressed.Seek (0, SeekOrigin.Begin);

            var zipStream = new InflaterInputStream (compressed) {
                IsStreamOwner = false
            };
            var b = zipStream.ReadByte ();
            while (b != -1) {
                decompressed.WriteByte ((byte)b);
                b = zipStream.ReadByte ();
            }

            decompressed.Flush ();
            zipStream.Close ();
        }

        #endregion


        bool ICompressionService.Compressable (Guid compression) => IsCompressed (compression);

        public static bool IsCompressed (Guid compression) => !((compression == CompressionTypes.None) || (compression == CompressionTypes.NeverCompress));
    }
}
