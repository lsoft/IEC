namespace IEC.Common.Scope
{
    public interface IScopeFactory
    {
        T CreateMutableScope<T>(
            )
            where T : MutableScope;

       ImmutableScope CreateImmutableScope(
            params object?[] objects
            );
    }
}