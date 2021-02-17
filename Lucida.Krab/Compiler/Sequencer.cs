using Lucida.Krab.Characters;
using Lucida.Krab.CodeDocument;
using System.Collections.Generic;
using System.Linq;

namespace Lucida.Krab.Compiler
{
    public class Sequencer
    {
        public delegate string Resolver(string parent, string name);

        public Dictionary<string, Member> Members
        {
            get
            {
                var result = new Dictionary<string, Member>();

                foreach (var pair in Structures) result.Add(pair.Key, pair.Value);
                foreach (var pair in Methods) result.Add(pair.Key, pair.Value);
                foreach (var pair in Fields) result.Add(pair.Key, pair.Value);
                foreach (var pair in Statements) result.Add(pair.Key, pair.Value);

                return result;
            }
        }

        public readonly Dictionary<string, Structure> Structures = new Dictionary<string, Structure>();
        public readonly Dictionary<string, Field> Fields = new Dictionary<string, Field>();
        public readonly Dictionary<string, Method> Methods = new Dictionary<string, Method>();
        public readonly Dictionary<string, Statement> Statements = new Dictionary<string, Statement>();

        public Sequencer(Document document)
        {
            CrawlNamespace("global", document);
        }

        public void CrawlNamespace(string path, IEnumerable<Member> members)
        {
            ulong statementIndex = 0;

            foreach (var member in members)
            {
                var name = $"{path}.{member.Name}";

                if (member is Field field)
                {
                    Fields.Add(name, field);
                }
                else if (member is Statement statement)
                {
                    Statements.Add($"{path}.{statementIndex}", statement);
                    statementIndex++;
                }
                else if (member is Method method)
                {
                    Methods.Add(name, method);
                    CrawlNamespace(name, method.Parameters);
                    CrawlNamespace(name, method.Body);
                }
                else if (member is Structure structure)
                {
                    Structures.Add(name, structure);
                    CrawlNamespace(name, structure.Members);
                }
                else if (member is Namespace ns)
                {
                    CrawlNamespace(name, ns);
                }
            }
        }

        public void Resolve()
        {
            var members = Members;

            Resolve((parent, name) =>
            {
                var path = parent.Split('.').ToList();

                while (path.Count > 0)
                {
                    var key = $"{string.Join(".", path)}.{name}";

                    if (members.ContainsKey(key))
                    {
                        return key;
                    }

                    path.RemoveAt(path.Count - 1);
                }

                throw new KeyNotFoundException();
            }, members);
        }

        public void Resolve(Resolver resolver)
        {
            Resolve(resolver, Members);
        }

        private static void Resolve(Resolver resolver, IEnumerable<KeyValuePair<string, Member>> members)
        {
            foreach (var member in members)
            {
                var parent = GetParent(member.Key);

                if (member.Value is Field field)
                {
                    field.Type = ResolveSource(resolver, parent, field.Type);
                }
                else if (member.Value is Method method)
                {
                    method.ReturnType = ResolveSource(resolver, parent, method.ReturnType);

                    foreach (var param in method.Parameters)
                    {
                        param.Type = ResolveSource(resolver, parent, param.Type);
                    }
                }
                else if (member.Value is Statement statement)
                {
                    ResolveExpression(resolver, parent, statement.Expression);
                }
            }
        }

        private static void ResolveExpression(Resolver resolver, string parent, Expression expression)
        {
            if (expression is CallExpression call)
            {
                call.Call = ResolveSource(resolver, parent, call.Call);

                foreach (var arg in call.Arguments)
                {
                    ResolveExpression(resolver, parent, arg);
                }
            }
            else if (expression is ReferenceExpression reference)
            {
                reference.Reference = ResolveSource(resolver, parent, reference.Reference);
            }
        }

        private static ICharacterSource ResolveSource(Resolver resolver, string parent, ICharacterSource source)
        {
            try
            {
                return new StringSource("&<resolved>", resolver(parent, source.Build()));
            }
            catch (KeyNotFoundException)
            {
                throw new SourceError(source, "Name could not be resolved.");
            }
        }

        private static string GetParent(string name)
        {
            var path = name.Split('.');

            return string.Join(".", path.Take(path.Length - 1));
        }
    }
}
