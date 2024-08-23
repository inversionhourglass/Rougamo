using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using System.Collections.Generic;
using System.Linq;

namespace Mono.Cecil
{
    public static class OptimizeExtensions
    {
        private static readonly Dictionary<Code, OpCode> _OptimizeCodes = new()
        {
            { Code.Leave_S, OpCodes.Leave }, { Code.Br_S, OpCodes.Br },
            { Code.Brfalse_S, OpCodes.Brfalse }, { Code.Brtrue_S, OpCodes.Brtrue },
            { Code.Beq_S, OpCodes.Beq }, { Code.Bne_Un_S, OpCodes.Bne_Un },
            { Code.Bge_S, OpCodes.Bge }, { Code.Bgt_S, OpCodes.Bgt },
            { Code.Ble_S, OpCodes.Ble }, { Code.Blt_S, OpCodes.Blt },
            { Code.Bge_Un_S, OpCodes.Bge_Un }, { Code.Bgt_Un_S, OpCodes.Bgt_Un },
            { Code.Ble_Un_S, OpCodes.Ble_Un }, { Code.Blt_Un_S, OpCodes.Blt_Un }
        };
        public static void OptimizePlus(this MethodBody body, Instruction[]? nops = null)
        {
            nops ??= [];

            body.OptimizeShort();
            body.OptimizeUselessBr();
            body.OptimizeNops(nops);
            body.Optimize();
        }

        private static void OptimizeShort(this MethodBody body)
        {
            foreach (var instruction in body.Instructions)
            {
                if (_OptimizeCodes.TryGetValue(instruction.OpCode.Code, out var opcode))
                {
                    instruction.OpCode = opcode;
                }
            }
        }

        private static void OptimizeUselessBr(this MethodBody body)
        {
            var cannotRemoves = new List<Instruction>();
            var canRemoves = new List<Instruction>();
            foreach (var instruction in body.Instructions)
            {
                if (instruction.Operand is Instruction br2)
                {
                    cannotRemoves.Add(br2);
                    if (instruction.OpCode.Code == Code.Br)
                    {
                        var current = instruction.Next;
                        while (current != br2)
                        {
                            if (current.OpCode.Code != Code.Nop) break;
                            current = current.Next;
                        }
                        if (current == br2) canRemoves.Add(instruction);
                    }
                }
            }
            foreach (var handler in body.ExceptionHandlers)
            {
                cannotRemoves.Add(handler.TryStart);
                cannotRemoves.Add(handler.TryEnd);
                cannotRemoves.Add(handler.HandlerStart);
                cannotRemoves.Add(handler.HandlerEnd);
            }
            foreach (var item in canRemoves)
            {
                if (cannotRemoves.Contains(item)) continue;

                body.Instructions.Remove(item);
            }
        }

        private static void OptimizeNops(this MethodBody body, Instruction[] nops)
        {
            foreach (var handler in body.ExceptionHandlers)
            {
                if (NeedUpdateHandlerAnchor(handler.TryStart, handler.TryEnd, nops, out var newTryStart))
                {
                    handler.TryStart = newTryStart;
                }
                if (handler.TryEnd != handler.HandlerStart)
                {
                    if (NeedUpdateHandlerAnchor(handler.TryEnd, handler.HandlerStart, nops, out var newTryEnd))
                    {
                        handler.TryEnd = newTryEnd;
                    }
                    if (NeedUpdateHandlerAnchor(handler.HandlerStart, handler.HandlerEnd, nops, out var newHandlerStart))
                    {
                        handler.HandlerStart = newHandlerStart;
                    }
                }
                else
                {
                    if (NeedUpdateHandlerAnchor(handler.TryEnd, handler.HandlerEnd, nops, out var newTryEndHandlerStart))
                    {
                        handler.TryEnd = newTryEndHandlerStart;
                        handler.HandlerStart = newTryEndHandlerStart;
                    }
                }
                if (NeedUpdateHandlerAnchor(handler.HandlerEnd, null, nops, out var newHandlerEnd))
                {
                    handler.HandlerEnd = newHandlerEnd;
                }
            }

            foreach (var instruction in body.Instructions)
            {
                if (instruction.Operand is Instruction to)
                {
                    var current = to;
                    while (nops.Contains(current))
                    {
                        current = current.Next;
                    }
                    if (current != to && current != null)
                    {
                        instruction.Operand = current;
                    }
                }
            }
            foreach (var nop in nops)
            {
                body.Instructions.Remove(nop);
            }
        }

        private static bool NeedUpdateHandlerAnchor(Instruction? target, Instruction? checkLimit, Instruction[] nops, out Instruction? updateTarget)
        {
            updateTarget = null;
            if (target == null) return false;

            var current = target;
            while (current != checkLimit)
            {
                if (!nops.Contains(current)) break;
                current = current.Next;
            }
            if (current == checkLimit || current == target)
            {
                return false;
            }
            updateTarget = current;
            return true;
        }
    }
}
