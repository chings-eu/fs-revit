namespace TYB.Tutorial.FSRevit
open Autodesk.Revit.Attributes
open Autodesk.Revit.UI
open Autodesk.Revit.DB
[<TransactionAttribute(TransactionMode.Manual)>]
type FilterSelectedElementsByCategory() =
    interface IExternalCommand with
        member x.Execute(cdata, msg, elset) =
            let uidoc = cdata.Application.ActiveUIDocument
            let selected = uidoc.Selection.GetElementIds() |> Seq.map uidoc.Document.GetElement |> List.ofSeq
            match selected with
            | [] ->
                msg <- "Select Element(s)"
                Result.Cancelled
            | _ ->
                let idcats =
                    selected |> List.map(fun sel -> sel.Category.Id)
                let intIds =
                    idcats |> List.map(fun id -> id.IntegerValue) |> List.distinct
                let idPicked =
                    let strCats = selected |> Seq.map string |> Seq.map(fun str -> Array.last (str.Split '.')) |> String.concat ", "
                    let promp = 
                        sprintf "Pick elements to filter by category: %s." strCats
                    uidoc.Selection.PickElementsByRectangle(promp)
                    |> Seq.filter(
                        fun e -> List.contains e.Category.Id.IntegerValue intIds
                    )
                    |> Seq.map(fun e -> e.Id)
                uidoc.Selection.SetElementIds(ResizeArray<ElementId>(idPicked))
                Result.Succeeded