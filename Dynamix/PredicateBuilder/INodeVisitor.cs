namespace Dynamix.PredicateBuilder
{
    public interface INodeVisitor<T>
    {
        T VisitComplexNode(ComplexNode node);
        T VisitUnaryNode(UnaryNode node);
    }

    public interface INodeVisitor<T, TInput>
    {
        T VisitComplexNode(ComplexNode node, TInput input);
        T VisitUnaryNode(UnaryNode node, TInput input);
    }

    public interface INodeVisitor<T, TInput, TState>
    {
        T VisitComplexNode(ComplexNode node, TInput input, TState state);
        T VisitUnaryNode(UnaryNode node, TInput input, TState state);
    }
}
