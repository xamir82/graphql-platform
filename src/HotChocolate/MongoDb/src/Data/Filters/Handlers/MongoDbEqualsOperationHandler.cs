using HotChocolate.Configuration;
using HotChocolate.Data.Filters;
using HotChocolate.Language;
using HotChocolate.Types;
using MongoDB.Bson;
using MongoDB.Driver;

namespace HotChocolate.Data.MongoDb.Filters;

/// <summary>
/// This filter operation handler maps a Equals operation field to a
/// <see cref="FilterDefinition{TDocument}"/>
/// </summary>
public class MongoDbEqualsOperationHandler
    : MongoDbOperationHandlerBase
{
    public MongoDbEqualsOperationHandler(InputParser inputParser) : base(inputParser)
    {
    }

    /// <inheritdoc />
    public override bool CanHandle(
        ITypeCompletionContext context,
        IFilterInputTypeDefinition typeDefinition,
        IFilterFieldDefinition fieldDefinition)
    {
        return fieldDefinition is FilterOperationFieldDefinition operationField &&
            operationField.Id is DefaultFilterOperations.Equals;
    }

    /// <inheritdoc />
    public override MongoDbFilterDefinition HandleOperation(
        MongoDbFilterVisitorContext context,
        IFilterOperationField field,
        IValueNode value,
        object? parsedValue)
    {
        var doc = new MongoDbFilterOperation("$eq", parsedValue);

        return new MongoDbFilterOperation(context.GetMongoFilterScope().GetPath(), doc);
    }
}
