﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Linq.Expressions;

namespace MetaLinq
{
    [DataContract]
    public class EditableMemberListBinding : EditableMemberBinding
    {
        // Properties
        [DataMember]
        public EditableElementInitCollection Initializers
        {
            get;
            set;
        }

        public override MemberBindingType BindingType
        {
            get { return MemberBindingType.ListBinding; }
            set { }
        }

        // Ctors
        public EditableMemberListBinding()
        {
            Initializers = new EditableElementInitCollection();
        }

        public EditableMemberListBinding(MemberListBinding member) : base(member.BindingType, member.Member)
        {
            Initializers = new EditableElementInitCollection();
            foreach (ElementInit e in member.Initializers)
            {
                Initializers.Add(new EditableElementInit(e));
            }
        }

        // Methods
        public override MemberBinding ToMemberBinding()
        {
            return Expression.ListBind(Member, Initializers.GetElementsInit());
        }

    }
}
