using PatternUsage.Attributes.Executions;
using PatternUsage.Attributes.Getters;
using PatternUsage.Attributes.Methods;
using PatternUsage.Attributes.Properties;
using PatternUsage.Attributes.Setters;

[assembly:PublicStatic]
[assembly:ChildOfInterfaceAB]
[module:ChildOfInterfaceAWidly]
[module:ReturnsDictionaryGeneric]
[module:ReturnsChildOfIDictionary]
[assembly: ReturnsChildOfDicOrList]
[assembly:AnyMethodGeneric]
[assembly:SingleMethodGeneric]
[assembly:DoubleMethodGeneric]
[module:AnyTypeGeneric]
[module:SingleTypeGeneric]
[module:DoubleTypeGeneric]
[module:AnyTypeGenericNoneMethodGeneric]
[module:NoneTypeGenericAnyMethodGeneric]
[assembly:SpecificNestedType]
[assembly:AnyDoubleDepthNestedType]