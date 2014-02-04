# Electrolyte.NET
## What?
Electrolyte.NET was my first attempt at building a Bitcoin library with **some pretty cool features**:

- **Thin client** - No need to download the whole Bitcoin blockchain before use; everything goes through the [Electrum](https://electrum.org) (also referred to as 'Stratum') protocol.
- **`async` support** - The library leverages .NET's [asynchronous programming](http://msdn.microsoft.com/en-us/library/hh191443.aspx) system to make everything run in parallel
- **PCL support** - .NET supports and underutilized system called "[Portable Class Libraries](http://msdn.microsoft.com/en-us/library/gg597391(v=vs.110).aspx)" that allows for platform-agnostic code. This means Electrolyte.NET will run on anything that can run a standard .NET implementation (including both Microsoft's platforms and anything that can run [Mono](http://www.mono-project.com/)).
- **Security*** - Wallets are AES-encrypted using a 1024-round SHA256 key. This is pretty great, but still offers much to be improved. (**NOTE**: I'm not a security expert, so proceed with caution in this regard).

Also included are the following:

- **An example MonoMac-based OS X GUI**
- **A command-line tool** (which should run on Windows, Mac, and Linux)

So that all sounds pretty great, right? So what's the catch? Well, **there's quite a lot that is unfinished**:

- **Coin picking** - The coin picker in place currently only uses the most recent transactions, which can lead to higher transaction fees.
- **Script parsing** - The only script that works (read: won't crash the library) is the [standard Bitcoin transaction script](https://en.bitcoin.it/wiki/Script#Standard_Transaction_to_Bitcoin_address_.28pay-to-pubkey-hash.29).
- **Script checking** - Scripts currently aren't actually interpreted to check validity; however, script parsing is only lacking one fundamental instruction: `OP_CHECKSIG`. Most everyhting else works as epxected.
- **Extra protocol rules** - This is sort of a 'bare bones' implementation of the Bitcoin protocol; that is, there's no extra check for, say, any of the accepted [BIPs](https://en.bitcoin.it/wiki/Bitcoin_Improvement_Proposals)

## Why?

Clearly, there's a lot that isn't done in this library. So why release it anyway? Well, there's 3 main reasons:

1. **It's hard writing a Bitcoin client** - There are few good resources on actually *making* a Bitcoin library (*\*gasp\**), so I figure that someone somewhere will try and write one in C#, and I know that having this codebase would have helped me while I was writing it. 
2. **More wallets are good** - Currently, the landscape of *proper* Bitcoin wallets is pretty limited (web-based wallets don't count, since those that do exist are relatively sparse in features — not to mention that there's an extra layer of required trust). While this isn't a library that's ready for prime time, it certainly could be utilized as a jumping-off point for a very good wallet in the future.
3. **This isn't the end** - which leads me to my next point...

## What's Next?

As I stated above, this was my *first* attempt at building a Bitcoin library. Well, I'm staring again from scratch— this time, in plain ol' C++. You can see the progress of this library [here](http://github.com/kylewlacy/electrolyte-lib) (just not that, at the time of writing, I've just barely started on this one).

## Some Extra Notes

The project is split up into several different projects:

- **Electrolyte.Core** - This is where most of the everything happens. This includes standard classes dealing with *transactions*, *addresses*, *scripts*, *protocol details*, and some other higher-level Bitcoin concepts
- **Electrolyte.Standard** - This includes all the standard platform-specific code, which uses **Dependency Injection** to tie in with the Core. Things like *file I/O*, *networking*, and *hashing algorithms* are taken care of here. Note that this is **NOT** a PCL-based project; everything that can't run against the PCL is separated out into here.
- **Electrolyte.CLI** - As you'd expect, this is the command-line interface. This is only partially done, as there's no help menu or the like; this is just a good starting point and demonstrates how the library is interfaced with.
- **Electrolyte.OSX** - This is the OS X interface, which use MonoMac to tie in with Cocoa. This is also only partially done, and demonstrates the strengths of using an `async`-based library.
- **Electrolyte.Test** - This is the NUnit-based testing suite. Run it to see if anything's been broken.

**SUPER IMPORTANT FINAL NOTE**: Be sure to get the **submodules**, since things won't work if you don't. The project is reliant on [*TikoContainer*](https://github.com/kylewlacy/TikoContainer) (for Dependency Injection/Inversion of Control) and [*BouncyCastle*](https://github.com/kylewlacy/bouncycastle-pcl) (for security stuff, like hashing and encryption. Both are custom forks that were modified to work with Portable Class Libraries.
