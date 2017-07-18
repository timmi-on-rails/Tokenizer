using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace Tokenizer
{
	class TextReaderTokenizer
	{
		readonly StringBuilder tokenContentBuilder = new StringBuilder();
		readonly TextReader textReader;
		int index;
		int peek;

		public TextReaderTokenizer(TextReader textReader)
		{
			this.textReader = textReader;
		}

		void Consume()
		{
			Debug.Assert(peek != -1);

			tokenContentBuilder.Append((char)peek);
			index++;
			peek = textReader.Read();
		}

		Token CreateToken(TokenType tokenType, ErrorCode errorCode = ErrorCode.Ok)
		{
			string content = tokenContentBuilder.ToString();
			tokenContentBuilder.Clear();
			int startPosition = index - content.Length;
			return new Token(tokenType, content, startPosition, errorCode);
		}

		bool PeekIsPunctuation()
		{
			return
				peek == '<' || peek == '>' ||
				peek == '(' || peek == ')' ||
				peek == '!' || peek == '^' ||
				peek == '*' || peek == '+' ||
				peek == '-' || peek == '=' ||
				peek == '/' || peek == '%' ||
				peek == ',' || peek == '?' ||
				peek == ':' || peek == ';' ||
				peek == '.' || peek == '{' ||
				peek == '}' || peek == '[' ||
				peek == ']' || peek == '&' ||
				peek == '|' || peek == '~';
		}

		bool PeekIsDigit()
		{
			return '0' <= peek && peek <= '9';
		}

		bool PeekIsLetter()
		{
			return ('a' <= peek && peek <= 'z') || ('A' <= peek && peek <= 'Z');
		}

		bool PeekIsWhiteSpace()
		{
			return peek == ' ' || peek == '\t';
		}

		bool PeekIsEOF()
		{
			return peek == -1;
		}

		bool PeekIsWordSeparator()
		{
			return PeekIsWhiteSpace() || PeekIsPunctuation() || PeekIsEOF();
		}

		public IEnumerable<Token> Scan()
		{
			peek = textReader.Read();

			while (peek != -1)
			{
				if (PeekIsLetter() || peek == '_')
				{
					yield return ScanIdentifier();
				}
				else if (PeekIsWhiteSpace())
				{
					yield return ScanWhiteSpace();
				}
				else if (peek == '.')
				{
					yield return ScanDecimal();
				}
				else if (PeekIsPunctuation())
				{
					yield return ScanPunctuation();
				}
				else if (PeekIsDigit())
				{
					yield return ScanInteger();
				}
				else
				{
					yield return ScanWord(ErrorCode.Unknown);
				}
			}

			yield return CreateToken(TokenType.EndOfFile);
		}

		Token ScanIdentifier()
		{
			do
			{
				Consume();
			} while (PeekIsLetter() || PeekIsDigit() || peek == '_');

			if (!PeekIsWordSeparator())
			{
				return ScanWord(ErrorCode.NotAllowedIdentifierCharacters);
			}

			return CreateToken(TokenType.Identifier);
		}

		Token ScanInteger()
		{
			do
			{
				Consume();
			} while (PeekIsDigit());

			if (peek == '.' || peek == 'e')
			{
				return ScanDecimal();
			}

			return CreateToken(TokenType.Integer);
		}

		Token ScanDecimal()
		{
			while (PeekIsDigit())
			{
				Consume();
			}

			if (peek == '.')
			{
				Consume();

				bool anyDigits = false;

				while (PeekIsDigit())
				{
					Consume();
					anyDigits = true;
				}

				if (!anyDigits)
				{
					if (tokenContentBuilder.Length == 1)
					{
						return CreateToken(TokenType.Dot);
					}

					return ScanWord(ErrorCode.ExpectedDecimal);
				}
			}

			if (peek == 'e')
			{
				Consume();

				if (peek == '+' || peek == '-')
				{
					Consume();
				}

				while (PeekIsDigit())
				{
					Consume();
				}
			}

			return CreateToken(TokenType.Decimal);
		}

		Token ScanPunctuation()
		{
			switch (peek)
			{
				case ':':
					Consume();
					return CreateToken(TokenType.Colon);
				case '(':
					Consume();
					return CreateToken(TokenType.LeftParenthesis);
				case ')':
					Consume();
					return CreateToken(TokenType.RightParenthesis);
				case '>':
					Consume();
					if (peek == '=')
					{
						Consume();
						return CreateToken(TokenType.GreaterOrEqual);
					}
					return CreateToken(TokenType.Greater);
				case '<':
					Consume();
					if (peek == '=')
					{
						Consume();
						return CreateToken(TokenType.LessOrEqual);
					}
					if (peek == '>')
					{
						Consume();
						return CreateToken(TokenType.NotEqual);
					}
					return CreateToken(TokenType.Less);
				case '~':
					Consume();
					return CreateToken(TokenType.Tilde);
				case '+':
					Consume();
					return CreateToken(TokenType.Plus);
				case '-':
					Consume();
					return CreateToken(TokenType.Minus);
				case '=':
					Consume();
					if (peek == '=')
					{
						Consume();
						return CreateToken(TokenType.Equal);
					}
					return CreateToken(TokenType.Assignment);
				case '!':
					Consume();
					if (peek == '=')
					{
						Consume();
						return CreateToken(TokenType.NotEqual);
					}
					return CreateToken(TokenType.Exclamation);
				case '*':
					Consume();
					return CreateToken(TokenType.Star);
				case '/':
					Consume();
					return CreateToken(TokenType.Slash);
				case '%':
					Consume();
					return CreateToken(TokenType.Percent);
				case ',':
					Consume();
					return CreateToken(TokenType.Comma);
				case '^':
					Consume();
					return CreateToken(TokenType.Pow);
				case '?':
					Consume();
					return CreateToken(TokenType.QuestionMark);
				case ';':
					Consume();
					return CreateToken(TokenType.Semicolon);
				case '.':
					Consume();
					return CreateToken(TokenType.Dot);
				case '{':
					Consume();
					return CreateToken(TokenType.CurlyLeft);
				case '}':
					Consume();
					return CreateToken(TokenType.CurlyRight);
				case '[':
					Consume();
					return CreateToken(TokenType.BracketLeft);
				case ']':
					Consume();
					return CreateToken(TokenType.BracketRight);
				case '&':
					Consume();
					if (peek == '&')
					{
						Consume();
						return CreateToken(TokenType.AndDouble);
					}
					return CreateToken(TokenType.AndSingle);
				case '|':
					Consume();
					if (peek == '|')
					{
						Consume();
						return CreateToken(TokenType.PipeDouble);
					}
					return CreateToken(TokenType.PipeSingle);
				default:
					throw new ArgumentException("bug, punctuation expected");
			}
		}

		Token ScanWhiteSpace()
		{
			do
			{
				Consume();
			} while (PeekIsWhiteSpace());

			return CreateToken(TokenType.WhiteSpace);
		}

		Token ScanWord(ErrorCode errorCode)
		{
			while (!PeekIsWordSeparator())
			{
				Consume();
			}

			return CreateToken(TokenType.Unknown, errorCode);
		}
	}
}
