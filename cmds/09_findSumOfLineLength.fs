namespace TYB.Tutorial.FSRevit
open Autodesk.Revit.UI
open Autodesk.Revit.DB
open Autodesk.Revit.Attributes
[<TransactionAttribute(TransactionMode.Manual)>]
type FindSumOfLineLength() as this = 
    interface IExternalCommand with
        member x.Execute(cdata, msg, elset) =
            let uidoc = cdata.Application.ActiveUIDocument
            let selected =
                uidoc.Selection.GetElementIds() 
                |> Seq.map uidoc.Document.GetElement
                |> Seq.filter(
                    fun e ->
                        match e with
                        | :? CurveElement -> true
                        | _ -> false
                ) |> Seq.cast<CurveElement> |> List.ofSeq
            match selected with
            | [] -> 
                msg <- "Select line(s)."
                Result.Cancelled
            | _ ->
                let txt =
                    let lens =
                        selected 
                        |> Seq.map(
                            fun ce -> fsMath.toCurrentUnits uidoc.Document 1.0 ce.GeometryCurve.Length
                        )
                    let uSys, sum =
                        lens |> Seq.head |> (fun(u, _) -> u), lens |> Seq.sumBy(fun (_, l) -> l)
                    match uSys with
                    | fsMath.Metric ->
                        sum |> fsMath.toRoundUp 4.0 |> string |> fun x -> x + " m"
                    | fsMath.Imperial ->
                        sum |> fsMath.toRoundUp 4.0 |> string |> fun x -> x + " ft"
                TaskDialog.Show(string this, txt) |> ignore
                Result.Succeeded
