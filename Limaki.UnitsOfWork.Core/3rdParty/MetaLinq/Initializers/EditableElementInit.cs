﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Runtime.Serialization;
using System.Linq.Expressions;
using System.Xml.Serialization;

namespace MetaLinq
{
    [DataContract]
    public class EditableElementInit
    {
        // Properties
        [DataMember]
        public EditableExpressionCollection Arguments
        {
            get;
            set;
        }

        [XmlIgnore]
        public MethodInfo AddMethod
        {
            get;
            set;
        }

        [DataMember]
        public string AddMethodName
        {
            get { return AddMethod.ToSerializableForm(); }
            set { AddMethod = AddMethod.FromSerializableForm(value); }
        }

        // Ctors
        public EditableElementInit()
        {
            Arguments = new EditableExpressionCollection();
        }

        public EditableElementInit(ElementInit elmInit) 
            : this()
        {
            AddMethod = elmInit.AddMethod;
            foreach (Expression ex in elmInit.Arguments)
            {
                Arguments.Add(EditableExpression.CreateEditableExpression(ex));
            }
        }

        // Methods
        public ElementInit ToElementInit()
        {
            return Expression.ElementInit(AddMethod, Arguments.GetExpressions());
        }
    }
}
