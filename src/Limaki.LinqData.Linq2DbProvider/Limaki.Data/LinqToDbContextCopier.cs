using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Limaki.Common.Linqish;
using Limaki.Common.Reflections;
using LinqToDB;
using LinqToDB.Data;
using LinqToDB.Mapping;

namespace Limaki.Data {

    /// <summary>
    /// copies all tables from one Dataconnection to another
    /// </summary>
    public class LinqToDbContextCopier {

        public Action<string> OnStart { get; set; }

        public Action<string> OnEnd { get; set; }

        public Action<string> OnProgress { get; set; }

        public void Copy<T> (T sourceConnection, T sinkConnection, bool generateIndices = false) where T : DataConnection {

            CopyDataConnection (sourceConnection, sinkConnection);

            if (generateIndices) {
                GenerateIndices (sinkConnection);
            }
        }

        public void GenerateIndices<T> (T sinkConnection) where T : DataConnection {

            var idxBuilder = new LinqToDBIndexBuilder (sinkConnection);

            foreach (var p in sinkConnection.GetType ().GetProperties ()
               .Where (p => p.PropertyType.IsGenericQueryableType ())) {

                var type = p.PropertyType.GetGenericArguments ()[0];

                OnStart?.Invoke ($"{nameof(idxBuilder.CheckIndices)} for {type.FriendlyClassName ()}");

                idxBuilder.CheckIndices (type);
            }
        }

        /// <summary>
        /// copy sources into dataconnection
        /// if ITable&lt;E&gt; not exists, its created
        /// if ITable&lt;E&gt; is not empty, nothing is copied
        /// </summary>
        /// <param name="dataConnection"></param>
        /// <param name="sources"></param>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="E"></typeparam>
        public BulkCopyRowsCopied CopyToDataConnection<T, E> (T dataConnection, IEnumerable<E> sources) where T : DataConnection where E : class {

            var msg = $"{nameof(CopyToDataConnection)} {typeof(T).FriendlyClassName ()}<{typeof(E).FriendlyClassName ()}>";
            ITable<E> dest = default;

            OnStart?.Invoke (msg);

            if (sources is ITable<E> source) {
                if (!dataConnection.TableSchemas ().Any (t => t.TableName == source.TableName)) {
                    dest = dataConnection.CreateTable<E> (source.TableName);
                } else {
                    dest = dataConnection.GetTable<E> ();
                }
            }

            if (dest?.Any () ?? true) {
                OnEnd?.Invoke ($"{msg}\t: {dest.TableName} is not empty, copy denied");

                return default;
            }

            var options = new BulkCopyOptions ();
            
            options.RowsCopiedCallback = copied => OnProgress?.Invoke ($"{copied.RowsCopied}");
            
            var result = dataConnection.BulkCopy (options, sources);

            OnEnd?.Invoke ($"{msg}\t {result.RowsCopied} copied");

            return result;
        }

        public void CopyDataConnection<T> (T sourceConnection, T sinkConnection) where T : DataConnection {

            var bulkCopy = new CallCache (ExpressionUtils.Lambda<Action<T, IEnumerable<CallCache.Entity>>> ((dataConnection, sources) => CopyToDataConnection (dataConnection, sources)));

            foreach (var queryableProp in typeof(T).GetProperties ()
               .Where (p => p.PropertyType.IsGenericTableType ())) {

                using var trans = sinkConnection.BeginTransaction ();

                var sourceTable = queryableProp.GetValue (sourceConnection);
                var type = queryableProp.PropertyType.GenericTypeArguments.First ();

                var meth = bulkCopy.Getter (type);

                meth.DynamicInvoke (sinkConnection, sourceTable);

                trans.Commit ();

            }
        }

    }

    public static class LinqToDbContextCopierExtensions {

        /// <summary>
        /// matching ITable&lt;E&gt; the of <see cref="DataConnection"/>
        /// </summary>
        public static ITable<E> TableOf<E> (this DataConnection dataConnection) {

            var queryableProperty = dataConnection.GetType ().GetProperties ().FirstOrDefault (
                p => p.PropertyType == typeof(ITable<E>));

            return queryableProperty?.GetValue (dataConnection) as ITable<E>;
        }

        public static bool IsGenericTableType (this Type type) {
            if (type.IsGenericType)
                if (typeof(ITable<>).IsSameOrParentOf (type))
                    return true;

            return false;
        }

    }

}