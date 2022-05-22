using System;
using System.Collections.Generic;
using static HotChocolate.Language.Properties.LangRewritersResources;

namespace HotChocolate.Language;

public class QuerySyntaxRewriter<TContext> : SyntaxRewriter<TContext>
{
    protected virtual bool VisitFragmentDefinitions => true;

    public virtual ISyntaxNode Rewrite(
        ISyntaxNode node,
        TContext context)
    {
        if (node == null)
        {
            throw new ArgumentNullException(nameof(node));
        }

        return node switch
        {
            DocumentNode document => RewriteDocument(document, context),
            OperationDefinitionNode operation => RewriteOperationDefinition(operation, context),
            FieldNode field => RewriteField(field, context),
            FragmentSpreadNode spread => RewriteFragmentSpread(spread, context),
            FragmentDefinitionNode fragment => RewriteFragmentDefinition(fragment, context),
            InlineFragmentNode inline => RewriteInlineFragment(inline, context),
            ObjectValueNode objectValue => RewriteObjectValue(objectValue, context),
            ListValueNode listValue => RewriteListValue(listValue, context),
            VariableNode variable => RewriteVariable(variable, context),
            IValueNode _ => node,
            _ => throw new NotSupportedException(QuerySyntaxRewriter_NotSupported)
        };
    }

    protected virtual DocumentNode RewriteDocument(
        DocumentNode node,
        TContext context)
    {
        IReadOnlyList<IDefinitionNode> rewrittenDefinitions =
            RewriteMany(node.Definitions, context, RewriteDefinition);

        return ReferenceEquals(node.Definitions, rewrittenDefinitions)
            ? node : node.WithDefinitions(rewrittenDefinitions);
    }

    protected virtual IDefinitionNode RewriteDefinition(
        IDefinitionNode node,
        TContext context)
    {
        return node switch
        {
            OperationDefinitionNode value => RewriteOperationDefinition(value, context),
            FragmentDefinitionNode value => VisitFragmentDefinitions
                ? RewriteFragmentDefinition(value, context)
                : value,
            _ => node
        };
    }

    protected virtual OperationDefinitionNode RewriteOperationDefinition(
        OperationDefinitionNode node,
        TContext context)
    {
        OperationDefinitionNode current = node;

        if (current.Name != null)
        {
            current = Rewrite(current, current.Name, context,
                RewriteName, current.WithName);

            current = Rewrite(current, current.VariableDefinitions, context,
                (p, c) => RewriteMany(p, c, RewriteVariableDefinition),
                current.WithVariableDefinitions);

            current = Rewrite(current, current.Directives, context,
                (p, c) => RewriteMany(p, c, RewriteDirective),
                current.WithDirectives);
        }

        current = Rewrite(current, current.SelectionSet, context,
            RewriteSelectionSet, current.WithSelectionSet);

        return current;
    }

    protected virtual VariableDefinitionNode RewriteVariableDefinition(
        VariableDefinitionNode node,
        TContext context)
    {
        VariableDefinitionNode current = node;

        current = Rewrite(current, current.Variable, context,
            RewriteVariable, current.WithVariable);

        current = Rewrite(current, current.Type, context,
            RewriteType, current.WithType);

        if (current.DefaultValue != null)
        {
            current = Rewrite(current, current.DefaultValue, context,
                RewriteValue, current.WithDefaultValue);
        }

        return current;
    }

    protected virtual FragmentDefinitionNode RewriteFragmentDefinition(
        FragmentDefinitionNode node,
        TContext context)
    {
        FragmentDefinitionNode current = node;

        current = Rewrite(current, current.Name, context,
            RewriteName, current.WithName);

        current = Rewrite(current, current.TypeCondition, context,
            RewriteNamedType, current.WithTypeCondition);

        current = Rewrite(current, current.VariableDefinitions, context,
            (p, c) => RewriteMany(p, c, RewriteVariableDefinition),
            current.WithVariableDefinitions);

        current = Rewrite(current, current.Directives, context,
            (p, c) => RewriteMany(p, c, RewriteDirective),
            current.WithDirectives);

        current = Rewrite(current, current.SelectionSet, context,
            RewriteSelectionSet, current.WithSelectionSet);

        return current;
    }

    protected virtual SelectionSetNode RewriteSelectionSet(
        SelectionSetNode node,
        TContext context)
    {
        SelectionSetNode current = node;

        current = Rewrite(current, current.Selections, context,
            (p, c) => RewriteMany(p, c, RewriteSelection),
            current.WithSelections);

        return current;
    }

    protected virtual ISelectionNode RewriteSelection(
        ISelectionNode node,
        TContext context)
    {
        switch (node)
        {
            case FieldNode value:
                return RewriteField(value, context);

            case FragmentSpreadNode value:
                return RewriteFragmentSpread(value, context);

            case InlineFragmentNode value:
                return RewriteInlineFragment(value, context);

            default:
                throw new NotSupportedException();
        }
    }

    protected virtual FieldNode RewriteField(
        FieldNode node,
        TContext context)
    {
        FieldNode current = node;

        if (current.Alias != null)
        {
            current = Rewrite(current, current.Alias, context,
                RewriteName, current.WithAlias);
        }

        current = Rewrite(current, current.Name, context,
            RewriteName, current.WithName);

        current = Rewrite(current, current.Arguments, context,
            (p, c) => RewriteMany(p, c, RewriteArgument),
            current.WithArguments);

        current = Rewrite(current, current.Directives, context,
            (p, c) => RewriteMany(p, c, RewriteDirective),
            current.WithDirectives);

        if (current.SelectionSet != null)
        {
            current = Rewrite(current, current.SelectionSet, context,
                RewriteSelectionSet, current.WithSelectionSet);
        }

        return current;
    }

    protected virtual FragmentSpreadNode RewriteFragmentSpread(
        FragmentSpreadNode node,
        TContext context)
    {
        FragmentSpreadNode current = node;

        current = Rewrite(current, current.Name, context,
            RewriteName, current.WithName);

        current = Rewrite(current, current.Directives, context,
            (p, c) => RewriteMany(p, c, RewriteDirective),
            current.WithDirectives);

        return current;
    }

    protected virtual InlineFragmentNode RewriteInlineFragment(
        InlineFragmentNode node,
        TContext context)
    {
        InlineFragmentNode current = node;

        if (current.TypeCondition != null)
        {
            current = Rewrite(current, current.TypeCondition, context,
                RewriteNamedType, current.WithTypeCondition);
        }

        current = Rewrite(current, current.Directives, context,
            (p, c) => RewriteMany(p, c, RewriteDirective),
            current.WithDirectives);

        current = Rewrite(current, current.SelectionSet, context,
            RewriteSelectionSet, current.WithSelectionSet);

        return current;
    }
}
