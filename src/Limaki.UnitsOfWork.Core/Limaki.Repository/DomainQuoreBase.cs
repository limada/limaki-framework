/*
 * Limada 
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
using System.IO;
using Limaki.Common.Linqish;

namespace Limaki.Repository {

    public abstract class DomainQuoreBase : IDomainQuore {
        
        public DomainQuoreBase (Func<IQuore> createQuore) {
            this.CreateQuore = () => {
                var c = createQuore ();
                c.Log = this.Log;
                return c;
            };
        }

        public Func<IQuore> CreateQuore { get; protected set; }

        public abstract IQuore Quore {get;}

        ExpressionCache _expressionCache = null;
        public ExpressionCache ExpressionCache {
            get { return _expressionCache ?? (_expressionCache = new ExpressionCache ()); }
            set { _expressionCache = value; }
        }

        TextWriter _log = null;

        public virtual TextWriter Log {
            get { return _log ?? (_log = new StringWriter ()); }
            set { _log = value; }
        }

        public abstract void Dispose ();

    }

    public class QuoreBase : IDomainQuore {

        public QuoreBase (Func<IQuore> createQuore) {
            this.CreateQuore = () => {
                var c = createQuore ();
                c.Log = this.Log;
                return c;
            };
        }

        protected Func<IQuore> CreateQuore { get; set; }

        IQuore _quore;
        public IQuore Quore => _quore ?? (_quore = CreateQuore());

        TextWriter _log = null;

        public virtual TextWriter Log {
            get { return _log ?? (_log = new StringWriter ()); }
            set { _log = value; }
        }

        public virtual void Dispose () {
            _quore?.Dispose ();
            _quore = null;
        }

    }

}