namespace IronManServer.Config;

using SPTarkov.DI.Annotations;

[Injectable(InjectionType = InjectionType.Singleton)]
public class ApplyConfig
{

    public Task ApplyConfiguration(CancellationToken cancellationToken = default)
    {
        // This doesn't do anything right now, I've built this just for now in case I use it later
        return Task.CompletedTask;
    }
}

