namespace Lucida.Krab.Parsing
{
    public struct Operator
    {
        public int Precedence;
        public int Arguments;
        public bool LeftAssociative;

        public Operator(int precedence, int arguments, bool leftAssociative)
        {
            Precedence = precedence;
            Arguments = arguments;
            LeftAssociative = leftAssociative;
        }
    }
}
