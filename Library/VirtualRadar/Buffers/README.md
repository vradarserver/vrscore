# VirtualRadar.Buffers

# `IsolatedMemoryPool`

The use of `MemoryPool<byte>.Shared` relies on discipline. If that discipline
is not maintained then it can have grevious consequences.

In particular if something were to erroneously store a byte array that had been
rented from the shared instance, and then write to the array after it had been
released back to the pool, then the effects are akin to a use-after-free bug in
unmanaged code.

The array could have been reused in a subsequent rent from the pool, so when the
bugged function starts writing to the array that it should never have held onto
it will corrupt data for some random part of the program. You won't see the issue
until that random part crashes. Bugs where the effect of the bug is far away from
the bug itself are very hard to track down.

But having a shared pool has obvious benefits and we don't want to ban them.
Moreover, having a single shared pool has the best chance of delaying the onset of
a garbage collection when allocating and freeing many small buffers.

## Private buffers

When you need to allocate and use many small short-lived buffers, and those
buffers are generally private to your class (or just your class and your subclasses),
then you should use `MemoryPool<byte>` as per usual.

## Public buffers

However, if your short-lived buffer is going to be passed in the arguments to
an event, or to a callback, or otherwise head off into a function that you don't
have control over, then trusting that unknown bit of code to not take a reference
to the memory starts to get a bit uncomfortable.

Ideally you would allocate a block from the heap, copy the pooled block to the
allocated block and then pass the allocated block in the event.

However, if the intention is that a well-behaved event would not need to retain
the block after use then you end up back at the original problem of creating many
small blocks short-lived blocks on the heap when, if everything were working fine,
you don't need to.

As a half-way house between the two we have the `IsolatedMemoryPool`. The idea is that
it's based on `MemoryPool<byte>` and it works the same way, except instead of one pool
for the entire program you have one pool per sub-system.

Having a per sub-system pool doesn't protect you from the bugs any more than an
application pool does. But what it does do is it limits the effect of those bugs to
just the sub-system. Now when you get a weird corruption you know that the only code
that can be responsible is whatever is allocating out of the isolated pool, and not
any bit of code across the entire application.

## Downsides

The problem with isolated pools is that you end up with more allocations, so don't go
mad with them. I would still use `MemoryPool<byte>.Shared` for most things, just use
isolated pools if the allocations are going to be exposed to the wider world.
