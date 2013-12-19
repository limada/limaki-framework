/*
 * Limaki 
 * 
 * This code is free software; you can redistribute it and/or modify it
 * under the terms of the GNU General Public License version 2 only, as
 * published by the Free Software Foundation.
 * 
 * Author: Lytico
 * Copyright (C) 2011 Lytico
 *
 * http://www.limada.org
 * 
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Mono.Linq.Expressions;

namespace Limaki.Common.Linqish {

    public class Mapping<T> {

        protected ICollection<Tuple<LambdaExpression, LambdaExpression, bool>> _mappings =
            new List<Tuple<LambdaExpression, LambdaExpression, bool>>();

        public void Add<R>(Expression<Func<T, R>> a, Expression<Func<T, R>> b) {
            Add(a, b, false);
        }

        public void Add<R>(Expression<Func<T, R>> a, Expression<Func<T, R>> b, bool assign) {
            _mappings.Add(Tuple.Create((LambdaExpression)a, (LambdaExpression)b, assign));
        }

        public bool Remove<R>(Expression<Func<T, R>> a) {
            var val = _mappings.Where(t => t.Item1.ToCSharpCode()==a.ToCSharpCode()).FirstOrDefault();
            if (val != null)
                return _mappings.Remove(val);
            return false;
        }

        public Expression<Func<T, bool>> BuildPredicate() {
            ParameterExpression[] param = null;
            Func<Tuple<LambdaExpression, LambdaExpression, bool>, Expression> make = ex => {
                param = ex.Item1.Parameters.ToArray();
                return
                    Expression.MakeBinary(
                        ExpressionType.NotEqual, ex.Item1.Body, ex.Item2.Body);
            };
            var mappings = _mappings.Where(t => !t.Item3);
            var body = make(mappings.First());
            foreach (var exp in mappings.Skip(1)) {
                var e2 = make(exp);
                body = Expression.MakeBinary(ExpressionType.OrElse, body, e2);
            }
            return Expression.Lambda<Func<T, bool>>(body, param);

        }

        public class ParameterChanger<C> : ExpressionVisitor {
            ParameterExpression _var = null;
            public ParameterChanger(ParameterExpression var) {
                _var = var;
            }

            protected override Expression VisitMember(MemberExpression node) {
                if (node.Member.ReflectedType == typeof(C)) {
                    return Expression.Property(_var, node.Member as PropertyInfo);
                } else
                    return base.VisitMember(node);
            }
        }

        public Expression<Action<T>> BuildAssign() {
            var para = Expression.Variable(typeof(T), "a");
            ParameterExpression[] paras = new ParameterExpression[] { para };

            var mappings = _mappings;
            var blockExpressions = new List<Expression>();
            var changer = new ParameterChanger<T>(para);
            foreach (var exp in mappings) {
                //param = exp.Item1.Parameters.ToArray();
                var exp1 = changer.Visit(exp.Item1.Body);
                var exp2 = changer.Visit(exp.Item2.Body);

                blockExpressions.Add(Expression.Assign(exp1, exp2));
            }

            return Expression.Lambda<Action<T>>(Expression.Block(blockExpressions), paras);
        }

        public Expression<Func<T, bool>> BuildEquals() {
            var para = Expression.Variable(typeof(T), "a");
            ParameterExpression[] paras = new ParameterExpression[] { para };
            var changer = new ParameterChanger<T>(para);
            Func<Tuple<LambdaExpression, LambdaExpression, bool>, Expression> make = ex => {
                return
                    Expression.MakeBinary(
                        ExpressionType.Equal, changer.Visit(ex.Item1.Body), changer.Visit(ex.Item2.Body));
            };
            var mappings = _mappings;
            var body = make(mappings.First());
            foreach (var exp in mappings.Skip(1)) {
                var e2 = make(exp);
                body = Expression.MakeBinary(ExpressionType.AndAlso, body, e2);
            }
            return Expression.Lambda<Func<T, bool>>(body, paras);

        }
    }
}