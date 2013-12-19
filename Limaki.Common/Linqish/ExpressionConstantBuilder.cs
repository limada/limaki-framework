/*
 * Limaki 
 * 
 * This code is free software; you can redistribute it and/or modify it
 * under the terms of the GNU General Public License version 2 only, as
 * published by the Free Software Foundation.
 * 
 * Author: Lytico
 * Copyright (C) 2012 Lytico
 *
 * http://www.limada.org
 * 
 */

using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System;

namespace Limaki.Common.Linqish {
    /// <summary>
    /// replaces local variables with constants
    /// </summary>
    public class ExpressionConstantBuilder : ExpressionVisitor {

        ICollection<ParameterExpression> sourceParam;

        public Expression<T> Replace<T>(Expression<T> source) {
            if (source == null)
                return null;
            this.sourceParam = source.Parameters;
            var result = base.Visit(source);
            return result as Expression<T>;
        }

        protected override Expression VisitMember(MemberExpression node) {
            //if (!sourceParam.Contains(node.Expression as ParameterExpression)) 
            if (IsConstant(node)) {
                var val = ConstantValue(node);
                return Expression.Constant(val, node.Type);
            }
            return base.VisitMember(node);
        }

        protected override Expression VisitMethodCall(MethodCallExpression node) {
            
            Func<MethodCallExpression, bool> isConstant = n =>
                IsConstant(n.Object) || (n.Object == null && n.Method.Attributes.HasFlag(MethodAttributes.Static));

            if (node.Arguments.Count == 0 && isConstant(node))
                return Expression.Constant(ConstantValue(node), node.Type);
            var args = new List<Expression>();
            var argsHaveConsts = false;
            var allArgsAreConsts = true;
            foreach (var arg in node.Arguments) {
                if (IsConstant(arg)) {
                    var val = ConstantValue(arg);
                    args.Add(Expression.Constant(val, arg.Type));
                    argsHaveConsts = true;
                } else {
                    args.Add(arg);
                    allArgsAreConsts = false;
                }
            }
            if (args.Count > 0 && argsHaveConsts) {
                var result = Expression.Call(node.Object, node.Method, args);
                if (isConstant(result) && allArgsAreConsts)
                    return Expression.Constant(ConstantValue(result), result.Type);
                return result;
            }
            return base.VisitMethodCall(node);
        }

        class ConstantIs : ExpressionVisitor {
            int constFound = 0;
            public bool HasConstant(Expression ex) {
                constFound = 0;
                Visit(ex);
                return constFound != 0;
            }
            protected override Expression VisitConstant(ConstantExpression node) {
                constFound ++;
                return base.VisitConstant(node);
            }
            protected override Expression VisitMethodCall(MethodCallExpression node) {
                Visit(node.Arguments);
                return base.VisitMethodCall(node);
            }
        }

        bool IsConstant(Expression expression) {
            return new ConstantIs().HasConstant(expression);
        }

        private static object ConstantValue(Expression expression) {
            object value;
            if (!TryGetContantValue(expression, out value)) { // fallback:
                value = Expression.Lambda(expression).Compile().DynamicInvoke();
            }
            return value;
        }

        private static bool TryGetContantValue(Expression expression, out object value) {
            if (expression == null) {   // used for static fields, etc
                value = null;
                return true;
            }
            switch (expression.NodeType) {
                case ExpressionType.Constant:
                    value = ((ConstantExpression)expression).Value;
                    return true;
                case ExpressionType.MemberAccess:
                    MemberExpression memberExpression = (MemberExpression)expression;
                    object target;
                    if (TryGetContantValue(memberExpression.Expression, out target)) {
                        switch (memberExpression.Member.MemberType) {
                            case MemberTypes.Field:
                                value = ((FieldInfo)memberExpression.Member).GetValue(target);
                                return true;
                            case MemberTypes.Property:
                                value = ((PropertyInfo)memberExpression.Member).GetValue(target, null);
                                return true;
                        }
                    }
                    break;
            }
            value = null;
            return false;
        }

    }
}