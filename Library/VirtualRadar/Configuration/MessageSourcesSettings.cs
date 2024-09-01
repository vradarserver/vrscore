using System.Collections.Immutable;

namespace VirtualRadar.Configuration
{
    [Settings("MessageSources")]
    public record MessageSourcesSettings
    {
        public ImmutableArray<ReceiverSettings> Receivers { get; init; }

        public MessageSourcesSettings(
            ReceiverSettings[] receivers = null
        )
        {
            Receivers = ImmutableArray.Create(receivers ?? []);
        }

        public virtual bool Equals(MessageSourcesSettings other)
        {
            return other != null
                && Receivers.SequenceEqual(other.Receivers)
            ;
        }

        public override int GetHashCode()
        {
            // I'm not expecting many of these, nor am I expecting them to be used as keys, so
            // I'm not too fussed about the cardinality of their hash codes.
            return Receivers.Length;
        }
    }
}
