using System;
using System.IO;
using Tokenizer;

namespace FileTokenizer
{
	class MainClass
	{
		public static void Main(string[] args)
		{
			if (args.Length == 0 || string.IsNullOrWhiteSpace(args[0]))
			{
				Console.WriteLine("usage: TokenizeFile <file name>");
				return;
			}

			string fileName = args[0];

			if (!File.Exists(fileName))
			{
				Console.WriteLine(fileName + ": no such file");
				return;
			}

			foreach (Token token in Tokenize.FromFile(fileName))
			{
				string escapedContent = token.Content
											 .Replace("\r\n", @"\r\n")
											 .Replace("\n", @"\n")
											 .Replace("\t", @"\t");

				Console.WriteLine("{0,7} {1,-17} {2}", token.Position, token.TokenType, escapedContent);
			}
		}
	}
}
