using System;
using System.Threading;

namespace Allure.Net.Commons;

public enum AllureRuntimeApiOperation
{
    SetTestName,
    SetDescription,
    SetDescriptionHtml,
    AddLabels,
    AddLabel,
    SetSeverity,
    SetOwner,
    SetAllureId,
    AddTags,
    AddEpic,
    AddFeature,
    AddStory,
    AddLinks,
    AddTestParameter,
}

public readonly record struct AllureRuntimeApiOperationContext(
    AllureRuntimeApiOperation Operation,
    string? Name = null
);

public static class AllureRuntimeApiPolicy
{
    static readonly AsyncLocal<Func<AllureRuntimeApiOperationContext, bool>?> s_filter = new();

    public static IDisposable PushFilter(Func<AllureRuntimeApiOperationContext, bool> filter)
    {
        var previousFilter = s_filter.Value;
        s_filter.Value = filter ?? throw new ArgumentNullException(nameof(filter));
        return new FilterScope(previousFilter);
    }

    internal static bool IsAllowed(AllureRuntimeApiOperationContext context) =>
        s_filter.Value?.Invoke(context) ?? true;

    sealed class FilterScope(Func<AllureRuntimeApiOperationContext, bool>? previousFilter) : IDisposable
    {
        bool _disposed;

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            s_filter.Value = previousFilter;
            _disposed = true;
        }
    }
}
