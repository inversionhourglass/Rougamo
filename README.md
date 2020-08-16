# Rougamo
### Priority
1. `IgnoreMoAttribute`
2. Method `MoAttribute`
3. Method `MoProxyAttribute`
4. Type `MoAttribute`
5. Type `MoProxyAttribute`
6. Type `IRougamo<>`, `IRougamo<,>`, `IRepulsionsRougamo<,>`
7. Assembly & Module `MoAttribute`

### IgnoreMoAttribute
```csharp
// ignore all mo weave
[assembly: IgnoreMo]
// ignore all TheMoAttribute weave
[assembly: IgnoreMo(MoTypes = new[] { typeof(TheMoAttribute))]

// ignore this class
[IgnoreMo]
class Class1
{
    // ...
}

// ignore TheMoAttribute only
[IgnoreMo(MoTypes = new[] { typeof(TheMoAttribute))]
class Class2
{
    // ...
}

class Class3
{
    // ignore this method
    [IgnoreMo]
    public void M1() { }

    // ignore TheMoAttribute only
    [IgnoreMo(MoTypes = new[] { typeof(TheMoAttribute))]
    public void M2() { }
}
```

### Repulsions
```csharp
public class Mo1Attribute : MoAttribute
{
    // ...
}
public class Mo2Attribute : MoAttribute
{
    // ...
}
public class Mo3Attribute : MoAttribute
{
    // ...
}

public class Test : IRougamo<Mo1Attribute, Mo2Attribute>
{
    [Mo2]
    public void M1()
    {
        // Mo1Attribute will be weave
    }
    [Mo3]
    public void M2()
    {
        // Mo1Attribute and Mo3Attribute will be weave
    }
}
```