namespace BDSM.Tokens;

public enum TokenType
{
    LeftParen, RightParen,
    LeftBrace, RightBrace,
    Comma, Dot,
    Minus, Plus,
    Slash,
    Star,

    Bang, BangEqual,
    Equal, DoubleEqual,
    Greater, GreaterEqual,
    Less, LessEqual,

    Identifier, String, Number, Boolean,

    Import, If, Else, Scene,
    Actor, Entity, Item,
    Verb, On, Animations, True, False,
    And, Or, Wait, Nil, Var,
    While, For, Fun, Return,
    Async, Then,
    Region, Prop,

    Newline,

    Print,

    EOF,
}