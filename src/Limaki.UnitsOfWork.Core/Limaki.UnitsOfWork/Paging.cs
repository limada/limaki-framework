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
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using Limaki.Common;

namespace Limaki.UnitsOfWork {

    [DataContract]
    public class Paging {
        [DataMember]
        public int Skip { get; set; }
        [DataMember]
        public int Take { get; set; }
        [DataMember]
        public int Count { get; set; }

        [DataMember]
        public bool CountRequired { get; set; }
        [DataMember]
        public bool DataRequired { get; set; }

        /// <summary>
        /// if count <= limit, 
        /// then Take Count entities
        ///  </summary>
        [DataMember]
        public int Limit { get; set; }

        public override string ToString ()
            => $"{nameof (Skip)}={Skip} {nameof (Take)}={Take} {nameof (Count)}={Count:#,0} {nameof (Limit)}={Limit} {nameof (CountRequired)}={CountRequired} {nameof (DataRequired)}={DataRequired}";

        public virtual Paging CopyTo (Paging other) {
            var copier = new Copier<Paging> ();
            return copier.Copy (this, other ?? new Paging ());
        }

        public override bool Equals (object obj) {
            if (obj is Paging other) {
                return Take == other.Take &&
                    Skip == other.Skip &&
                    Count == other.Count &&
                    Limit == other.Limit &&
                    DataRequired == other.DataRequired &&
                    CountRequired == other.CountRequired;
            }
            return false;
        }

        public override int GetHashCode () {
            var hashCode = -656153635;
            hashCode = hashCode * -1521134295 + Skip.GetHashCode ();
            hashCode = hashCode * -1521134295 + Take.GetHashCode ();
            hashCode = hashCode * -1521134295 + Count.GetHashCode ();
            hashCode = hashCode * -1521134295 + CountRequired.GetHashCode ();
            hashCode = hashCode * -1521134295 + DataRequired.GetHashCode ();
            hashCode = hashCode * -1521134295 + Limit.GetHashCode ();
            return hashCode;
        }
    }

    public interface IPaged {
        Paging Paging { get; }
    }

    public static class PagingExtensions {

        public static Paging NextPage (this Paging it) => SwitchPage (it, true);
        public static Paging PreviousPage (this Paging it) => SwitchPage (it, false);

        public static Paging SwitchPage (this Paging it, bool takeNext) {
            if (it == null)
                return it;
            var take = it.Copy ();
            take.CountRequired = false;
            if (takeNext)
                take.Skip = Math.Min (take.Skip + it.Take, it.Count - it.Count%it.Take);
            else
                take.Skip = Math.Max (0, take.Skip - it.Take);
            return take;
        }

        public static int PageNr (this Paging it) => it == null ? 0 : (int)Math.Floor ((double)it.Skip / it.Take) + 1;

        public static int PageCount (this Paging it) => it == null ? 0 : (int)Math.Floor ((double)it.Count / it.Take) + 1;

        /// <summary>
        /// amount of elements in current paging
        /// </summary>
        public static int ActualPageSize (this Paging it) => it == null ? 0 : Math.Min (it.Count - it.Skip, it.Take);

        public static bool HasNext (this Paging it) => it != null && (it.Skip + it.Take) < it.Count;

        public static Paging Copy (this Paging it) => it?.CopyTo (new Paging ());

        public static Paging NewPaging (int take = 100) => new Paging { Take = take, DataRequired = true, CountRequired = true };

        public static Paging NewPaging (this IPaged it, int take = 100) => NewPaging (it?.Paging, take);
        public static Paging NewPaging (this Paging it, int take = 100) => NewPaging (take).CopyTo (it);

        public static string ToJson (this Paging it)
                        => $"{{\"{nameof (Paging.Skip)}\":{it.Skip},\"{nameof (Paging.Take)}\":{it.Take},\"{nameof (Paging.Limit)}\":{it.Limit},"
                           + $"\"{nameof (Paging.Count)}\":{it.Count},\"{nameof (Paging.CountRequired)}\":{it.CountRequired.ToString ().ToLower ()},"
                           + $"\"{nameof (Paging.DataRequired)}\":{it.DataRequired.ToString ().ToLower ()}}}";

        public static Paging FromJson (this Paging it, string jsonValue) {

            if (string.IsNullOrWhiteSpace (jsonValue))
                return it;
            if (false) {
                var writer = System.Runtime.Serialization.Json.JsonReaderWriterFactory.CreateJsonWriter (new MemoryStream (), System.Text.Encoding.UTF8, true);
                var ser = new System.Runtime.Serialization.Json.DataContractJsonSerializer (typeof (Paging));
                ser.WriteStartObject (writer, it);
                ser.WriteEndObject (writer);
            }
            int parseint (string value) {
                int.TryParse (value, out var val);
                return val;
            }
            bool parsebool (string value) {
                bool.TryParse (value, out var val);
                return val;
            }
            it = it ?? new Paging ();
            var props = jsonValue.Substring (1, jsonValue.Length - 2).Replace ("\"", "").Split (',');

            foreach (var prop in props.Select (p => p.Split (':'))) {
                var name = prop [0]; var propval = prop [1];
                if (name == nameof (Paging.Skip)) it.Skip = parseint (propval);
                if (name == nameof (Paging.Take)) it.Take = parseint (propval);
                if (name == nameof (Paging.Count)) it.Count = parseint (propval);
                if (name == nameof (Paging.Limit)) it.Limit = parseint (propval);
                if (name == nameof (Paging.CountRequired)) it.CountRequired = parsebool (propval);
                if (name == nameof (Paging.DataRequired)) it.DataRequired = parsebool (propval);

            }

            return it;
        }

    }
}