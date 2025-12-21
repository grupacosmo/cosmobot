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
        public void test()
        {
            List<MethodData> implementations = GetImplementedFunctions();
            string[] functionNames = functions
                .Select(f => f.name)
                .ToArray();

            foreach (var function in implementations)
            {
                Assert.Contains(function.name, functionNames, $"Function {function.name} is not documented.");
            }
        }

        private List<MethodData> GetImplementedFunctions()
        {
            var interfaceType = typeof(IEngineLogic);
            var runtimeAssembly = interfaceType.Assembly;
            List<MethodData> allImplementedFunctions = new();

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
                    MethodData data = new();
                    data.name = function.Key;

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
                                    name = arg.Element("name")?.Value,
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
    }
}