# Level verifier (no Unity needed)

Compiles the engine-free rules code (`Assets/_Project/Scripts/Core`, `GameEnums.cs`) together with the level
classes (`Assets/_Project/Editor/Levels`) and proves every level can be won within its move limit.

```powershell
# from the repository root (Windows, built-in .NET Framework compiler)
$files = @("Tools~\LevelVerifier\Program.cs", "Assets\_Project\Scripts\GameEnums.cs") +
         (Get-ChildItem "Assets\_Project\Scripts\Core\*.cs" | % FullName) +
         @("Assets\_Project\Editor\LevelLibrary.cs") +
         (Get-ChildItem "Assets\_Project\Editor\Levels\*.cs" | % FullName)
& "C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe" -nologo -out:LevelVerifier.exe $files

.\LevelVerifier.exe 1 20          # verify levels 1..20 (prints par, ammo/HP per colour, OK/FAIL)
.\LevelVerifier.exe mon 20        # can the monument be dismantled at all, ignoring board geometry?
.\LevelVerifier.exe dbg 20 5      # print one random layout of a template and per-block mobility
.\LevelVerifier.exe search 20 60  # search layouts for a template with unplaced blocks (x = -1)
```

Environment variables for `search`: `CAPB` / `CAPH` (BFS / best-first state budgets), `BIAS` (0..1, start blocks near
their own gates), `LIMIT` (override move limit), `SEED`, `STRIP` (`ice,peel,shutter,scrap` to bisect a broken template).
The same checks run inside Unity via **Tools > Color Gate Blast > Generate Levels N-M** and **Verify All Levels**.
