//
// MonoTODOAttribute.cs
//
// Authors:
//   Ravi Pratap (ravi@ximian.com)
//   Eyal Alaluf <eyala@mainsoft.com> 
//
// (C) Ximian, Inc.  http://www.ximian.com
//

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
// Copyright (C) 2006 Mainsoft, Inc (http://www.mainsoft.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

namespace System {
	// this is MonoTODOAttribute renamed


	[AttributeUsage (AttributeTargets.All, AllowMultiple=true)]
	public class TODOAttribute : Attribute {

		string comment;
		
		public TODOAttribute ()
		{
		}

		public TODOAttribute (string comment)
		{
			this.comment = comment;
		}

		public virtual string Comment {
			get { return comment; }
		}
	}

    [AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
    public class DONEAttribute : TODOAttribute {}

	[AttributeUsage (AttributeTargets.All, AllowMultiple=true)]
	internal class MonoDocumentationNoteAttribute : TODOAttribute {

		public MonoDocumentationNoteAttribute (string comment)
			: base (comment)
		{
		}

		public override string Comment {
			get { return base.Comment; }
		}
	}

	[AttributeUsage (AttributeTargets.All, AllowMultiple=true)]
	internal class MonoExtensionAttribute : TODOAttribute {

		public MonoExtensionAttribute (string comment)
			: base (comment)
		{
		}

		public override string Comment {
			get { return base.Comment; }
		}
	}

	[AttributeUsage (AttributeTargets.All, AllowMultiple=true)]
	internal class MonoInternalNoteAttribute : TODOAttribute {

		public MonoInternalNoteAttribute (string comment)
			: base (comment)
		{
		}

		public override string Comment {
			get { return base.Comment; }
		}
	}

	[AttributeUsage (AttributeTargets.All, AllowMultiple=true)]
	internal class MonoLimitationAttribute : TODOAttribute {

		public MonoLimitationAttribute (string comment)
			: base (comment)
		{
		}

		public override string Comment {
			get { return base.Comment; }
		}
	}

	[AttributeUsage (AttributeTargets.All, AllowMultiple=true)]
	internal class MonoNotSupportedAttribute : TODOAttribute {

		public MonoNotSupportedAttribute (string comment)
			: base (comment)
		{
		}

		public override string Comment {
			get { return base.Comment; }
		}
	}
}
