dotnet pack IEC.Target.SqlServer\IEC.Target.SqlServer.csproj -p:TargetFrameworks=net5 -c:Release

xcopy /Y IEC.Target.SqlServer\bin\Release\*.nupkg .