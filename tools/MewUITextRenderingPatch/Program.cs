using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;

try
{
    Patch(args);
    return 0;
}
catch (Exception error)
{
    Console.Error.WriteLine($"Islands text rendering patch failed: {error.Message}");
    return 1;
}

static void Patch(string[] args)
{
    if (args.Length != 3)
        throw new ArgumentException("Usage: MewUITextRenderingPatch <original DLL> <output DLL> <reference paths file>");

    var input = Path.GetFullPath(args[0]);
    var output = Path.GetFullPath(args[1]);
    if (string.Equals(input, output, OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal))
        throw new ArgumentException("The original package assembly must not be modified in place.");

    using var resolver = new DefaultAssemblyResolver();
    foreach (var directory in File.ReadAllLines(args[2]).Select(Path.GetDirectoryName).Distinct())
        if (!string.IsNullOrEmpty(directory)) resolver.AddSearchDirectory(directory);
    using var assembly = AssemblyDefinition.ReadAssembly(input, new ReaderParameters { AssemblyResolver = resolver });
    if (assembly.Name.Name != "Aprillz.MewUI.Backend.Direct2D" || assembly.Name.Version != new Version(0, 22, 1, 0))
        throw new InvalidOperationException($"Islands Windows text workaround only supports MewUI 0.22.1; resolved {assembly.Name.Name} {assembly.Name.Version}. Review the workaround when upgrading, or set IslandsWindowsTextRenderingWorkaround=false to disable it.");

    var module = assembly.MainModule;
    var factory = module.GetType("Aprillz.MewUI.Rendering.Direct2D.Direct2DGraphicsFactory");
    var context = module.GetType("Aprillz.MewUI.Rendering.Direct2D.Direct2DGraphicsContext");
    const string fieldName = "_islandsLowDpiTextRenderingParams";
    if (factory.Fields.Any(field => field.Name == fieldName))
        throw new InvalidOperationException("Expected the original NuGet assembly, not an already patched copy.");

    var originalParams = factory.Fields.Single(field => field.Name == "_textRenderingParams");
    var dwFactory = factory.Fields.Single(field => field.Name == "_dwriteFactory");
    var lowDpiParams = new FieldDefinition(fieldName, FieldAttributes.Private, module.TypeSystem.IntPtr);
    factory.Fields.Add(lowDpiParams);

    // The extra COM reference belongs to the graphics factory, just like the original rendering params.
    var createParams = module.GetType("Aprillz.MewUI.Native.DirectWrite.DWriteVTable").Methods
        .Single(method => method.Name == "CreateRenderingParams");
    var initialize = factory.Methods.Single(method => method.Name == "EnsureInitialized");
    initialize.Body.SimplifyMacros();
    var initStore = initialize.Body.Instructions.Single(instruction =>
        instruction.OpCode == OpCodes.Stfld && instruction.Operand == originalParams);
    var initIl = initialize.Body.GetILProcessor();
    var initNext = initStore.Next;
    foreach (var instruction in new[]
    {
        Instruction.Create(OpCodes.Ldarg_0),
        Instruction.Create(OpCodes.Ldfld, dwFactory),
        Instruction.Create(OpCodes.Ldarg_0),
        Instruction.Create(OpCodes.Ldflda, lowDpiParams),
        Instruction.Create(OpCodes.Call, createParams),
        Instruction.Create(OpCodes.Pop), // Failure leaves the zero-initialized field at zero; use original params.
    }) initIl.InsertBefore(initNext, instruction);

    var dispose = factory.Methods.Single(method => method.Name == "Dispose");
    dispose.Body.SimplifyMacros();
    var releaseOriginal = dispose.Body.Instructions.Single(instruction =>
        instruction.OpCode == OpCodes.Ldfld && instruction.Operand == originalParams);
    var release = (MethodReference)releaseOriginal.Next.Operand;
    var disposeIl = dispose.Body.GetILProcessor();
    var disposeBefore = releaseOriginal.Previous;
    foreach (var instruction in new[]
    {
        Instruction.Create(OpCodes.Ldarg_0),
        Instruction.Create(OpCodes.Ldfld, lowDpiParams),
        Instruction.Create(OpCodes.Call, release),
        Instruction.Create(OpCodes.Pop),
        Instruction.Create(OpCodes.Ldarg_0),
        Instruction.Create(OpCodes.Ldc_I4_0),
        Instruction.Create(OpCodes.Conv_I),
        Instruction.Create(OpCodes.Stfld, lowDpiParams),
    }) disposeIl.InsertBefore(disposeBefore, instruction);

    // Keep the original NATURAL_SYMMETRIC params above 96 DPI. At 100%, use the Windows defaults
    // verified by the native font probe. Choose per render target, including popups and cached surfaces.
    var select = new MethodDefinition("IslandsSelectTextRenderingParams", MethodAttributes.Assembly | MethodAttributes.HideBySig,
        module.TypeSystem.IntPtr);
    select.Parameters.Add(new ParameterDefinition("dpiScale", ParameterAttributes.None, module.TypeSystem.Double));
    factory.Methods.Add(select);
    var il = select.Body.GetILProcessor();
    var useOriginal = Instruction.Create(OpCodes.Ldarg_0);
    foreach (var instruction in new[]
    {
        Instruction.Create(OpCodes.Ldarg_1),
        Instruction.Create(OpCodes.Ldc_R8, 1d),
        Instruction.Create(OpCodes.Bgt_Un, useOriginal),
        Instruction.Create(OpCodes.Ldarg_0),
        Instruction.Create(OpCodes.Ldfld, lowDpiParams),
        Instruction.Create(OpCodes.Brfalse, useOriginal),
        Instruction.Create(OpCodes.Ldarg_0),
        Instruction.Create(OpCodes.Ldfld, lowDpiParams),
        Instruction.Create(OpCodes.Ret),
        useOriginal,
        Instruction.Create(OpCodes.Ldfld, originalParams),
        Instruction.Create(OpCodes.Ret),
    }) il.Append(instruction);

    var configure = context.Methods.Single(method => method.Name == "ConfigureRenderTargetForFrame");
    configure.Body.SimplifyMacros();
    var getParams = configure.Body.Instructions.Single(instruction =>
        instruction.Operand is MethodReference method && method.Name == "get_TextRenderingParams");
    var configureIl = configure.Body.GetILProcessor();
    configureIl.InsertBefore(getParams, Instruction.Create(OpCodes.Ldarg_0));
    configureIl.InsertBefore(getParams, Instruction.Create(OpCodes.Ldfld, context.Fields.Single(field => field.Name == "_dpiScale")));
    getParams.OpCode = OpCodes.Call;
    getParams.Operand = select;

    foreach (var method in new[] { initialize, dispose, configure, select })
        method.Body.OptimizeMacros();

    Directory.CreateDirectory(Path.GetDirectoryName(output)!);
    assembly.Write(output);
    Console.WriteLine("MewUI 0.22.1 workaround: system text params at 96 DPI; original smoothing above 96 DPI.");
}
