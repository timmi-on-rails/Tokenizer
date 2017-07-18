namespace Tokenizer
{
	public class Token
	{
		// public Object Value { get; }

		public TokenType TokenType { get; }

		public string Content { get; }

		public int Position { get; }

		public ErrorCode ErrorCode { get; }

		public Token(TokenType tokenType, string content, int position, ErrorCode errorCode)
		{
			TokenType = tokenType;
			Content = content;
			Position = position;
			ErrorCode = errorCode;
		}
	}
}
