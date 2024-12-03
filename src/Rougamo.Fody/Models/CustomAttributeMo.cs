using Fody;
using Fody.Simulations;
using Fody.Simulations.PlainValues;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Rougamo.Fody.Simulations.Types;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Rougamo.Fody.Models
{
    internal class CustomAttributeMo(CustomAttribute attribute, MoFrom from) : Mo(attribute.AttributeType, from)
    {
        public override bool IsStruct => false;

        public CustomAttribute Attribute => attribute;

        public override bool HasArguments => attribute.HasConstructorArguments;

        public override bool HasProperties => attribute.HasProperties;

        public override IList<Instruction> New(TsMo tMo, MethodSimulation host)
        {
            var ctor = Attribute.Constructor.Simulate(tMo);
            var arguments = Attribute.ConstructorArguments.Select(x => x.Simulate(tMo.ModuleWeaver)).ToArray();
            return ctor.Call(host, arguments);
        }

        protected override AccessFlags ExtractFlags()
        {
            if (Attribute.AttributeType.Implement(Constants.TYPE_IFlexibleModifierPointcut) && Attribute.Properties.TryGet(Constants.PROP_Flags, out var property))
            {
                return (AccessFlags)Convert.ToInt32(property!.Value.Argument.Value);
            }

            return base.ExtractFlags();
        }

        protected override string? ExtractPattern()
        {
            if (Attribute.AttributeType.Implement(Constants.TYPE_IFlexiblePatternPointcut) && Attribute.Properties.TryGet(Constants.PROP_Pattern, out var property))
            {
                return (string)property!.Value.Argument.Value;
            }

            return base.ExtractPattern();
        }

        protected override double ExtractOrder()
        {
            if (Attribute.AttributeType.Implement(Constants.TYPE_IFlexibleOrderable) && Attribute.Properties.TryGet(Constants.PROP_Order, out var property))
            {
                return (double)property!.Value.Argument.Value;
            }

            return base.ExtractOrder();
        }
    }
}
