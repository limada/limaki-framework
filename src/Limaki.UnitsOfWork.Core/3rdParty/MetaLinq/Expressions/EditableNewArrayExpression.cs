﻿using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Linq.Expressions;
using System.Runtime.Serialization;

namespace MetaLinq
{
    [DataContract]
    public class EditableNewArrayExpression : EditableExpression
    {
        // Members           
        protected ExpressionType _nodeType;

        // Properties
        [DataMember]
        public EditableExpressionCollection Expressions
        {
            get;
            set;
        }
        
        public override ExpressionType NodeType
        {
            get
            {
                return _nodeType;
            }
            set
            {
                if (value == ExpressionType.NewArrayInit || value == ExpressionType.NewArrayBounds)
                    _nodeType = value;
                else
                    throw new InvalidOperationException("NodeType for NewArrayExpression must be ExpressionType.NewArrayInit or ExpressionType.NewArrayBounds");
            }
        }

        // Ctors
        public EditableNewArrayExpression()
        {
            Expressions = new EditableExpressionCollection();
        }

        public EditableNewArrayExpression(NewArrayExpression newEx)
            : this(new EditableExpressionCollection(newEx.Expressions), newEx.NodeType, newEx.Type)
        {
        }

        public EditableNewArrayExpression(IEnumerable<EditableExpression> expressions, ExpressionType nodeType, Type type)
            : this(new EditableExpressionCollection(expressions), nodeType, type)
        {
        }

        public EditableNewArrayExpression(EditableExpressionCollection expressions, ExpressionType nodeType, Type type)
            : base(type)
        {
            Expressions = expressions;
            NodeType = nodeType;
        }

        // Methods
        public override Expression ToExpression()
        {
            if (NodeType == ExpressionType.NewArrayBounds)
                return Expression.NewArrayBounds(Type.GetElementType(), Expressions.GetExpressions());
            else if (NodeType == ExpressionType.NewArrayInit)
                return Expression.NewArrayInit(Type.GetElementType(), Expressions.GetExpressions());
            else
                throw new InvalidOperationException("NodeType for NewArrayExpression must be ExpressionType.NewArrayInit or ExpressionType.NewArrayBounds");
        }
    }
}
