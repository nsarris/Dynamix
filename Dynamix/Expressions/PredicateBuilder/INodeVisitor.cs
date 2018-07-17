namespace Dynamix.Expressions.PredicateBuilder
{
    public interface INodeVisitor<out T>
    {
        T VisitComplexNode(ComplexNode node);
        T VisitUnaryNode(BinaryNode node);
    }

    public interface INodeVisitor<out T, in TInput>
    {
        T VisitComplexNode(ComplexNode node, TInput input);
        T VisitUnaryNode(BinaryNode node, TInput input);
    }

    public interface INodeVisitor<out T, in TInput, in TState>
    {
        T VisitComplexNode(ComplexNode node, TInput input, TState state);
        T VisitUnaryNode(BinaryNode node, TInput input, TState state);
    }
}
