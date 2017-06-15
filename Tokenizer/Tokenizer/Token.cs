namespace Tokenizer
{
	public class Token
	{
		// public Object Value { get; }

		// public ErrorCode ErrorCode { get; }

		public TokenType TokenType { get; }

		public string Content { get; }

		public int Position { get; }

		public Token(TokenType tokenType, string content, int position)
		{
			TokenType = tokenType;
			Content = content;
			Position = position;
		}
	}
}
