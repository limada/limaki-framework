/*
 * Limada 
 *
 * This code is free software; you can redistribute it and/or modify it
 * under the terms of the GNU General Public License version 2 only, as
 * published by the Free Software Foundation.
 * 
 * Author: Lytico
 * Copyright (C) 2014 - 2017 Lytico
 *
 * http://www.limada.org
 * 
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;

namespace Limaki.Data {

    /// <summary>
    /// Wraps another <see cref="IQuore"/> (the innerStore) with an <see cref="IQuoreMapper"/>
    /// </summary>
    public class MappingQuore : IQuore {

        public MappingQuore (IQuore innerQuore, IQuoreMapper mapper) {
            this.Mapper = mapper;
            this.InnerQuore = innerQuore;
        }

        public IQuoreMapper Mapper { get; protected set; }

        public Action Disposed { get; set; }

        IQuore _innerQuore = null;
        public IQuore InnerQuore {
            get { return _innerQuore; }
            protected set {
                var convertable = value as IConvertableQuore;
                if (convertable != null)
                    convertable.Convert = Mapper.Map;
                _innerQuore = value;
            }
        }

        public IGateway Gateway {
            get { return InnerQuore.Gateway; }
        }

        public TextWriter Log {
            get { return InnerQuore.Log; }
            set { InnerQuore.Log = value; }
        }

        public virtual IQueryable<T> GetQuery<T> () {

            var result = Mapper.MapQuery<T> (this);
            if (result != null)
                return result;

            var mappedType = Mapper.MapIn (typeof (T));
            if (mappedType == null) {
                return InnerQuore.GetQuery<T> ();
            }

            Expression<Action> lambda = () => GetQuery<T> (); 
            var method = (lambda.Body as MethodCallExpression).Method
                .GetGenericMethodDefinition ().MakeGenericMethod (mappedType);

            return (IQueryable<T>) method.Invoke (this, new object[0]);

        }

        protected object EntitiesOfType<T> (IEnumerable<T> source, Type mappedType) {
            Expression<Func<IEnumerable<T>>> lambda = () => source.OfType<T> ();
            var method = (lambda.Body as MethodCallExpression).Method
                .GetGenericMethodDefinition ().MakeGenericMethod (mappedType);
            return method.Invoke (null, new object[] { source });
        }

        protected virtual void CudCall<T> (Expression<Action> lambda, IEnumerable<T> entities, Type mappedType) {

            var method = (lambda.Body as MethodCallExpression).Method
               .GetGenericMethodDefinition ().MakeGenericMethod (mappedType);

            var param = EntitiesOfType (Mapper.MapIn (entities), mappedType);
            method.Invoke (this, new object[] { param });
        }

        public virtual void Upsert<T> (IEnumerable<T> entities) {

            if (!entities.Any ())
                return;

            var mappedType = Mapper.MapIn (typeof (T));
            if (mappedType == null) {
                InnerQuore.Upsert (Mapper.MapIn (entities));
                return;
            }

            CudCall<T> (() => Upsert<T> (entities), entities, mappedType);

        }

        public virtual void Remove<T> (IEnumerable<T> entities) {
            if (!entities.Any ())
                return;

            var mappedType = Mapper.MapIn (typeof (T));
            if (mappedType == null) {
                InnerQuore.Remove (Mapper.MapIn (entities));
                return;
            }

            CudCall<T> (() => Remove<T> (entities), entities, mappedType);

        }

        public virtual void Remove<T> (Expression<Func<T, bool>> where) {

            var entityClass = Mapper.MapIn (typeof (T));
            if (entityClass == null) {
                InnerQuore.Remove (where);
                return;
            }

            Expression<Action> lambda = () => Remove<T> (where); 
            var method = (lambda.Body as MethodCallExpression).Method
                .GetGenericMethodDefinition ().MakeGenericMethod (entityClass);

            var param = (InnerQuore is IConvertableQuore) ? Mapper.Map (where, typeof(T)) : where;

            method.Invoke (this, new object[] { param });
        }

        public virtual void Dispose () {
            if (InnerQuore != null) {
                InnerQuore.Dispose ();
            }
            InnerQuore.Dispose ();
            if (Disposed != null)
                Disposed ();
        }

        public IQuoreTransaction BeginTransaction () {
            return InnerQuore.BeginTransaction ();
        }

        public void EndTransaction (IQuoreTransaction transaction) {
            InnerQuore.EndTransaction (transaction);
        }
    }
}