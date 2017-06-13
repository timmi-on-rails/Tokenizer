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
			return new TokenEnumerableFromFile(fileName);
		}

		public static IEnumerable<Token> FromString(string content)
		{
			return new TokenEnumerableFromString(content);
		}

		public static IEnumerable<Token> FromTextReader(TextReader textReader)
		{
			return new TokenEnumerableFromTextReader(textReader);
		}

		class TokenEnumerableFromFile : IEnumerable<Token>
		{
			readonly string fileName;
			bool alreadyIterated;

			public TokenEnumerableFromFile(string fileName)
			{
				this.fileName = fileName;
			}

			public IEnumerator<Token> GetEnumerator()
			{
				if (alreadyIterated)
				{
					throw new InvalidOperationException("It is not allowed to iterate more than once.");
				}

				alreadyIterated = true;

				using (TextReader textReader = File.OpenText(fileName))
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

		class TokenEnumerableFromTextReader : IEnumerable<Token>
		{
			readonly TextReader textReader;
			bool alreadyIterated;

			public TokenEnumerableFromTextReader(TextReader textReader)
			{
				this.textReader = textReader;
			}

			public IEnumerator<Token> GetEnumerator()
			{
				if (alreadyIterated)
				{
					throw new InvalidOperationException("It is not allowed to iterate more than once.");
				}

				alreadyIterated = true;

				TextReaderTokenizer tokenizer = new TextReaderTokenizer(textReader);
				return tokenizer.Scan().GetEnumerator();
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return GetEnumerator();
			}
		}
	}
}
