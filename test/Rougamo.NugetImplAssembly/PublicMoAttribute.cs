using Rougamo.Context;
using System;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Rougamo.NugetImplAssembly
{
    public class PublicMoAttribute : MoAttribute
    {
        public PublicMoAttribute(int intValue, double[] doubleArray)
        {
            IntValue = intValue;
            DoubleArray = doubleArray;
        }

        public int IntValue { get; set; }

        public string StringValue { get; set; }

        public double[] DoubleArray { get; set; }

        public int[] IntArray { get; set; }

        public object ObjectValue { get; set; }

        public override AccessFlags Flags => AccessFlags.Public;

        public override void OnEntry(MethodContext context)
        {
            Console.WriteLine($@"[{this.GetType().FullName}.{nameof(OnEntry)}] Self=>
IntValue: {IntValue},
StringValue: {StringValue},
DoubleArray: {string.Join(",", DoubleArray ?? new double[0])},
IntArray: {string.Join(",", IntArray ?? new int[0])},
ObjectValue: {ObjectValue},
Flags: {Flags}");
            Console.WriteLine($@"[{this.GetType().FullName}.{nameof(OnEntry)}] context=>
Target: {context.Target},
TargetType: {context.TargetType},
Arguments: {JsonSerializer.Serialize(context.Arguments)}");
        }

        public override void OnException(MethodContext context)
        {
            Console.WriteLine($"[{this.GetType().FullName}.{nameof(OnException)}] {context.Exception}");
        }

        public override void OnExit(MethodContext context)
        {
            Console.WriteLine($"[{this.GetType().FullName}.{nameof(OnExit)}]");
        }

        public override void OnSuccess(MethodContext context)
        {
            Console.WriteLine($"[{this.GetType().FullName}.{nameof(OnSuccess)}] {context.HasReturnValue}<->{JsonSerializer.Serialize(context.ReturnValue)}");
        }
    }
}
