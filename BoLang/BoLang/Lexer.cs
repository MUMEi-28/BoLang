using System;
using System.Collections.Generic;

namespace BoLang
{
	public enum TokenType
	{
		// Literal Types
		Number,
		Identifier,

		// Keywords
		Let,
		Const,
		Fn,

		// Grouping & Operators
		BinaryOperator,
		Equals,
		Comma,
		Dot,
		Colon,
		Semicolon,
		OpenParen,
		CloseParen,
		OpenBrace,
		CloseBrace,
		OpenBracket,
		CloseBracket,
		EOF
	}

	public class Token
	{
		public string Value { get; set; }
		public TokenType Type { get; set; }

		public Token(string value, TokenType type)
		{
			Value = value;
			Type = type;
		}
	}

	public static class Lexer
	{
		private static readonly Dictionary<string, TokenType> Keywords = new()
		{
			{ "let", TokenType.Let },
			{ "const", TokenType.Const },
			{ "fn", TokenType.Fn }
		};

		public static List<Token> Tokenize(string sourceCode)
		{
			var tokens = new List<Token>();
			var src = new Queue<char>(sourceCode);

			while (src.Count > 0)
			{
				char current = src.Peek();

				switch (current)
				{
					case '(':
						tokens.Add(NewToken(src.Dequeue().ToString(), TokenType.OpenParen));
						break;
					case ')':
						tokens.Add(NewToken(src.Dequeue().ToString(), TokenType.CloseParen));
						break;
					case '{':
						tokens.Add(NewToken(src.Dequeue().ToString(), TokenType.OpenBrace));
						break;
					case '}':
						tokens.Add(NewToken(src.Dequeue().ToString(), TokenType.CloseBrace));
						break;
					case '[':
						tokens.Add(NewToken(src.Dequeue().ToString(), TokenType.OpenBracket));
						break;
					case ']':
						tokens.Add(NewToken(src.Dequeue().ToString(), TokenType.CloseBracket));
						break;
					case '+':
					case '-':
					case '*':
					case '/':
					case '%':
						tokens.Add(NewToken(src.Dequeue().ToString(), TokenType.BinaryOperator));
						break;
					case '=':
						tokens.Add(NewToken(src.Dequeue().ToString(), TokenType.Equals));
						break;
					case ';':
						tokens.Add(NewToken(src.Dequeue().ToString(), TokenType.Semicolon));
						break;
					case ':':
						tokens.Add(NewToken(src.Dequeue().ToString(), TokenType.Colon));
						break;
					case ',':
						tokens.Add(NewToken(src.Dequeue().ToString(), TokenType.Comma));
						break;
					case '.':
						tokens.Add(NewToken(src.Dequeue().ToString(), TokenType.Dot));
						break;
					default:
						if (IsDigit(current))
						{
							string number = "";
							while (src.Count > 0 && IsDigit(src.Peek()))
							{
								number += src.Dequeue();
							}
							tokens.Add(NewToken(number, TokenType.Number));
						}
						else if (IsAlpha(current))
						{
							string ident = "";
							while (src.Count > 0 && IsAlpha(src.Peek()))
							{
								ident += src.Dequeue();
							}

							if (Keywords.TryGetValue(ident, out TokenType type))
							{
								tokens.Add(NewToken(ident, type));
							}
							else
							{
								tokens.Add(NewToken(ident, TokenType.Identifier));
							}
						}
						else if (IsSkippable(current))
						{
							src.Dequeue();
						}
						else
						{
							Console.Error.WriteLine($"Unrecognized character in source: {(int)current} '{current}'");
							Environment.Exit(1);
						}

						break;
				}
			}

			tokens.Add(new Token("EndOfFile", TokenType.EOF));
			return tokens;
		}

		private static Token NewToken(string value, TokenType type)
		{
			return new Token(value, type);
		}

		private static bool IsAlpha(char c)
		{
			return char.IsLetter(c);
		}

		private static bool IsDigit(char c)
		{
			return char.IsDigit(c);
		}

		private static bool IsSkippable(char c)
		{
			return c == ' ' || c == '\n' || c == '\t' || c == '\r';
		}
	}
}
