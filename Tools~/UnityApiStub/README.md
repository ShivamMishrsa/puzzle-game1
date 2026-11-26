# Unity API stub (compile check without Unity)

`UnityStubs.cs` is a tiny hand-written subset of the UnityEngine API (signatures only, no behaviour).
It lets the C# 5 compiler that ships with Windows type-check the runtime scripts when Unity is not installed:

```powershell
$files = @("Tools~\UnityApiStub\UnityStubs.cs") + (Get-ChildItem "Assets\_Project\Scripts\*.cs","Assets\_Project\Scripts\Core\*.cs","Assets\_Project\Scripts\Monetization\*.cs" | % FullName)
& "C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe" -nologo -target:library -out:check.dll $files
```

This catches typos, missing members and type errors in the game code. It does NOT prove that the real Unity API
behaves as assumed - always still open the project in Unity and read the Console. (Editor scripts under `Assets/_Project/Editor`
that use UnityEditor / URP types are not covered.)
