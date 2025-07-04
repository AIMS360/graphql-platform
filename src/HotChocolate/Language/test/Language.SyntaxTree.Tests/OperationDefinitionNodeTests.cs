namespace HotChocolate.Language.SyntaxTree;

public class OperationDefinitionNodeTests
{
    [Fact]
    public void Equals_With_Same_Location()
    {
        // arrange
        var a = new OperationDefinitionNode(
            TestLocations.Location1,
            new NameNode("aa"),
            description: null,
            OperationType.Query,
            new List<VariableDefinitionNode>(0),
            new List<DirectiveNode>(0),
            new SelectionSetNode(new List<ISelectionNode>(0)));
        var b = new OperationDefinitionNode(
            TestLocations.Location2,
            new NameNode("aa"),
            description: null,
            OperationType.Query,
            new List<VariableDefinitionNode>(0),
            new List<DirectiveNode>(0),
            new SelectionSetNode(new List<ISelectionNode>(0)));
        var c = new OperationDefinitionNode(
            TestLocations.Location1,
            new NameNode("aa"),
            description: null,
            OperationType.Mutation,
            new List<VariableDefinitionNode>(0),
            new List<DirectiveNode>(0),
            new SelectionSetNode(new List<ISelectionNode>(0)));
        var d = new OperationDefinitionNode(
            TestLocations.Location1,
            new NameNode("aa"),
            description: null,
            OperationType.Mutation,
            new List<VariableDefinitionNode>(0),
            new List<DirectiveNode>(0),
            new SelectionSetNode(new List<ISelectionNode>(0)));

        // act
        var abResult = SyntaxComparer.BySyntax.Equals(a, b);
        var aaResult = SyntaxComparer.BySyntax.Equals(a, a);
        var acResult = SyntaxComparer.BySyntax.Equals(a, c);
        var adResult = SyntaxComparer.BySyntax.Equals(a, d);
        var aNullResult = SyntaxComparer.BySyntax.Equals(a, null);

        // assert
        Assert.True(abResult);
        Assert.True(aaResult);
        Assert.False(acResult);
        Assert.False(adResult);
        Assert.False(aNullResult);
    }

    [Fact]
    public void Equals_With_Different_Location()
    {
        // arrange
        var a = new OperationDefinitionNode(
            TestLocations.Location1,
            new NameNode("aa"),
            description: null,
            OperationType.Query,
            new List<VariableDefinitionNode>(0),
            new List<DirectiveNode>(0),
            new SelectionSetNode(new List<ISelectionNode>(0)));
        var b = new OperationDefinitionNode(
            TestLocations.Location2,
            new NameNode("aa"),
            description: null,
            OperationType.Query,
            new List<VariableDefinitionNode>(0),
            new List<DirectiveNode>(0),
            new SelectionSetNode(new List<ISelectionNode>(0)));
        var c = new OperationDefinitionNode(
            TestLocations.Location1,
            new NameNode("aa"),
            description: null,
            OperationType.Mutation,
            new List<VariableDefinitionNode>(0),
            new List<DirectiveNode>(0),
            new SelectionSetNode(new List<ISelectionNode>(0)));
        var d = new OperationDefinitionNode(
            TestLocations.Location2,
            new NameNode("aa"),
            description: null,
            OperationType.Mutation,
            new List<VariableDefinitionNode>(0),
            new List<DirectiveNode>(0),
            new SelectionSetNode(new List<ISelectionNode>(0)));

        // act
        var abResult = SyntaxComparer.BySyntax.Equals(a, b);
        var aaResult = SyntaxComparer.BySyntax.Equals(a, a);
        var acResult = SyntaxComparer.BySyntax.Equals(a, c);
        var adResult = SyntaxComparer.BySyntax.Equals(a, d);
        var aNullResult = SyntaxComparer.BySyntax.Equals(a, null);

        // assert
        Assert.True(abResult);
        Assert.True(aaResult);
        Assert.False(acResult);
        Assert.False(adResult);
        Assert.False(aNullResult);
    }

    [Fact]
    public void GetHashCode_With_Location()
    {
        // arrange
        var a = new OperationDefinitionNode(
            TestLocations.Location1,
            new NameNode("aa"),
            description: null,
            OperationType.Query,
            new List<VariableDefinitionNode>(0),
            new List<DirectiveNode>(0),
            new SelectionSetNode(new List<ISelectionNode>(0)));
        var b = new OperationDefinitionNode(
            TestLocations.Location2,
            new NameNode("aa"),
            description: null,
            OperationType.Query,
            new List<VariableDefinitionNode>(0),
            new List<DirectiveNode>(0),
            new SelectionSetNode(new List<ISelectionNode>(0)));
        var c = new OperationDefinitionNode(
            TestLocations.Location1,
            new NameNode("aa"),
            description: null,
            OperationType.Mutation,
            new List<VariableDefinitionNode>(0),
            new List<DirectiveNode>(0),
            new SelectionSetNode(new List<ISelectionNode>(0)));
        var d = new OperationDefinitionNode(
            TestLocations.Location2,
            new NameNode("aa"),
            description: null,
            OperationType.Mutation,
            new List<VariableDefinitionNode>(0),
            new List<DirectiveNode>(0),
            new SelectionSetNode(new List<ISelectionNode>(0)));

        // act
        var aHash = SyntaxComparer.BySyntax.GetHashCode(a);
        var bHash = SyntaxComparer.BySyntax.GetHashCode(b);
        var cHash = SyntaxComparer.BySyntax.GetHashCode(c);
        var dHash = SyntaxComparer.BySyntax.GetHashCode(d);

        // assert
        Assert.Equal(aHash, bHash);
        Assert.NotEqual(aHash, cHash);
        Assert.Equal(cHash, dHash);
        Assert.NotEqual(aHash, dHash);
    }
}
