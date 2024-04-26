using Mono.Cecil;
using System.Linq;

namespace Rougamo.Fody.Enhances.AsyncIterator
{
    internal class AiteratorFields : StateMachineFields, IIteratorFields
    {
        public AiteratorFields(
            TypeDefinition stateMachineTypeDef,
            FieldDefinition? moArray, FieldDefinition[] mos,
            FieldDefinition methodContext,
            FieldDefinition state, FieldDefinition current,
            FieldDefinition initialThreadId, FieldDefinition disposed,
            FieldDefinition builder, FieldDefinition promise,
            FieldDefinition? recordedReturn, FieldDefinition?[] transitParameters,
            FieldDefinition?[] parameters) : base(stateMachineTypeDef)
        {
            MoArray = MakeReference(moArray);
            Mos = mos.Select(x => MakeReference(x)!).ToArray();
            MethodContext = MakeReference(methodContext)!;
            State = MakeReference(state)!;
            Current = MakeReference(current)!;
            InitialThreadId = MakeReference(initialThreadId)!;
            Disposed = MakeReference(disposed)!;
            Builder = MakeReference(builder)!;
            Promise = MakeReference(promise)!;
            RecordedReturn = MakeReference(recordedReturn);
            TransitParameters = transitParameters.Select(MakeReference).ToArray();
            Parameters = parameters.Select(MakeReference).ToArray();
        }

        public FieldReference? MoArray { get; }

        public FieldReference[] Mos { get; }

        public FieldReference MethodContext { get; }

        public FieldReference State { get; }

        public FieldReference Current { get; }

        public FieldReference InitialThreadId { get; }

        public FieldReference Disposed { get; }

        public FieldReference Builder { get; }

        public FieldReference Promise { get; }

        public FieldReference? RecordedReturn { get; }

        public FieldReference?[] TransitParameters { get; set; }

        public FieldReference?[] Parameters { get; }

        private FieldReference? _declaringThis;
        public FieldReference? DeclaringThis
        {
            get => _declaringThis;
            set => _declaringThis = value is FieldDefinition fd ? MakeReference(fd) : value;
        }

        private FieldReference? _awaiter;
        public FieldReference? Awaiter {
            get => _awaiter;
            set => _awaiter = value is FieldDefinition fd ? MakeReference(fd) : value;
        }

        private FieldReference? _iterator;
        public FieldReference? Iterator
        {
            get => _iterator;
            set => _iterator = value is FieldDefinition fd ? MakeReference(fd) : value;
        }

        public void SetParameter(int index, FieldDefinition fieldDef)
        {
            throw new System.NotImplementedException();
        }
    }
}
