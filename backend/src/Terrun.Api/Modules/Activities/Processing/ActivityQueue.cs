using System.Threading.Channels;

namespace Terrun.Api.Modules.Activities.Processing;

// LLD §6 (D-25): in-memory queue of uploaded activities. Losing it on a restart loses nothing: the startup sweep and the
// hourly job pick up every activity still pending
public sealed class ActivityQueue
{
    private readonly Channel<Guid> _channel = Channel.CreateUnbounded<Guid>(new UnboundedChannelOptions { SingleReader = true });

    public ChannelReader<Guid> Reader => _channel.Reader;

    public void Enqueue(Guid activityId) => _channel.Writer.TryWrite(activityId);
}
