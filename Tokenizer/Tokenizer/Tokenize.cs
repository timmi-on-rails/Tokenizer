using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace Tokenizer
{
	public static class Tokenize
	{
		public static IEnumerable<Token> FromFile(string fileName)
		{
			using (TextReader stringReader = File.OpenText(fileName))
			{
				foreach (Token token in FromTextReader(stringReader))
				{
					yield return token;
				}
			}
		}

		public static IEnumerable<Token> FromString(string content)
		{
			using (StringReader stringReader = new StringReader(content))
			{
				foreach (Token token in FromTextReader(stringReader))
				{
					yield return token;
				}
			}
		}

		public static IEnumerable<Token> FromTextReader(TextReader textReader)
		{
			return new TokenEnumerable(textReader);
		}

		class TokenEnumerable : IEnumerable<Token>
		{
			readonly TextReader textReader;
			bool used;

			public TokenEnumerable(TextReader textReader)
			{
				this.textReader = textReader;
			}

			public IEnumerator<Token> GetEnumerator()
			{
				if (used)
					throw new InvalidOperationException();

				used = true;
				TextReaderTokenizer tokenizer = new TextReaderTokenizer(textReader);

				foreach (Token token in tokenizer.Scan())
				{
					yield return token;
				}
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return GetEnumerator();
			}
		}
	}
}
