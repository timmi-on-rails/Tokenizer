using System;
using Tokenizer;

namespace TokenizerDemo
{
	class MainClass
	{
		public static void Main()
		{
			Console.Write("Enter string to tokenize: ");
			string input = Console.ReadLine();

			foreach (Token token in Tokenize.FromString(input))
			{
				string escapedContent = token.Content
											 .Replace(" ", @"\ ")
											 .Replace("\t", @"\t");

				Console.WriteLine("{0,4} {1,-20} {2,-40} {3}", token.Position, token.TokenType, token.ErrorCode, escapedContent);
			}
		}
	}
}
