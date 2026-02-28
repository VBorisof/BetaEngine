using BDSM.Language;
using BDSM.Logging;
using BDSM.Tokens;
using System.Collections.Generic;
using System.Globalization;

namespace BDSM.Scanning;

public class Scanner
{
    private readonly IBDSMLogger _logger;

    private string _source = "";
    private string _currentLine = "";

    private int _start;
    private int _lineNumber;
    private int _totalLines;
    private int _columnIndex;

    private bool _isInMultilineComment;

    public bool IsSuccess { get; private set; }

    public Scanner(IBDSMLogger logger)
    {
        _logger = logger;
    }


    public List<Token> ScanTokens(string source)
    {
        IsSuccess = true;

        _start = 0;
        _lineNumber = 0;
        _columnIndex = 0;
        _currentLine = "";

        var tokens = new List<Token>();
        _source = source;
        var lines = source.Split("\n");
        _totalLines = lines.Length;

        _logger.Debug($"Scanning {_totalLines} lines.");

        // Go line by line.
        foreach (var line in lines)
        {
            _lineNumber++;
            _columnIndex = 0;
            _currentLine = line;

            _logger.Debug($"Scan line {_lineNumber}: `{_currentLine}`.");

            // Scan each character in the line.
            while (_columnIndex < _currentLine.Length)
            {
                var token = ScanToken(_currentLine[_columnIndex]);
                if (token != null)
                {
                    tokens.Add(token);
                }
            }
            tokens.Add(new Token(TokenType.Newline, "", null, _lineNumber, 0, 0));
        }

        tokens.Add(new Token(TokenType.EOF, "", null, _lineNumber, 0, 0));

        return tokens;
    }

    private Token? ScanToken(char c)
    {
        _start = _columnIndex;
        ++_columnIndex;

        if (_isInMultilineComment)
        {
            if (c == '*' && IsMatch('/'))
            {
                _isInMultilineComment = false;
            }
            return null;
        }
        switch (c)
        {
            case '(': return MakeToken(TokenType.LeftParen);
            case ')': return MakeToken(TokenType.RightParen);
            case '{': return MakeToken(TokenType.LeftBrace);
            case '}': return MakeToken(TokenType.RightBrace);
            case ',': return MakeToken(TokenType.Comma);
            case '.': return MakeToken(TokenType.Dot);
            case '-': return MakeToken(TokenType.Minus);
            case '+': return MakeToken(TokenType.Plus);
            case '*': return MakeToken(TokenType.Star);
            // Special handling for some multi-char tokens:
            case '!':
                return MakeToken(IsMatch('=')
                      ? TokenType.BangEqual
                      : TokenType.Bang);
            case '=':
                return MakeToken(IsMatch('=')
                      ? TokenType.DoubleEqual
                      : TokenType.Equal);
            case '<':
                return MakeToken(IsMatch('=')
                      ? TokenType.LessEqual
                      : TokenType.Less);
            case '>':
                return MakeToken(IsMatch('=')
                      ? TokenType.GreaterEqual
                      : TokenType.Greater);
            case '/':
                {
                    // We have a comment
                    if (IsMatch('/'))
                    {
                        while (!IsLineEnd())
                        {
                            ++_columnIndex;
                        }
                    }
                    else if (IsMatch('*'))
                    {
                        _isInMultilineComment = true;
                        while (!IsLineEnd())
                        {
                            ++_columnIndex;
                        }
                    }
                    else
                    {
                        return MakeToken(TokenType.Slash);
                    }

                    break;
                }

            case ' ':
            case '\r':
            case '\t':
                // Ignore whitespace.
                break;

            case '"':
                {
                    var token = ReadString();
                    if (token != null)
                    {
                        return token;
                    }
                    break;
                }

            default:
                {
                    if (IsDigit(c))
                    {
                        var token = ReadNumber();
                        if (token != null)
                        {
                            return token;
                        }
                    }
                    else if (IsAlpha(c))
                    {
                        var token = ReadIdentifier();
                        if (token != null)
                        {
                            return token;
                        }
                    }
                    else
                    {
                        IsSuccess = false;

                        _logger.Error(
                            _lineNumber,
                            _columnIndex,
                            _currentLine,
                            $"Unexpected character: `{c}`"
                        );
                    }
                    return null;
                }
        }

        return null;
    }

    private Token MakeToken(TokenType type)
    {
        return MakeToken(type, null);
    }

    private Token MakeToken(TokenType type, object? literal)
    {
        var text = _currentLine[_start.._columnIndex];
        return new Token(
            tokenType: type,
            lexeme: text,
            literal: literal,
            _lineNumber,
            _columnIndex,
            _columnIndex - _start
        );
    }

    private Token? ReadString()
    {
        while (Peek() != '"' && !IsLineEnd())
        {
            ++_columnIndex;
        }

        if (IsLineEnd())
        {
            _logger.Error(
                _lineNumber,
                _columnIndex + 1,
                _currentLine,
                "Unterminated string."
            );
            IsSuccess = false;
            return null;
        }

        ++_columnIndex;

        var text = _currentLine[_start.._columnIndex];
        var val = _currentLine.Substring(_start + 1, _columnIndex - _start - 2);
        return new Token(
            tokenType: TokenType.String,
            lexeme: text,
            literal: val,
            _lineNumber,
            _columnIndex,
            _columnIndex - _start
        );
    }

    private Token? ReadNumber()
    {
        while (IsDigit(Peek()))
        {
            ++_columnIndex;
        }

        if (Peek() == '.' && IsDigit(Peek(1)))
        {
            ++_columnIndex;

            while (IsDigit(Peek()))
            {
                ++_columnIndex;
            }
        }

        var text = _currentLine[_start.._columnIndex];
        var val = double.Parse(text, CultureInfo.InvariantCulture);
        return new Token(
            tokenType: TokenType.Number,
            lexeme: text,
            literal: val,
            _lineNumber,
            _columnIndex,
            _columnIndex - _start
        );
    }

    private Token? ReadIdentifier()
    {
        while (IsAlphaNumeric(Peek()))
        {
            ++_columnIndex;
        }

        var text = _currentLine[_start.._columnIndex];
        bool isKeyword = BDSMKeywords.Keywords.TryGetValue(text, out TokenType type);

        if (!isKeyword)
        {
            type = TokenType.Identifier;
        }
        return MakeToken(type);
    }

    private bool IsMatch(char expected)
    {
        if (IsLineEnd())
        {
            return false;
        }
        if (_currentLine[_columnIndex] != expected)
        {
            return false;
        }

        ++_columnIndex;
        return true;
    }

    private char Peek(int num = 0)
    {
        if (IsLineEnd())
        {
            return '\0';
        }
        if (_columnIndex + num >= _currentLine.Length)
        {
            return '\0';
        }

        return _currentLine[_columnIndex + num];
    }

    private bool IsLineEnd()
    {
        return _columnIndex >= _currentLine.Length;
    }

    private static bool IsDigit(char c)
    {
        return c is >= '0' and <= '9';
    }
    private static bool IsAlpha(char c)
    {
        return c is (>= 'a' and <= 'z') or (>= 'A' and <= 'Z') or '_';
    }
    private static bool IsAlphaNumeric(char c)
    {
        return IsDigit(c) || IsAlpha(c);
    }
}
