.assembly extern netstandard
{
  .ver 2:0:0:0
  .publickeytoken = (CC 7B 13 FF CD 2D DD 51 ) // .{...-.Q
}

.assembly 'com.anatawa12.animator-controller-as-a-code.unsafe'
{
  // Attributes are based on result of C# Project
  .custom instance void class [netstandard]System.Runtime.CompilerServices.CompilationRelaxationsAttribute::'.ctor'(int32) =  (01 00 08 00 00 00 00 00 ) // ........

  .custom instance void class [netstandard]System.Runtime.CompilerServices.RuntimeCompatibilityAttribute::'.ctor'() =  (
		01 00 01 00 54 02 16 57 72 61 70 4E 6F 6E 45 78   // ....T..WrapNonEx
		63 65 70 74 69 6F 6E 54 68 72 6F 77 73 01       ) // ceptionThrows.
 
  .hash algorithm 0x00008004
  .ver  0:0:0:0
}

.module 'com.anatawa12.animator-controller-as-a-code.unsafe.dll'
.imagebase 0x00400000
.file alignment 0x00000200
.stackreserve 0x00100000
.subsystem 0x0003       // WINDOWS_CUI
.corflags 0x00000001    //  ILONLY

.class public abstract auto ansi sealed beforefieldinit Anatawa12.AnimatorControllerAsACode.Unsafe
       extends [netstandard]System.Object
{
  .method public hidebysig static !!TTo& As<TFrom, TTo>(!!TFrom& source) cil managed aggressiveinlining
  {
    .maxstack 1
    ldarg.0
    ret
  }

  .method public hidebysig static !!T& Add<T>(!!T& source, int32 elementOffset) cil managed aggressiveinlining
  {
    .maxstack 3
    ldarg.0
    ldarg.1
    sizeof !!T
    conv.i
    mul
    add
    ret
  }
}
