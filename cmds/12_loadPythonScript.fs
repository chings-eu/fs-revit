namespace TYB.Tutorial.FSRevit

open Autodesk.Revit.UI
open Autodesk.Revit.Attributes
open IronPython.Compiler
module loadPythonScript =
    let loader(cdata:ExternalCommandData)(pth:string) =
        let d = TYB.Lib.debug()
        let lst = [("Frames", true:>obj); ("FullFrames", true:>obj); ("LightweightScoped", true:>obj)]
        let dic = Seq.ofList lst |> dict

        // IronPython Engine
        let engine = IronPython.Hosting.Python.CreateEngine(options = dic)
        engine.Runtime.LoadAssembly(typeof<Autodesk.Revit.DB.Document>.Assembly)
        engine.Runtime.LoadAssembly(typeof<Autodesk.Revit.UI.Result>.Assembly)

        // Builtin Module
        let mdlBuiltin = IronPython.Hosting.Python.GetBuiltinModule(engine)
        mdlBuiltin.SetVariable("uiapp", cdata.Application)
        mdlBuiltin.SetVariable("__window__", 0)

        // Scope
        let scope = IronPython.Hosting.Python.CreateModule(engine, "__main__")
        scope.SetVariable("d", d)
        //scope.SetVariable("msg", msg)
        scope.SetVariable("res", Result.Succeeded)
        scope.SetVariable("__file__", 0)

        // Script
        let script = engine.CreateScriptSourceFromFile(path = pth, encoding = System.Text.Encoding.UTF8, kind = Microsoft.Scripting.SourceCodeKind.Statements)

        // Compile
        let optCompiler = engine.GetCompilerOptions(scope):?>PythonCompilerOptions
        optCompiler.ModuleName <- "__main__"
        optCompiler.Module <- IronPython.Runtime.ModuleOptions.Initialize

        // Command
        let command = script.Compile(optCompiler)

        scope, script, command

[<TransactionAttribute(TransactionMode.Manual)>]
type LoadPythonScript() =
    interface IExternalCommand with 
        member x.Execute(cdata, msg, elset) =
            let d = TYB.Lib.debug()
            let pth = @"C:\Dropbox\tailoryourrevit\Load\PythonScripts\pythonRunByFSharp.py"

            let scope, script, command = loadPythonScript.loader cdata pth
            match command with
            | null -> Result.Cancelled
            | _ ->
                try
                    script.Execute(scope)
                    Result.Succeeded
                with
                | :? System.Exception as exn ->
                    d.info exn.Message
                    Result.Cancelled
