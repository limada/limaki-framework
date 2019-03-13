/*
 * Limaki 
 * 
 * This code is free software; you can redistribute it and/or modify it
 * under the terms of the GNU General Public License version 2 only, as
 * published by the Free Software Foundation.
 * 
 * Author: Lytico
 * Copyright (C) 2017 Lytico
 *
 * http://www.limada.org
 * 
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Limaki.Common;

namespace Limaki.UnitsOfWork {

    public class Validator {


        // TODO: 
        // introduce a sort of validation chain with ValidationRule

        // TODO: introduce a sort of ValidationRule - class
        // and make it fluent


        // public Func<ValidationException, bool> OnError { get; set; }

        Dictionary<int, Delegate> _validChange = new Dictionary<int, Delegate> ();

        Dictionary<int, Delegate> _memberChanged = new Dictionary<int, Delegate> ();

        Dictionary<Type, Delegate> _entityChanged = new Dictionary<Type, Delegate> ();

        int Key<T, M> (string member) {
            return KeyMaker.GetHashCode (typeof (T), member);
        }

        public void Add<T, M> (Func<T, M, M, string, bool> validate, string member) {
            var key = Key<T, M> (member);

            if (_validChange.TryGetValue (key, out var d)) {
                foreach (var dd in d.GetInvocationList ()) {
                    if (dd.Equals (validate))
                        return;
                }
                var f = (Func<T, M, M, string, bool>)d;
                f += validate;
                validate = f;
            }
            _validChange[key] = validate;
        }

        /// <summary>
        /// T item, M oldValue, M newValue, string member
        /// more than one validation per member is allowed
        /// they are called in order of adding
        /// </summary>
        /// <param name="member">Member.</param>
        /// <param name="validate">Validate.</param>
        /// <typeparam name="T">item</typeparam>
        /// <typeparam name="M">value</typeparam>
        public void Add<T, M> (Expression<Func<T, M>> member, Func<T, M, M, string, bool> validate) {
            if (member.Body is MemberExpression me) {
                Add (validate, me.Member.Name);
                return;
            }
            throw new ArgumentException ($"{nameof (member)} must be of type {nameof (MemberExpression)}");
        }

        /// <summary>
        /// T item, M oldValue, M newValue, string member
        /// more than one validation per member is allowed
        /// they are called in order of adding
        /// </summary>
        /// <returns>is a valid change</returns>
        /// <param name="member">Member.</param>
        /// <typeparam name="T">item</typeparam>
        /// <typeparam name="M">value</typeparam>
        public bool IsValidChange<T, M> (T item, M oldValue, M newValue, string member, bool omitEqualityCheck = false) {

            // TODO: check if member is a "virtual" field, not a member of T
            try {
                if (!omitEqualityCheck) {
                    if (object.Equals (oldValue, newValue))
                        return true;
                }

                if (_validChange.TryGetValue (Key<T, M> (member), out var d)) {
                    var result = true;
                    foreach (var dd in d.GetInvocationList ()) {
                        var f = (Func<T, M, M, string, bool>)dd;
                        result &= f (item, oldValue, newValue, member);
                    }
                    return result;
                }
                return true;
            } catch (Exception ex) {
                Trace.TraceError ($"ERROR {nameof (Validator)}: {ex.Message} ");
                throw ex;
                return false;
            }
        }

        public bool IsValidChange<T, M> (Expression<Func<T, M>> member, T item, M oldValue, M newValue) {
            if (member.Body is MemberExpression me) {
                return IsValidChange (item, oldValue, newValue, me.Member.Name);
            }
            throw new ArgumentException ($"{nameof (member)} must be of type {nameof (MemberExpression)}");
        }

        MethodInfo validatorCall = typeof (Validator)
            .GetMethods ().First (m => m.Name == nameof (Validator.IsValidChange) && m.GetParameters ()[3].ParameterType == typeof (string));

        public bool IsValidChange<T> (T item, PropertyInfo member, object newValue) {

            var func = validatorCall.MakeGenericMethod (new[] { typeof (T), member.PropertyType });

            return (bool)func.Invoke (this, new object[] { item, member.GetValue (item), newValue, member.Name, false });

        }

        public void Add<T, M> (Action<T, M, M, string> memberChanged, string member) {
            var key = Key<T, M> (member);

            if (_memberChanged.TryGetValue (key, out var d)) {
                foreach (var dd in d.GetInvocationList ()) {
                    if (dd.Equals (memberChanged))
                        return;
                }
                var f = (Action<T, M, M, string>)d;
                f += memberChanged;
                memberChanged = f;
            }
            _memberChanged[key] = memberChanged;
        }

        /// <summary>
        /// Add Check after member has changed
        /// </summary>
        /// <param name="member">Member-Expression</param>
        /// <param name="memberChanged">Action(entity,old,new,membername)</param>
        /// <typeparam name="T">type of entity</typeparam>
        /// <typeparam name="M">type of member</typeparam>
        public void Add<T, M> (Expression<Func<T, M>> member, Action<T, M, M, string> memberChanged) {
            if (member.Body is MemberExpression me) {
                Add (memberChanged, me.Member.Name);
                return;
            }
            throw new ArgumentException ($"{nameof (member)} must be of type {nameof (MemberExpression)}");
        }

        public void MemberChanged<T, M> (Expression<Func<T, M>> member, T item, M oldValue, M newValue) {
            if (member.Body is MemberExpression me) {
                MemberChanged (item, oldValue, newValue, me.Member.Name, false);
                return;
            }
            throw new ArgumentException ($"{nameof (member)} must be of type {nameof (MemberExpression)}");
        }

        public void MemberChanged<T, M> (T item, M oldValue, M newValue, string member, bool omitEqualityCheck) {
            try {
                if (!omitEqualityCheck) {
                    if (object.Equals (oldValue, newValue))
                        return;
                }

                if (_memberChanged.TryGetValue (Key<T, M> (member), out var d)) {
                    foreach (var dd in d.GetInvocationList ()) {
                        var f = (Action<T, M, M, string>)dd;
                        f (item, oldValue, newValue, member);
                    }
                }
                EntityChanged (item, member);
            } catch (Exception ex) {
                Trace.TraceError ($"ERROR {nameof (Validator)}: {ex.Message} ");
                throw ex;
            }
        }

        MethodInfo memberChangedCall = typeof (Validator)
        .GetMethods ().First (m => m.Name == nameof (Validator.MemberChanged) && m.GetParameters ()[3].ParameterType == typeof (string));

        public void MemberChanged<T> (T item, PropertyInfo member, object oldValue, object newValue) {

            var action = memberChangedCall.MakeGenericMethod (new[] { typeof (T), member.PropertyType });

            action.Invoke (this, new object[] { item, oldValue, newValue, member.Name, false });
        }

        #region EntityChanged

        public void Add<T> (Action<T, string> validate) {
            var key = typeof (T);
            if (_entityChanged.TryGetValue (key, out var d)) {
                foreach (var dd in d.GetInvocationList ()) {
                    if (dd.Equals (validate))
                        return;
                }
                var f = (Action<T, string>)d;
                f += validate;
                validate = f;
            }
            _entityChanged[key] = validate;
        }

        public void EntityChanged<T> (T item, string member = null) {
            try {
                var key = typeof (T);
                if (_entityChanged.TryGetValue (key, out var d)) {
                    var f = (Action<T, string>)d;
                    f (item, member);
                }
            } catch (Exception ex) {
                Trace.TraceError ($"ERROR {nameof (Validator)}: {ex.Message} ");
                throw ex;
            }
        }
        #endregion

    }

    public class ValidationException : Exception {
        public ValidationException (string message) : base (message) { }
    }
}
