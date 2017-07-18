using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace Tokenizer
{
	public static class Tokenize
	{
		public static IEnumerable<Token> FromString(string content)
		{
			return new TokenEnumerableFromString(content);
		}

		class TokenEnumerableFromString : IEnumerable<Token>
		{
			readonly string content;
			bool alreadyIterated;

			public TokenEnumerableFromString(string content)
			{
				this.content = content;
			}

			public IEnumerator<Token> GetEnumerator()
			{
				if (alreadyIterated)
				{
					throw new InvalidOperationException("It is not allowed to iterate more than once.");
				}

				alreadyIterated = true;

				using (TextReader textReader = new StringReader(content))
				{
					TextReaderTokenizer tokenizer = new TextReaderTokenizer(textReader);

					foreach (Token token in tokenizer.Scan())
					{
						yield return token;
					}
				}
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return GetEnumerator();
			}
		}
	}
}
