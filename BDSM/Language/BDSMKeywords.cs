using BDSM.Tokens;
using System.Collections.Generic;

namespace BDSM.Language;

public static class BDSMKeywords
{
    public static readonly Dictionary<string, TokenType> Keywords;

    static BDSMKeywords()
    {
        Keywords = new Dictionary<string, TokenType>
        {
            { "import",     TokenType.Import },
            { "if",         TokenType.If },
            { "else",       TokenType.Else },
            { "scene",      TokenType.Scene },
            { "actor",      TokenType.Actor },
            { "entity",     TokenType.Entity },
            { "verb",       TokenType.Verb },
            { "on",         TokenType.On },
            { "animations", TokenType.Animations },
            { "nil",        TokenType.Nil },
            { "var",        TokenType.Var },
            { "and",        TokenType.And },
            { "or",         TokenType.Or },
            { "while",      TokenType.While },
            { "for",        TokenType.For },
            { "fun",        TokenType.Fun },
            { "return",     TokenType.Return },
            { "true",       TokenType.True },
            { "false",      TokenType.False },
            { "async",      TokenType.Async },
            { "then",       TokenType.Then },
            { "region",     TokenType.Region },
            { "prop",       TokenType.Prop },

            // DEBUG
            { "print", TokenType.Print },
        };
    }
}