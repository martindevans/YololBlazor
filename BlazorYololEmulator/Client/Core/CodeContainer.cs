using Yolol.Grammar;

namespace BlazorYololEmulator.Client.Core;

public class CodeContainer
{
    private string _code;
    public string YololCode
    {
        get => _code;
        set
        {
            _code = value;
            ParseResult = Parser.ParseProgram(_code);
        }
    }

    public Parser.Result<Yolol.Grammar.AST.Program, Parser.ParseError> ParseResult { get; private set; }

    public CodeContainer()
    {
        _code = "";
        ParseResult = Parser.ParseProgram("");
    }
}