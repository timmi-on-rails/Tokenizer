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
			int totalUnknown = 0;
			int numberOfFiles = 0;

			foreach (string fileName in Directory.EnumerateFiles(args[0], "*.cs", SearchOption.AllDirectories))
			{
				IEnumerable<Token> tokens = Tokenize.FromFile(fileName);

				IEnumerable<Token> uTokens = tokens.Where(token => token.TokenType == TokenType.Unknown).ToList();
				if (uTokens.Count() > 0)
				{
					Console.WriteLine("Found {0} unknown tokens in file {1}: {2},...", uTokens.Count(), fileName,
								  string.Join(",", uTokens.Take(5).Select(token => token.Content)));
				}
				totalUnknown += uTokens.Count();

				numberOfFiles++;
			}

			Console.WriteLine("Total unknown tokens: {0}, in {1} files", totalUnknown, numberOfFiles);
		}
	}
}
