using System.IO;
using System.Text;
using HotChocolate.Language.Extensions;
using Snapshooter.Xunit;
using Xunit;

namespace HotChocolate.Language.Rewriters;

public class QuerySyntaxRewriterTests
{
    [Fact]
    public void RewriteEveryFieldOfTheQuery()
    {
        // arange
        DocumentNode document = Utf8GraphQLParser.Parse(
            "{ foo { bar { baz } } }");

        // act
        DocumentNode rewritten = document
            .Rewrite<DirectiveQuerySyntaxRewriter, DirectiveNode>(
                new DirectiveNode("upper"));

        // assert
        rewritten.ToString(false).MatchSnapshot();
    }
}
