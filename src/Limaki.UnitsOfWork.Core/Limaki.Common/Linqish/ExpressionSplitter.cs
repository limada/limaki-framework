/*
 * Limaki 
 * 
 * This code is free software; you can redistribute it and/or modify it
 * under the terms of the GNU General Public License version 2 only, as
 * published by the Free Software Foundation.
 * 
 * Author: Lytico
 * Copyright (C) 2019 Lytico
 *
* http://www.limada.org
 * 
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Limaki.Common.Linqish {

    /// <summary>
    /// splits a <see cref="LambdaExpression"/>
    /// into parts of type:
    /// E is an argument
    /// M is the Expression type
    /// </summary>
    public class ExpressionSplitter<E, M> {

        public bool CheckIfProperty (Expression expression) =>
            expression is MemberExpression me //&& me.Member.MemberType == System.Reflection.MemberTypes.Property
            && me.Expression is ParameterExpression p && p.Type == typeof(E);

        public MemberExpression GetProperty (Expression expression) {
            Expression result = null;
            var visitor = new ExpressionVisitVisitor ();
            visitor.VisitMemberFunc = v => {
                if (CheckIfProperty (v))
                    result = v;
                return v;
            };
            visitor.Visit (expression);
            return result as MemberExpression;
        }

        public IEnumerable<MemberExpression> GetProperties (Expression expression) {
            var result = new Queue<MemberExpression> ();
            var visitor = new ExpressionVisitVisitor ();
            visitor.VisitMemberFunc = v => {
                if (CheckIfProperty (v))
                    result.Enqueue (v);
                return v;
            };
            visitor.Visit (expression);
            return result;
        }

        public int CountProperty (Expression expression) {
            int result = 0;
            var visitor = new ExpressionVisitVisitor ();
            visitor.VisitMemberFunc = v => {
                if (CheckIfProperty (v))
                    result++;
                return v;
            };

            visitor.Visit (expression);
            return result;
        }

        public IEnumerable<Expression> Split (Expression<Func<E, M>> lambda) {
            var result = new Queue<Expression> ();
            var nodeTypes = new Queue<ExpressionType> ();

            var visitor = new ExpressionVisitVisitor ();
            var candidates = new Stack<Expression> ();

            Expression Pop () => candidates.Any () ? candidates.Pop () : default;

            void Take () {
                var cand = Pop ();
                if (cand != null && CountProperty (cand) == 1)
                    result.Enqueue (cand);
            }

            visitor.VisitBinaryFunc = v => {
                if (v.Type == typeof(M)) {
                    candidates.Push (v);
                }

                visitor.BaseVisit (v.Left);
                visitor.BaseVisit (v.Right);

                return v;
            };

            visitor.VisitMethodCallFunc = v => {
                if (v.Type == typeof(M)) {
                    candidates.Push (v);
                    visitor.BaseVisit (v.Object);
                    return v;
                }

                visitor.BaseVisit (v.Object);
                return v;
            };

            visitor.Visit (lambda);
            while (candidates.Count != 0) {
                Take ();
            }

            return result;
        }

        public IEnumerable<Expression> Split (Expression<Func<E, M>> lambda, HashSet<MemberInfo> valid) {
            bool CheckIfValid (Expression expression) =>
                expression is MemberExpression {Expression: ParameterExpression p} me 
                && p.Type == typeof(E) 
                && valid.Contains (me.Member);

            bool HasOnlyValidProperties (Expression expression) => GetProperties (expression).All (CheckIfValid);

            if (HasOnlyValidProperties (lambda)) {
                return new[] {lambda};
            }

            var visitor = new ExpressionVisitVisitor ();
            var candidates = new Queue<Expression> ();
            visitor.VisitConstantFunc = v => {
                return v;
            };
            visitor.VisitBinaryFunc = v => {
                if (v.Type == typeof(M) && HasOnlyValidProperties (v)) {
                    candidates.Enqueue (v);
                    return v;
                }

                visitor.BaseVisit (v.Left);
                visitor.BaseVisit (v.Right);

                return v;
            };

            visitor.VisitMethodCallFunc = v => {
                if (v.Type == typeof(M) && HasOnlyValidProperties (v)) {
                    candidates.Enqueue (v);
                    return v;
                }

                visitor.BaseVisit (v.Object);
                return v;
            };
            visitor.VisitLambdaFunc = v => {
                if (HasOnlyValidProperties (v) && v is Expression<Func<E, M>>) {
                    candidates.Enqueue (v);
                    return v;
                }

                visitor.BaseVisit (v.Body);
                return v;
            };
            visitor.Visit (lambda);
            return candidates;
        }

        public IEnumerable<Expression<Func<E, M>>> LambdasOfSplit (Expression<Func<E, M>> lambda) {
            foreach (var expression in Split (lambda)) {
                yield return Expression.Lambda<Func<E, M>> (expression, lambda.Parameters);
            }
        }

        public IEnumerable<Expression<Func<E, M>>> LambdasOfSplit (Expression<Func<E, M>> lambda, HashSet<MemberInfo> valid) {
            foreach (var expression in Split (lambda, valid)) {
                if (expression is Expression<Func<E, M>> lambdaExpression) {
                    yield return lambdaExpression;
                    continue;
                }

                yield return Expression.Lambda<Func<E, M>> (expression, lambda.Parameters);
            }
        }
    }

}