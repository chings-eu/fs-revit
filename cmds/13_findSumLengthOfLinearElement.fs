namespace TYB.Tutorial.FSRevit
open Autodesk.Revit.UI
open Autodesk.Revit.DB
open Autodesk.Revit.Attributes
[<TransactionAttribute(TransactionMode.Manual)>]
type FindSumLengthOfLinearElement() as this = 
    interface IExternalCommand with
        member x.Execute(cdata, msg, elset) =
            let uidoc = cdata.Application.ActiveUIDocument
            let selected =
                uidoc.Selection.GetElementIds() 
                |> Seq.map uidoc.Document.GetElement
                |> Seq.map(
                    fun e ->
                        match e with
                        | :? CurveElement as ce -> Some ce.GeometryCurve
                        | :? Wall as w -> Some (w.Location:?>LocationCurve).Curve
                        | :? FamilyInstance as fi ->
                            match fi.Category.Id.IntegerValue with
                            | x when x = int BuiltInCategory.OST_StructuralFraming -> 
                                Some (fi.Location:?>LocationCurve).Curve
                            | _ -> None
                        | _ -> None
                )
                |> Seq.filter(
                    fun opt -> opt.IsSome
                ) |> List.ofSeq
            match selected with
            | [] -> 
                msg <- "Select line(s) / Wall(s)."
                Result.Cancelled
            | _ ->
                let unitlen =
                    selected
                    |> List.map(fun opt -> opt.Value.Length)
                    |> List.sum
                    |> fsMath.toCurrentUnits uidoc.Document 1.0
                let txt =
                    let uSys, sum = unitlen
                    match uSys with
                    | fsMath.Metric ->
                        sum |> fsMath.toRoundUp 4.0 |> string |> fun x -> x + " m"
                    | fsMath.Imperial ->
                        sum |> fsMath.toRoundUp 4.0 |> string |> fun x -> x + " ft"
                TaskDialog.Show(string this, string txt) |> ignore
                Result.Succeeded