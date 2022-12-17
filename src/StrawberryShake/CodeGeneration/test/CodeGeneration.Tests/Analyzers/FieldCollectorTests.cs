using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using HotChocolate;
using HotChocolate.Execution;
using HotChocolate.Language;
using HotChocolate.StarWars;
using HotChocolate.Types;
using Xunit;
using Path = HotChocolate.Path;

namespace StrawberryShake.CodeGeneration.Analyzers;

public class FieldCollectorTests
{
    [Fact]
    public async Task Collect_First_Level_No_Fragments()
    {
        // arrange
        var schema =
            await new ServiceCollection()
                .AddStarWarsRepositories()
                .AddGraphQL()
                .AddStarWars()
                .BuildSchemaAsync();

        var document =
            Utf8GraphQLParser.Parse(@"
                {
                    hero(episode: NEW_HOPE) {
                        name
                        ... on Droid {
                            primaryFunction
                        }
                    }
                }");

        var operation = document
            .Definitions
            .OfType<OperationDefinitionNode>()
            .First();

        // act
        var selectionSetVariants =
            new FieldCollector(schema, document)
                .CollectFields(operation.SelectionSet, schema.QueryType, Path.Root);

        // assert
        Assert.Collection(
            selectionSetVariants.ReturnType.Fields,
            field => Assert.Equal("hero", field.ResponseName));
    }

    [Fact]
    public async Task Collect_Second_Level_Fragments()
    {
        // arrange
        var schema =
            await new ServiceCollection()
                .AddStarWarsRepositories()
                .AddGraphQL()
                .AddStarWars()
                .BuildSchemaAsync();

        var character = schema.GetType<InterfaceType>("Character");

        var document =
            Utf8GraphQLParser.Parse(@"
                {
                    hero(episode: NEW_HOPE) {
                        name
                        ... on Droid {
                            primaryFunction
                        }
                    }
                }");

        var operation = document
            .Definitions
            .OfType<OperationDefinitionNode>()
            .First();

        var secondLevel = operation
            .SelectionSet
            .Selections
            .OfType<FieldNode>()
            .First();

        // act
        var selectionSetVariants =
            new FieldCollector(schema, document)
                .CollectFields(
                    secondLevel.SelectionSet!,
                    character,
                    PathFactory.Instance.New("hero"));

        // assert
        Assert.Collection(
            selectionSetVariants.ReturnType.Fields,
            field => Assert.Equal("name", field.ResponseName));
        Assert.Equal("Character", selectionSetVariants.ReturnType.Type.Name);
        Assert.Equal("Human", selectionSetVariants.Variants[0].Type.Name);
        Assert.Equal("Droid", selectionSetVariants.Variants[1].Type.Name);

        Assert.Collection(
            selectionSetVariants.Variants[1].FragmentNodes,
            fragmentNode => Assert.Equal(FragmentKind.Inline, fragmentNode.Fragment.Kind));
    }
}
