namespace HotChocolate.Types
{
    public interface ITypeSystemObject
        : IHasName
        , IHasDescription
        , IHasContextData
        , IHasScope
        , ITypeSystemMember
    {
    }
}
