namespace TYB.Tutorial.FSRevit
open Autodesk.Revit.UI
open Autodesk.Revit.DB
open Autodesk.Revit.Attributes
type PurgeUnsedTypes() =
    interface IExternalCommand with
        override x.Execute(cdata, msg, elset) =

            let uidoc = cdata.Application.ActiveUIDocument
            let selected = 
                uidoc.Selection.GetElementIds()
                |> Seq.map(fun eid -> eid |> uidoc.Document.GetElement) 
                |> List.ofSeq

            let bic = BuiltInCategory.OST_StructuralFraming
            let eIdDel() =
                let c = new FilteredElementCollector(uidoc.Document)
                c.OfCategory(bic).WhereElementIsElementType()
                |> Seq.cast<ElementType>
                |> Seq.filter(
                    fun et ->
                        let col = new FilteredElementCollector(uidoc.Document)
                        col.OfCategory(bic).WhereElementIsNotElementType()
                        |> Seq.filter(fun e -> e.GetTypeId() = et.Id)
                        |> Seq.length = 0
                )
                |> Seq.map(fun et -> d.info et.Name; et.Id)
                |> List.ofSeq
            // delete unused family types
            let famsym =
                let c = new FilteredElementCollector(uidoc.Document)
                c.OfClass(typeof<FamilySymbol>).WhereElementIsNotElementType()
            let run() =
                eIdDel() |> ResizeArray |> uidoc.Document.Delete |> ignore
                ()
            try
                let t = new Transaction(uidoc.Document, "cmd" + system.datetime.dt())
                t.Start() |> ignore
                run()
                uidoc.Document.Regenerate()
                t.Commit() |> ignore
                Result.Succeeded
            with
            | :? System.Exception as exn ->
                d.info(exn.Message + "\n" + exn.StackTrace)
                Result.Cancelled