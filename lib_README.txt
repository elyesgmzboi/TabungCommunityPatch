# lib/ folder setup for TabungCommunityPatch

The original .csproj pointed every reference at a folder that only existed on the
original developer's machine (..\..\OfflinePhoton\bin\Debug\net472\). That path
doesn't exist on your machine, which is why MonoMod.Backports.dll (and possibly
others) were silently missing or resolving to old/wrong versions.

All HintPaths have been changed to `lib\<filename>`, matching the folder-based
setup used in the standalone PhotonRedirect project. You need to populate lib\
with these files, grouped by where they actually come from:

## 1. From your game's <GameName>_Data\Managed folder
(Same source as your PhotonRedirect project's lib folder - copy these over directly)
- Assembly-CSharp.dll
- Assembly-CSharp-firstpass.dll
- Photon3Unity3D.dll
- PhotonChat.dll
- PhotonRealtime.dll
- PhotonUnityNetworking.dll
- PhotonVoice.dll
- PhotonVoice.API.dll
- PhotonVoice.PUN.dll
- Unity.RenderPipelines.Core.Runtime.dll
- mscorlib.dll
- All UnityEngine.*.dll entries listed in the .csproj (there are ~50 - copy every
  UnityEngine module the game ships; safest is to copy every UnityEngine*.dll from
  Managed into lib\)

## 2. From BepInEx/core (same as before)
- BepInEx.dll
- 0Harmony.dll
- Mono.Cecil.dll
- Mono.Cecil.Mdb.dll
- Mono.Cecil.Pdb.dll
- Mono.Cecil.Rocks.dll
- MonoMod.RuntimeDetour.dll
- MonoMod.Utils.dll

## 3. MonoMod split-assembly files (NOT in your BepInEx/core - this is the actual bug)
These need to be fetched separately since your BepInEx 5.4.23.5 core folder predates
them:
- MonoMod.Backports.dll
- MonoMod.ILHelpers.dll
- MonoMod.Core.dll
- MonoMod.Iced.dll
- System.ValueTuple.dll

Get these from a NuGet package cache after a `dotnet restore`, OR from a newer
BepInEx build/pack that bundles them, OR directly via NuGet package extraction:
https://www.nuget.org/packages/MonoMod.Backports
https://www.nuget.org/packages/MonoMod.ILHelpers
https://www.nuget.org/packages/MonoMod.Core

## 4. Build-time-only tools - do these actually need to be referenced?
- AsmResolver.dll, AsmResolver.DotNet.dll, AsmResolver.PE.dll, AsmResolver.PE.File.dll
- BepInEx.AssemblyPublicizer.dll, BepInEx.AssemblyPublicizer.MSBuild.dll

These are normally MSBuild-time tools (used to "publicize" internal game types
during compilation), not runtime dependencies a plugin needs at execution time.
Their presence as plain <Reference> entries (rather than as an MSBuild task/
PackageReference) is unusual and may be a leftover from however the original
project was scaffolded. If you don't have these files and get missing-reference
errors, it's worth first checking whether the game code you're patching actually
needs AssemblyPublicizer's output, or whether these can just be removed from the
.csproj entirely.

## Important: remove the OfflinePhoton self-reference
The original .csproj referenced its own output file (OfflinePhoton.dll) as an
external dependency - this is circular and is the direct cause of the CS0436
"conflicts with imported type" warnings you saw during your last build. This has
already been removed from the fixed .csproj included here.
