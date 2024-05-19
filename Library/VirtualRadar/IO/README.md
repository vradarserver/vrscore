# VirtualRadar.IO

## Chunking

Chunking is the act of taking a feed of bytes and breaking it up into
chunks for further processing. An ASCII text feed might break the feed
up into lines, a binary feed into messages and so on.

### StreamChunker

`StreamChunker` is the abstract base for the classes that extract chunks
from a feed stream. It exposes the chunks as an event with a
ReadOnlyMemory<byte> payload. 

The chunker blocks while it waits for the event handlers to run. It is
important that the event handlers take as little time as possible.

The chunker allocates the memory for the event out of a shared pool. It
will release the memory for reuse as soon as the event handler returns.
It is important that the event handler does not retain a reference to
the memory.

Ideally the event handler should take its own copy of the chunk and then
pass that to a background thread for processing so that it doesn't block
the chunker for any longer than necessary.
