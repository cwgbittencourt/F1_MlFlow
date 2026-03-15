namespace F1_MlFlow.Services.State;

public sealed class NavigationLoadingState
{
    public bool IsActive { get; private set; }
    public string? TargetUri { get; private set; }
    public event Action? OnChange;

    public void Start(string targetUri)
    {
        if (string.IsNullOrWhiteSpace(targetUri))
        {
            return;
        }

        IsActive = true;
        TargetUri = targetUri;
        NotifyStateChanged();
    }

    public void Stop()
    {
        if (!IsActive)
        {
            return;
        }

        IsActive = false;
        TargetUri = null;
        NotifyStateChanged();
    }

    private void NotifyStateChanged() => OnChange?.Invoke();
}
