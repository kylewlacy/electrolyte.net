using System.Reflection;
using System.Runtime.CompilerServices;
using Tiko;

// Information about this assembly is defined by the following attributes.
// Change them to the values specific to your project.
[assembly: AssemblyTitle("Electrolyte.Standard")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("")]
[assembly: AssemblyProduct("")]
[assembly: AssemblyCopyright("kyle")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]
// The assembly version has the format "{Major}.{Minor}.{Build}.{Revision}".
// The form "{Major}.{Minor}.*" will automatically update the build and revision,
// and "{Major}.{Minor}.{Build}.*" will update just the revision.
[assembly: AssemblyVersion("1.0.*")]
// The following attributes are used to specify the signing key for the assembly,
// if desired. See the Mono documentation for more information about signing.
//[assembly: AssemblyDelaySign(false)]
//[assembly: AssemblyKeyFile("")]

// TODO: WHY IS THERE NO EASY WAY OF REFERENCING AN ASSEMBLY
[assembly: Uses(typeof(Org.BouncyCastle.Standard.Crypto.Prng.ThreadedSeedGenerator))]

