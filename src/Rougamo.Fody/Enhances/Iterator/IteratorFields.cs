using Mono.Cecil;
using System;
using System.Linq;

namespace Rougamo.Fody.Enhances.Iterator
{
    internal class IteratorFields : StateMachineFields, IIteratorFields
    {
        public IteratorFields(
            TypeDefinition stateMachineTypeDef,
            FieldDefinition? moArray, FieldDefinition[] mos,
            FieldDefinition methodContext,
            FieldDefinition state, FieldDefinition current,
            FieldDefinition initialThreadId, FieldDefinition? recordedReturn,
            FieldDefinition? declaringThis,
            FieldDefinition?[] transitParameters, FieldDefinition?[] parameters) : base(stateMachineTypeDef)
        {
            MoArray = MakeReference(moArray);
            Mos = mos.Select(x => MakeReference(x)!).ToArray();
            MethodContext = MakeReference(methodContext)!;
            State = MakeReference(state)!;
            Current = MakeReference(current)!;
            InitialThreadId = MakeReference(initialThreadId)!;
            RecordedReturn = MakeReference(recordedReturn);
            TransitParameters = transitParameters.Select(MakeReference).ToArray();
            Parameters = parameters.Select(MakeReference).ToArray();
            DeclaringThis = MakeReference(declaringThis);
        }

        public FieldReference? MoArray { get; }

        public FieldReference[] Mos { get; }

        public FieldReference MethodContext { get; }

        public FieldReference State { get; }

        public FieldReference Current { get; }

        public FieldReference InitialThreadId { get; }

        public FieldReference? RecordedReturn { get; }

        public FieldReference?[] TransitParameters { get; set; }

        public FieldReference?[] Parameters { get; }

        private FieldReference? _declaringThis;
        public FieldReference? DeclaringThis
        {
            get => _declaringThis;
            set
            {
                _declaringThis = value is FieldDefinition fd ? MakeReference(fd) : value;
            }
        }

        private FieldReference? _iterator;
        public FieldReference? Iterator
        {
            get => _iterator;
            set
            {
                _iterator = value is FieldDefinition fd ? MakeReference(fd) : value;
            }
        }

        public void SetParameter(int index, FieldDefinition fieldDef)
        {
            Parameters[index] = MakeReference(fieldDef);
        }
    }
}
