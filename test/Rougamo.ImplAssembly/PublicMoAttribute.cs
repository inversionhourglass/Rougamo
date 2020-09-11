using Rougamo.Context;
using System;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Rougamo.ImplAssembly
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
            Debug.Print($@"[{this.GetType().FullName}.{nameof(OnEntry)}] Self=>
IntValue: {IntValue},
StringValue: {StringValue},
DoubleArray: {string.Join(",", DoubleArray)},
IntArray: {string.Join(",", IntArray)},
ObjectValue: {ObjectValue},
Flags: {Flags}");
            Debug.Print($@"[{this.GetType().FullName}.{nameof(OnEntry)}] context=>
Target: {context.Target},
TargetType: {context.TargetType},
Arguments: {JsonSerializer.Serialize(context.Arguments)}");
        }

        public override void OnException(MethodContext context)
        {
            Debug.Print($"[{this.GetType().FullName}.{nameof(OnException)}] {context.Exception}");
        }

        public override void OnExit(MethodContext context)
        {
            Debug.Print($"[{this.GetType().FullName}.{nameof(OnExit)}]");
        }

        public override void OnSuccess(MethodContext context)
        {
            Debug.Print($"[{this.GetType().FullName}.{nameof(OnSuccess)}] {JsonSerializer.Serialize(context.ReturnValue)}");
        }
    }
}
