# IEC
Application's internal errors catcher

This software generate dumps like

```
Exception #0   System.InvalidOperationException   exception-wrapper


Exception #1   IEC.TestConsole.ExpectedException   Exception of type 'IEC.TestConsole.ExpectedException' was thrown.
   at IEC.TestConsole.Program.ProcessMutable(IEC iec) in C:\temp\IEC\IEC.TestConsole\Program.cs:line 105


Frame #0
  IEC.TestConsole.H.ListItem2<System.Object>:{
    B:System.Boolean = "True"
    Index:System.Int32 = "1"
    NullableValue:System.Object = null
  }
  System.Int32 = "1"
  System.String = "11"
  System.DateTime = "01.02.2021 13:01:56.207"

```

and is able to store it to SQL Server (this can be changed with 3rd party code).
