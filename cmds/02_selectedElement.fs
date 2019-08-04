namespace TYB.Tutorial.FSRevit

open Autodesk.Revit.Attributes
open Autodesk.Revit.UI
open Autodesk.Revit.DB

[<TransactionAttribute(TransactionMode.Manual)>]
type SelectedElement() = 
    interface IExternalCommand with
        member this.Execute(cdata:ExternalCommandData, msg, elset) =
            let uiapp = cdata.Application
            let uidoc = uiapp.ActiveUIDocument
            let selected = 
                uidoc.Selection.GetElementIds() |> Seq.cast
                |> Seq.map (fun(eid:ElementId) -> uidoc.Document.GetElement(eid))
            let msg =
                selected
                |> Seq.map (fun(e:Element) -> e.Name)
                |> String.concat "\n"
            TaskDialog.Show("Title", msg) |> ignore
            Result.Succeeded