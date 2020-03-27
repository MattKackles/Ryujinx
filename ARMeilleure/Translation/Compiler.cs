using ARMeilleure.CodeGen;
using ARMeilleure.CodeGen.X86;
using ARMeilleure.Diagnostics;
using ARMeilleure.IntermediateRepresentation;
using System;
using System.Runtime.InteropServices;

namespace ARMeilleure.Translation
{
    using PTC;

    static class Compiler
    {
        public static T Compile<T>(
            ControlFlowGraph cfg,
            OperandType[]    argTypes,
            OperandType      retType,
            CompilerOptions  options,
            PtcInfo          ptcInfo = null)
        {
            CompiledFunction func = CompileAndGetCf(cfg, argTypes, retType, options, ptcInfo);

            IntPtr codePtr = JitCache.Map(func);

            return Marshal.GetDelegateForFunctionPointer<T>(codePtr);
        }

        public static CompiledFunction CompileAndGetCf(
            ControlFlowGraph cfg,
            OperandType[]    argTypes,
            OperandType      retType,
            CompilerOptions  options,
            PtcInfo          ptcInfo = null)
        {
            Logger.StartPass(PassName.Dominance);

            Dominance.FindDominators(cfg);
            Dominance.FindDominanceFrontiers(cfg);

            Logger.EndPass(PassName.Dominance);

            Logger.StartPass(PassName.SsaConstruction);

            if ((options & CompilerOptions.SsaForm) != 0)
            {
                Ssa.Construct(cfg);
            }
            else
            {
                RegisterToLocal.Rename(cfg);
            }

            Logger.EndPass(PassName.SsaConstruction, cfg);

            CompilerContext cctx = new CompilerContext(cfg, argTypes, retType, options);

            return CodeGenerator.Generate(cctx, ptcInfo);
        }
    }
}