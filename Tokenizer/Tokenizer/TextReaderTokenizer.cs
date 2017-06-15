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
		int peek_1;

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
			peek_1 = textReader.Peek();
		}

		Token CreateToken(TokenType tokenType)
		{
			string content = tokenContentBuilder.ToString();
			tokenContentBuilder.Clear();
			int startPosition = index - content.Length;
			return new Token(tokenType, content, startPosition);
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

		bool PeekIsNewLine()
		{
			return peek == '\n' || (peek == '\r' && peek_1 == '\n');
		}

		bool PeekIsDigit()
		{
			return '0' <= peek && peek <= '9';
		}

		bool PeekIsLetter()
		{
			return ('a' <= peek && peek <= 'z') || ('A' <= peek && peek <= 'Z') || peek == 'ß';
		}

		bool PeekIsWhiteSpace()
		{
			return peek == ' ' || peek == '\t';
		}

		bool PeekIsEOF()
		{
			return peek == -1;
		}

		public IEnumerable<Token> Scan()
		{
			peek = textReader.Read();
			peek_1 = textReader.Peek();

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
				else if (peek == '@')
				{
					Consume();
					if (peek == '"')
					{
						yield return ScanVerbatimString();
					}
					else if (PeekIsLetter() || peek == '_')
					{
						yield return ScanIdentifier();
					}
					else
					{
						yield return ScanWord();
					}
				}
				else if (peek == '\n')
				{
					Consume();
					yield return CreateToken(TokenType.NewLine);
				}
				else if (peek == '\r' && peek_1 == '\n')
				{
					Consume();
					Consume();
					yield return CreateToken(TokenType.NewLine);
				}
				else if (PeekIsDigit())
				{
					yield return ScanInteger();
				}
				else if (peek == '\'')
				{
					yield return ScanCharacter();
				}
				else if (peek == '"')
				{
					yield return ScanString();
				}
				else if (peek == '#')
				{
					yield return ScanDirective();
				}
				else
				{
					yield return ScanWord();
				}
			}

			yield return CreateToken(TokenType.EndOfFile);
		}

		Token ScanMultiLineComment()
		{
			do
			{
				Consume();
			} while (!(peek == '*' && peek_1 == '/') && !PeekIsEOF());

			if (PeekIsEOF())
			{
				return ScanWord();
			}

			return CreateToken(TokenType.MultiLineComment);
		}

		Token ScanSingleLineComment()
		{
			do
			{
				Consume();
			} while (!PeekIsNewLine() && !PeekIsEOF());

			return CreateToken(TokenType.SingleLineComment);
		}

		Token ScanIdentifier()
		{
			do
			{
				Consume();
			} while (PeekIsLetter() || PeekIsDigit() || peek == '_');

			if (!PeekIsEOF() && !PeekIsNewLine() && !PeekIsWhiteSpace() && !PeekIsPunctuation())
			{
				return ScanWord();
			}

			return CreateToken(TokenType.Identifier);
		}

		Token ScanCharacter()
		{
			// TODO escape chars
			do
			{
				Consume();
			} while (peek != '\'' && !PeekIsNewLine() && !PeekIsEOF());

			if (peek == '\'')
			{
				Consume();
				return CreateToken(TokenType.Character);
			}

			return ScanWord();
		}

		Token ScanString()
		{
			// TODO escape chars 
			do
			{
				Consume();

				if (peek == '\\' && peek_1 == '\\')
				{
					Consume();
					Consume();
				}

				if (peek == '\\' && peek_1 == '\"')
				{
					Consume();
					Consume();
				}
			} while (peek != '"' && !PeekIsNewLine() && !PeekIsEOF());

			if (peek == '"')
			{
				Consume();
				return CreateToken(TokenType.String);
			}

			return ScanWord();
		}

		Token ScanVerbatimString()
		{
			do
			{
				Consume();
			} while (peek != '"' && !PeekIsNewLine() && !PeekIsEOF());

			if (peek == '"')
			{
				Consume();
				return CreateToken(TokenType.String);
			}

			return ScanWord();
		}

		Token ScanDirective()
		{
			do
			{
				Consume();
			} while (PeekIsLetter());

			return CreateToken(TokenType.Directive);
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

					return ScanWord();
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
					if (peek == '*')
					{
						return ScanMultiLineComment();
					}
					else if (peek == '/')
					{
						return ScanSingleLineComment();
					}
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
					Console.WriteLine("sadddsa");
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

		Token ScanWord()
		{
			while (!PeekIsEOF() && !PeekIsNewLine() && !PeekIsWhiteSpace() && !PeekIsPunctuation())
			{
				Consume();
			}

			return CreateToken(TokenType.Unknown);
		}
	}
}
