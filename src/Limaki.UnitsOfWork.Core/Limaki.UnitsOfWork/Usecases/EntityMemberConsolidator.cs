using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Limaki.UnitsOfWork.Usecases {
    
    public class EntityMemberConsolidator<E> {

        public Func<IEnumerable<E>> EntityGetter { get; set; }

        public IEnumerable<E> Entitys => EntityGetter?.Invoke ();

        public Expression<Func<E, string>> MemberExpression { get; set; }

        Func<E, string> _memberFunc = null;
        public Func<E, string> MemberFunc => _memberFunc ?? (_memberFunc = MemberExpression.Compile ());

        public IEnumerable<IGrouping<string, E>> Doubles => Entitys.Where (e => !string.IsNullOrWhiteSpace (MemberFunc (e)))
                                                                   .GroupBy (e => MemberFunc (e))
                                                                  .Where (g => g.Count () > 1);

        public IEnumerable<E> Emptys => Entitys.Where (e => string.IsNullOrWhiteSpace (MemberFunc (e)));
        public IEnumerable<E> DoubleFirsts => Doubles.Select (g => g.First ());
        public IEnumerable<E> Consolidateds => Entitys.Except (Doubles.SelectMany (g => g)).Union (DoubleFirsts);

    }
}
