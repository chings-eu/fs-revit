namespace TYB.Tutorial.FSRevit
open Autodesk.Revit.UI
open Autodesk.Revit.Attributes
open Autodesk.Revit.DB

[<Regeneration(RegenerationOption.Manual)>]
[<TransactionAttribute(TransactionMode.Manual)>]
type purgeDuplicatedFillpattern() =
    interface IExternalCommand with
        override x.Execute(cdata, msg, elset) =
            let uidoc = cdata.Application.ActiveUIDocument
            let nam = "Solid fill"
            let pats =
                let c = new FilteredElementCollector(uidoc.Document)
                c.OfClass(typeof<FillPatternElement>).WhereElementIsNotElementType()
                |> Seq.cast<FillPatternElement>
                |> Seq.filter(fun fpe -> fpe.Name.StartsWith nam)
                |> Seq.sortBy(fun fpe -> fpe.Name)
                |> List.ofSeq
            let pat, dups =
                match pats with
                | [] -> failwith (sprintf "No fillpattern %s in this document." nam)
                | h::t -> h, t

            let run() =
                dups
                |> List.map(fun pt -> pt.SetFillPattern(pat.GetFillPattern()))
                |> ignore

                ()
            try
                let t = new Transaction(uidoc.Document, "cmd" + TYB.Lib.fs.dt())
                t.Start() |> ignore
                run()
                uidoc.Document.Regenerate()
                t.Commit() |> ignore
                Result.Succeeded
            with
            | :? System.Exception as exn ->
                msg <- exn.Message + "\n" + exn.StackTrace
                Result.Cancelled