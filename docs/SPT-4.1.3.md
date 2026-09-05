# Port MoreBotsAPI to SPT 4.1.3 and fix SAIN custom-role initialization

On the tested setup, SAIN 4.5 threw ArgumentOutOfRangeException in ToESain for blackDivIb and bossWedge. Failed initialization then caused secondary disposal exceptions. The custom role must remain available to factions and BigBrain while SAIN receives a role it understands.

## Implementation notes

- Patch only ToESain(WildSpawnType). Registered custom roles map to their declared vanilla BaseBrain at this conversion boundary, provided that role exists in SAIN's mirror enum. Ordinary and unknown roles pass through. The profile's role and faction identity are not rewritten.
- Port server metadata, DI services, table/config access and namespaces to SPT 4.1.3 / .NET 10.
- Await ProfileDataService operations through the faction service and HTTP route flow, forwarding CancellationToken. Do not block asynchronously persisted profile data with Result/Wait.
- Normalize configuration dictionary keys where SPT uses lowercase roles, including PresetBatch and Bosses, so custom bot generation finds the intended configuration.
- Update client AI/Harmony type mappings and references to BigBrain 1.5.0 and SAIN 4.5.0.

## Validation specific to this mod

An isolated probe against the actual SAIN assembly reproduced the original exception and verified the prefix for all six BlackDiv custom roles, normal roles and an unknown role. Server checks returned faction data and generated blackdivib, bosswedge and assault bots with nonempty inventories. The tester subsequently reported normal-looking bot behavior in a completed raid; difficulty tuning was not changed.

The API-compatibility fix does not claim to add full independent SAIN difficulty presets for every custom role. Existing interop scaffolding is retained.

## Compatibility and evidence

Target: **SPT 4.1.3**, EFT **0.16.9.5.40743**. This is a source contribution for that environment, not a claim of compatibility with future SPT releases or Fika.

The tester successfully loaded Icebreaker, entered a raid and extracted. In subsequent feedback they confirmed the blowtorch, extraction and doors work, and Black Division bots appeared to behave normally after the SAIN fix. These are user-reported functional observations, not automated coverage of every encounter or performance benchmarks. Ten continuous hours, all quests and multiplayer have not been tested.

The migration used installed SPT 4.1.3 assemblies and these guides:
- https://wiki.sp-tushonka.com/en/modding/SPT_41_Modding/Server_413_Changes
- https://wiki.sp-tushonka.com/en/modding/SPT_41_Modding/client/Class_Name_Mappings
- https://wiki.sp-tushonka.com/en/modding/SPT_41_Modding/server/Mod_Web_Pages

## Build and packaging

Use the .NET 10 SDK and an installed SPT 4.1.3 dependency set. Pass `-p:SPTPath=<installation-root>` to dotnet build; the fallback expects this repository under a development tree. DeployToGame defaults to disabled. Build the client and server projects in Release, and the companion dependency ports first. Proprietary game DLLs are local references and must not be committed.


## Updated upstream base

Merged upstream 4b4fdcdd18132360fe977744ba7f77f7400cdd74 (server SAIN interop). Its new registration service, model, contract DLL and 2.1 version are retained. The client no-op registration call stays disabled as upstream intended; the ToESain guard and awaited, cancellable profile flow remain. The original raid evidence predates this merge; the combined source is build-verified only.
