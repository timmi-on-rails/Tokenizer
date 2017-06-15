using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

			if (args.Length == 2 && args[0] == "-d")
			{
				TokenizeDirectory(args[1]);
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

		static void TokenizeDirectory(string directory)
		{
			int unknownTokens = 0;
			string[] files = Directory.GetFiles(directory, "*.cs", SearchOption.AllDirectories);

			foreach (string fileName in files)
			{
				List<Token> tokens = Tokenize.FromFile(fileName).Where(token => token.TokenType == TokenType.SingleLineComment).ToList();
				unknownTokens += tokens.Count;

				if (tokens.Count > 0)
				{
					Console.ForegroundColor = ConsoleColor.DarkMagenta;
					Console.WriteLine("\n" + fileName + ":");
					Console.ResetColor();
					Console.WriteLine(string.Join("\n", tokens.Select(token => token.Content)));
				}
			}

			Console.WriteLine("Found {0} unknown tokens in {1} files.", unknownTokens, files.Length);
		}
	}
}
