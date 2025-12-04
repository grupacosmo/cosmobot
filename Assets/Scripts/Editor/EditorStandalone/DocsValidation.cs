using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;
using System.Xml.Linq;
using System.Linq;

namespace Cosmobot.Editor.Standalone
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

    public class DocsValidation
    {
        private List<FunctionData> functions = new();
        private List<TypeData> types = new();
        private readonly static string path = Application.dataPath + "/Data/documentation.xml";
        private readonly XDocument document = XDocument.Load(path);

        [MenuItem("Cosmo/Validate JS Docs")]
        public static void PrintInfo()
        {
            DocsValidation loader = new();
            loader.LoadFunctions();
            loader.LoadTypes();

            foreach (var f in loader.functions)
            {
                Debug.Log("Function: " + f.name);

                foreach (var a in f.args)
                    Debug.Log($"  Arg: {a.name} ({a.type})");

                Debug.Log("  Returns: " + f.returns);
            }

            foreach (var t in loader.types)
            {
                Debug.Log("Type: " + t.name);

                foreach (var f in t.fields)
                    Debug.Log($"  Field: {f.name} ({f.type})");

                foreach (var m in t.methods)
                    Debug.Log($"  Method: {m.name} ({m.returns})");
            }
        }
        private void LoadFunctions()
        {
            XElement functionsRoot = document.Root.Element("functions");
            
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

                functions.Add(func);
            }
        }

        private void LoadTypes()
        {
            XElement typesRoot = document.Root.Element("types");

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

                types.Add(type);
            }
        }
    }
}