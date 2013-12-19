﻿using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace MetaLinq
{
    [DataContract]
    public class EditableNewExpression : EditableExpression
    {
        // Properties                
        [XmlIgnore]
        public ConstructorInfo Constructor
        {
            get;
            set;
        }

        [DataMember]
        public EditableExpressionCollection Arguments
        {
            get;
            set;
        }

        [DataMember]
        public EditableMemberInfoCollection Members
        {
            get;
            set;
        }

        [DataMember]
        public string ConstructorName
        {
            get { return Constructor.ToSerializableForm(); }
            set { Constructor = Constructor.FromSerializableForm(value); }
        }
        
        public override ExpressionType NodeType
        {
            get { return ExpressionType.New; }
            set { }
        }

        // Ctors
        public EditableNewExpression()
        {
        }

        public EditableNewExpression(NewExpression newEx)
            : this(newEx.Constructor, new EditableExpressionCollection(newEx.Arguments), newEx.Members, newEx.Type)
        { }

        public EditableNewExpression(ConstructorInfo constructor, IEnumerable<EditableExpression> arguments, IEnumerable<MemberInfo> members, Type type)
            : this(constructor, new EditableExpressionCollection(arguments), members, type)
        { }

        public EditableNewExpression(ConstructorInfo constructor, EditableExpressionCollection arguments, IEnumerable<MemberInfo> members, Type type)
            : base(type)
        {
            Arguments = arguments;
            Constructor = constructor;            
            Members = new EditableMemberInfoCollection(members);
        }

        // Methods
        public override Expression ToExpression()
        {
            if (Constructor != null)
                return Expression.New(Constructor, Arguments.GetExpressions());
            else
                return Expression.New(Type);
        }
    }
}
