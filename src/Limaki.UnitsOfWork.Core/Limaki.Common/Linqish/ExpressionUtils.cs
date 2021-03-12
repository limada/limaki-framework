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
using System.Linq.Expressions;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Limaki.Common.Linqish {

    public static class ExpressionUtils {

        public static MemberInfo MemberInfo<T, TMember>(this Expression<Func<T, TMember>> exp) {
            if (exp.Body is MemberExpression member)
                return member.Member;
            if (exp.Body is MethodCallExpression method)
                return method.Method;
            throw new ArgumentException(string.Format("{0} is not a MemberExpression", exp.ToString()));
        }

        public static string Nameof<T, TMember>(Expression<Func<T, TMember>> exp) {
            return MemberInfo(exp).Name;
        }

        public static Expression<Func<T, TMember>> ReplaceBody<T, TMember> (Expression<Func<T, TMember>> exp, Expression replace, bool right) {
            var body = exp.Body as BinaryExpression;
            if (body == null)
                throw new NotSupportedException ("Only BinaryExpressions are supported ");
            if (right)
                body = Expression.MakeBinary (body.NodeType, body.Left, replace);
            else
                body = Expression.MakeBinary (body.NodeType, replace, body.Right);
            return Expression.Lambda<Func<T, TMember>> (body, exp.Parameters);
        }

        /// <summary>
        /// Expression<T> expr = (...) => ...;
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="expr"></param>
        /// <returns></returns>
        public static Expression<T> Lambda<T>(Expression<T> expr) {
            return expr;
        }

        public static IQueryable<T> WhereIf<T> (this IQueryable<T> query, Expression<Func<T, bool>> whereClause) {
            if (whereClause != null) {
                return query.Where (whereClause);
            }
            return query;

        }



        /// <summary>
        /// Returns the ParameterExpression for the LINQ parameter.
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        /// <remarks>from: AutoMapper.Extensions.ExpressionMapping/Extensions/VisitorExtensions</remarks>
        public static ParameterExpression GetParameterExpression (this Expression expression) {
            if (expression == null)
                return null;

            //the node represents parameter of the expression
            switch (expression.NodeType) {
            case ExpressionType.Parameter:
                return (ParameterExpression)expression;
            case ExpressionType.Quote:
                return GetParameterExpression (GetMemberExpression ((LambdaExpression)((UnaryExpression)expression).Operand));
            case ExpressionType.Lambda:
                return GetParameterExpression (GetMemberExpression ((LambdaExpression)expression));
            case ExpressionType.ConvertChecked:
            case ExpressionType.Convert:
                var ue = expression as UnaryExpression;
                return GetParameterExpression (ue?.Operand);
            case ExpressionType.MemberAccess:
                return GetParameterExpression (((MemberExpression)expression).Expression);
            case ExpressionType.Call:
                var methodExpression = expression as MethodCallExpression;
                var memberExpression = methodExpression?.Object as MemberExpression;//Method is an instance method

                var isExtension = methodExpression != null && methodExpression.Method.IsDefined (typeof (ExtensionAttribute), true);
                if (isExtension && memberExpression == null && methodExpression.Arguments.Count > 0)
                    memberExpression = methodExpression.Arguments [0] as MemberExpression;//Method is an extension method based on the type of methodExpression.Arguments[0] and methodExpression.Arguments[0] is a member expression.

                return isExtension && memberExpression == null && methodExpression.Arguments.Count > 0
                    ? GetParameterExpression (methodExpression.Arguments [0])
                    : (memberExpression == null ? null : GetParameterExpression (memberExpression.Expression));
            }

            return null;
        }

        /// <summary>
        /// Gets the member expression.
        /// </summary>
        /// <returns>The member expression.</returns>
        /// <param name="expr">Expr.</param>
        /// <remarks>from: AutoMapper.Extensions.ExpressionMapping/Extensions/VisitorExtensions</remarks>
        public static MemberExpression GetMemberExpression (LambdaExpression expr) {
            MemberExpression me;
            switch (expr.Body.NodeType) {
            case ExpressionType.Convert:
            case ExpressionType.ConvertChecked:
                var ue = expr.Body as UnaryExpression;
                me = ue?.Operand as MemberExpression;
                break;
            default:
                me = expr.Body as MemberExpression;
                if (me == null) {
                    if (expr.Body is BinaryExpression binaryExpression) {
                        if (binaryExpression.Left is MemberExpression left)
                            return left;
                        if (binaryExpression.Right is MemberExpression right)
                            return right;
                    }
                }
                break;
            }

            return me;
        }

        public static string NullMember<E, M>(E q, Expression<Func<E, M>> member, bool addValue = false)
        {
            if (!(member.Body is MemberExpression me && me.Member is PropertyInfo prop))
                return string.Empty;

            var r = $"{prop.Name}";
            if (q == null)
                return r;
            object val = prop.GetValue(q);

            return val == null ? $"{r}==null" : addValue ? $"{r}:{val}" : $"{r}";
        }
    }
}