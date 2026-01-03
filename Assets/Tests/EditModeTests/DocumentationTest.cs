using System;
using System.IO;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;
using System.Xml.Linq;
using System.Linq;
using System.Reflection;

namespace Cosmobot
{
    public class ArgData
    {
        public string name;
        public string type;
    }

    public class FunctionData
    {
        public string name;
        public List<ArgData> args = new();
        public string returns;
    }

    public class FieldData
    {
        public string type;
        public string name;
    }

    public class MethodData
    {
        public string name;
        public string returns;
    }

    public class TypeData
    {
        public string name;
        public List<FieldData> fields = new();
        public List<MethodData> methods = new();
    }

    public class DocumentationTest
    {
        private List<FunctionData> functions;
        private List<TypeData> types;
        private GameObject testObject;
        private XDocument document = XDocument.Parse(File.ReadAllText(Application.dataPath + "/Data/documentation.xml"));
        private static readonly string[] ApiNamespaces = {"Cosmobot.Api.Types", "Cosmobot.Api.TypesInternal"};

        [SetUp]
        public void SetUp()
        {
            functions = LoadFunctions();
            types = LoadTypes();
            testObject = new GameObject("TestObject");
        }

        [TearDown]
        public void TearDown()
        {
            UnityEngine.Object.DestroyImmediate(testObject);
        }

        [Test]
        public void ValidateFunctions()
        {
            List<FunctionData> implementations = GetImplementedFunctions();
            var matchedPairs = from impl in implementations
                                join doc in functions
                                on impl.name equals doc.name
                                select new { Implementation = impl, Documented = doc };

            var undocumented = implementations
                .Where(impl => !functions.Any(doc => doc.name == impl.name))
                .ToList();

            var unimplemented = functions
                .Where(impl => !implementations.Any(doc => doc.name == impl.name))
                .ToList();
            
            foreach (var unimpl in unimplemented)
            {
                Assert.True(false, $"Function {unimpl.name} is documented but not implemented.");
            }

            foreach (var impl in undocumented)
            {
                Assert.True(false, $"Function {impl.name} is implemented but not documented.");
            }

            foreach (var pair in matchedPairs)
            {
                string implReturn = NormalizeReturnType(pair.Implementation.returns);
                Assert.IsTrue(ReturnMatches(implReturn, pair.Documented.returns), 
                $"Argument types of {pair.Documented.name} do not match. Implementation: {implReturn}, Documentation: {pair.Documented.returns}");

                List<ArgData> implArgs = pair.Implementation.args;
                List<ArgData> docArgs = pair.Documented.args;

                Assert.That(implArgs.Count, Is.EqualTo(docArgs.Count), 
                $"Argument count mismatch for {pair.Documented.name}. Implementation: {implArgs.Count}, Documentation: {docArgs.Count}");


                for (int i = 0; i < implArgs.Count; i++)
                {
                    string argType = NormalizeReturnType(implArgs[i].type);

                    Assert.IsTrue(ReturnMatches(argType, docArgs[i].type), 
                    $"Argument types of {pair.Documented.name} do not match. Implementation: {argType}, Documentation: {docArgs[i].type}");
                }
            }
        }

        [Test]
        public void ValidateTypes()
        {
            List<TypeData> implementations = GetImplementedTypes();
            var matchedPairs = from impl in implementations
                            join doc in types
                            on impl.name equals doc.name
                            select new { Implementation = impl, Documented = doc };

            var undocumented = implementations
                .Where(impl => !types.Any(doc => doc.name == impl.name))
                .ToList();

            var unimplemented = types
                .Where(doc => !implementations.Any(impl => impl.name == doc.name))
                .ToList();

            foreach (var unimpl in unimplemented)
            {
                Assert.True(false, $"Type {unimpl.name} is documented but not implemented.");
            }

            foreach (var impl in undocumented)
            {
                Assert.True(false, $"Type {impl.name} is implemented but not documented.");
            }

            foreach (var pair in matchedPairs)
            {
                var implFields = pair.Implementation.fields;
                var docFields = pair.Documented.fields;

                for (int i = 0; i < implFields.Count; i++)
                {
                    string implType = NormalizeReturnType(implFields[i].type);
                    string docType = pair.Documented.fields[i].type;

                    Assert.IsTrue(ReturnMatches(implType, docType),
                    $"Field type mismatch in type {pair.Documented.name} for field {implFields[i].name}. Implementation: {implType}, Documentation: {docType}");
                }
            }
        }

        private List<TypeData> GetImplementedTypes()
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            List<TypeData> result = new();

            var targetTypes = assemblies
                .SelectMany(a =>
                {
                    try { return a.GetTypes(); }
                    catch (ReflectionTypeLoadException e) { return e.Types.Where(t => t != null); }
                })
                .Where(t =>
                    t.IsClass &&
                    !t.IsAbstract &&
                    t.Namespace != null &&
                    ApiNamespaces.Contains(t.Namespace))
                .Where(t => !t.Name.StartsWith("<"))
                .OrderBy(t => t.FullName)
                .ToList();

            foreach (var type in targetTypes)
            {
                TypeData typeData = new()
                {
                    name = type.Name,
                    fields = type.GetFields(
                        BindingFlags.Instance |
                        BindingFlags.Public |
                        BindingFlags.NonPublic |
                        BindingFlags.DeclaredOnly)
                    .Where(f => f.DeclaringType == type)
                    .Where(f => !f.IsAssembly)
                    .Select(f => new FieldData
                    {
                        name = f.Name,
                        type = GetTypeName(f.FieldType) 
                    })
                    .ToList(),
                    methods = type.GetMethods(
                        BindingFlags.Instance |
                        BindingFlags.Public |
                        BindingFlags.NonPublic |
                        BindingFlags.DeclaredOnly)
                    .Where(m => m.DeclaringType == type)
                    .Where(m => !m.IsSpecialName)
                    .Where(m => !m.Name.StartsWith("<"))
                    .Select(m => new MethodData
                    {
                        name = m.Name,
                        returns = GetTypeName(m.ReturnType)
                    })
                    .ToList()
                };

                result.Add(typeData);
            }

            return result;
        }

        private List<FunctionData> GetImplementedFunctions()
        {
            var interfaceType = typeof(IEngineLogic);
            var runtimeAssembly = interfaceType.Assembly;
            List<FunctionData> allImplementedFunctions = new();

            var allValidImplementations = runtimeAssembly
            .GetTypes()
            .Where(t =>
                interfaceType.IsAssignableFrom(t) &&
                !t.IsInterface &&
                !t.IsAbstract)
            .ToArray();

            foreach (var implementation in allValidImplementations)
            {
                IEngineLogic instance = (IEngineLogic)testObject.AddComponent(implementation);
                instance.SetupThread(
                    new ManualResetEvent(false),
                    new CancellationToken(),
                    new ConcurrentQueue<Action>()
                );
                
                var functions = instance.GetFunctions();

                foreach (var function in functions)
                {
                    FunctionData data = new();
                    var methodInfo = function.Value.Method;
                    
                    data.name = function.Key;
                    data.returns = methodInfo.ReturnType == typeof(void) ? "null" : GetReturnTypeName(methodInfo.ReturnType);
                    
                    foreach (var parameter in methodInfo.GetParameters())
                    {
                        data.args.Add(new ArgData
                        {
                            type = GetTypeName(parameter.ParameterType)
                        });
                    }

                    allImplementedFunctions.Add(data);
                }
            }

            return allImplementedFunctions;
        }

        private List<FunctionData> LoadFunctions()
        {
            XElement functionsRoot = document.Root.Element("functions");
            List<FunctionData> allFunctions = new();

            foreach (var f in functionsRoot.Elements("function"))
            {
                FunctionData func = new()
                {
                    name = f.Attribute("name")?.Value,

                    args = f.Element("args")
                                .Elements("arg")
                                .Select(arg => new ArgData
                                {
                                    type = arg.Element("type")?.Value,
                                })
                                .ToList(),

                    returns = f.Element("returns").Element("type")?.Value
                };

                allFunctions.Add(func);
            }

            return allFunctions;
        }

        private List<TypeData> LoadTypes()
        {
            XElement typesRoot = document.Root.Element("types");
            List<TypeData> allTypes = new();

            foreach (var t in typesRoot.Elements("type"))
            {
                TypeData type = new()
                {
                    name = t.Attribute("name")?.Value,

                    fields = t.Element("fields")
                                .Elements("field")
                                .Select(field => new FieldData
                                {
                                    name = field.Element("name")?.Value,
                                    type = field.Element("type")?.Value,
                                })
                                .ToList(),

                    methods = t.Element("methods")
                                .Elements("method")
                                .Select(method => new MethodData
                                {
                                    name = method.Element("name")?.Value,
                                    returns = method.Element("returns")?.Value,
                                })
                                .ToList()
                };

                allTypes.Add(type);
            }

            return allTypes;
        }

        private static string GetTypeName(Type type)
        {
            if (type.IsByRef)
                type = type.GetElementType();

            if (type.IsGenericType)
                return $"{type.Name[..type.Name.IndexOf('`')]}<{string.Join(", ", type.GetGenericArguments().Select(GetTypeName))}>";

            return Type.GetTypeCode(type) switch
            {
                TypeCode.Boolean => "bool",
                TypeCode.Byte    => "byte",
                TypeCode.SByte   => "sbyte",
                TypeCode.Int16   => "short",
                TypeCode.UInt16  => "ushort",
                TypeCode.Int32   => "int",
                TypeCode.UInt32  => "uint",
                TypeCode.Int64   => "long",
                TypeCode.UInt64  => "ulong",
                TypeCode.Single  => "float",
                TypeCode.Double  => "double",
                TypeCode.Decimal => "decimal",
                TypeCode.String  => "string",
                _ => type.Name
            };
        }

        private static string GetReturnTypeName(Type type)
        {
            if (type.IsArray)
                return $"Array({GetReturnTypeName(type.GetElementType())})";

            if (type.IsGenericType)
            {
                var typeName = type.Name;
                var backtickIndex = typeName.IndexOf('`');
                if (backtickIndex > 0)
                    typeName = typeName.Substring(0, backtickIndex);

                var genericArgs = type.GetGenericArguments()
                                    .Select(GetReturnTypeName);

                return $"{typeName}({string.Join(", ", genericArgs)})";
            }

            return type.Name;
        }

        private static string NormalizeReturnType(string implReturn)
        {
            if (implReturn.StartsWith("List(") && implReturn.EndsWith(")"))
            {
                string innerType = implReturn.Substring(5, implReturn.Length - 6);
                return $"Array({innerType})";
            }

            return implReturn;
        }

        private static bool ReturnMatches(string implReturn, string docReturn)
        {
            // match returns that contain |null, since there is no information of possible null return in function signatures
            return docReturn == implReturn || (docReturn.EndsWith("|null") && docReturn.Substring(0, docReturn.Length - 5).Trim() == implReturn);
        }
    }
}