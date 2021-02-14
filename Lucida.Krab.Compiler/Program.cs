using Lucida.Krab.Parsing;
using System;
using System.Collections.Generic;

namespace Lucida.Krab.Compiler
{
    class Program
    {
        static void Main(string[] args)
        {
            var lines = new List<string>();
            string line = null;

            do
            {
                if (line != null) lines.Add(line);
                line = Console.ReadLine();
            } while (line.Length > 0);

            Console.WriteLine(Parser.Parse(new Characters.StringSource("stdio", string.Join(Environment.NewLine, lines))));
        }
    }
}
