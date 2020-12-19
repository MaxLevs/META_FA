
using System.Data;
using BFParser;
using BFParser.Parsers;

namespace StateMachineLib.Options.Exception
{
    public class LoadFromFileException : CoreOptionsException
    {
        public override string Message { get; }
        
        public LoadFromFileException(Grammar grammar, CoreParser parser, string configText)
        {
            Message = $"Errors occured during a parsing. Grammar: {grammar}; Parser: {parser}";
        }
    }
}