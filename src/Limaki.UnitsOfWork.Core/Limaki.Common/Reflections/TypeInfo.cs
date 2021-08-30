using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Limaki.Common.Reflections {

    public class TypeInfo {

        public virtual Type Type { get; set; }

        public virtual TypeInfo BaseTypeInfo {
            get {
                if (Type.IsClass) {
                    return new TypeInfo { Type = Type.BaseType };
                }
                if (Type.IsInterface) {
                    var interfaze = Type.GetInterfaces ().First ();
                    return new TypeInfo { Type = interfaze };
                }
                return new TypeInfo { Type = typeof (object) };
            }
        }

        public virtual string Name => Type.Name;

        public string ClassName => GetClassName (Type);

        private static readonly Dictionary<Type, string> Aliases = new Dictionary<Type, string> () {
            { typeof(byte), "byte" },
            { typeof(sbyte), "sbyte" },
            { typeof(short), "short" },
            { typeof(ushort), "ushort" },
            { typeof(int), "int" },
            { typeof(uint), "uint" },
            { typeof(long), "long" },
            { typeof(ulong), "ulong" },
            { typeof(float), "float" },
            { typeof(double), "double" },
            { typeof(decimal), "decimal" },
            { typeof(object), "object" },
            { typeof(bool), "bool" },
            { typeof(char), "char" },
            { typeof(string), "string" },
            { typeof(void), "void" },
			//{ typeof(Nullable<byte>), "byte?" },
			//{ typeof(Nullable<sbyte>), "sbyte?" },
			//{ typeof(Nullable<short>), "short?" },
			//{ typeof(Nullable<ushort>), "ushort?" },
			//{ typeof(Nullable<int>), "int?" },
			//{ typeof(Nullable<uint>), "uint?" },
			//{ typeof(Nullable<long>), "long?" },
			//{ typeof(Nullable<ulong>), "ulong?" },
			//{ typeof(Nullable<float>), "float?" },
			//{ typeof(Nullable<double>), "double?" },
			//{ typeof(Nullable<decimal>), "decimal?" },
			//{ typeof(Nullable<bool>), "bool?" },
			//{ typeof(Nullable<char>), "char?" }
        };

        public virtual string GetClassName (Type type) {
            var name = type.Name;
            var niceName = name;
            if (Aliases.TryGetValue (type, out niceName)) {
                name = niceName;
            }

            if (type.IsNested && !type.IsGenericParameter) {
                name = type.FullName.Replace (type.DeclaringType.FullName + "+", GetClassName (type.DeclaringType));
            }

            if (type.IsGenericType) {

                var genPos = name.IndexOf ('`');
                if (genPos > 0)
                    name = name.Substring (0, genPos);
                else
                    name = name + "";
                bool isNullable = name == "Nullable";
                if (!isNullable)
                    name += "<";
                else
                    name = "";
                foreach (var item in type.GetGenericArguments ())
                    name += GetClassName (item) + ",";
                name = name.Remove (name.Length - 1, 1);
                if (isNullable)
                    name += "?";
                else
                    name += ">";
            }
            return name;
        }

        public virtual string ImplName => GetImplName (Type);

        public virtual string ParamName { get {
                var name = ImplName;
                name = $"{name.Substring (0, 1).ToLower()}{name.Substring (1)}";
                return name;
            } 
        }

        public string GetImplName (Type type) => (type.IsInterface && type.Name.StartsWith ("I")) ? type.Name.Remove (0, 1) : type.Name;

        public bool IsGenericIEnumerable => typeof (IEnumerable).IsAssignableFrom (Type) &&
                                          Type != typeof (string) && Type.IsGenericType;

        public IEnumerable<Type> GenericArgumentTypes => Type.IsGenericType ? Type.GetGenericArguments () : null;

        public IEnumerable<string> GenericArgumentNames => Type.IsGenericType ? Type.GetGenericArguments ().Select (p => p.Name) : null;

        public string GetPlural (string name) => name.EndsWith ("s") ? name + "es" : name + "s";

        public string DefaultMemberName {
            get {
                var type = Type;
                var name = type.Name;
                var isEnumerable = IsGenericIEnumerable;
                if (isEnumerable) {
                    type = GenericArgumentTypes.First ();
                    name = type.Name;
                }
                if (type.IsInterface) {
                    name = name.Remove (0, 1);
                }
                if (isEnumerable) {
                    name = GetPlural (name);
                }
                return name;
            }
        }

        public bool HierarchieMembers { get; set; }

        public IEnumerable<MemberInfo> MemberInfos {
            get {

                var flags = BindingFlags.Public | BindingFlags.Instance;

                if (!Type.IsInterface || !HierarchieMembers) {
                    if (HierarchieMembers) {
                        flags |= BindingFlags.FlattenHierarchy;
                    }
                    return Type.GetProperties (flags).Select (m => new MemberInfo { PropertyInfo = m });
                } else {
                    return (new Type[] { Type })
                           .Concat (Type.GetInterfaces ())
                           .SelectMany (i => i.GetProperties ())
                           .Select (m => new MemberInfo { PropertyInfo = m });
                }
            }
        }
    }
}
