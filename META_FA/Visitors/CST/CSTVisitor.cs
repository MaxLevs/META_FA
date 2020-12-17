using META_FA.CST;

namespace META_FA.Visitors.CST
{
    public abstract class CSTVisitor
    {
        public abstract void Apply(RegexSymbol regexSymbol);
        public abstract void Apply(RegexQuantifierMaybe regexQuantifierMaybe);
        public abstract void Apply(RegexQuantifierOneOrInfinity regexQuantifierOneOrInfinity);
        public abstract void Apply(RegexQuantifierZeroOrInfinity regexQuantifierZeroOrInfinity);
        public abstract void Apply(RegexString regexString);
        public abstract void Apply(RegexVariant regexVariant);
    }
}