namespace TYB.Tutorial.FSRevit
open Autodesk.Revit.UI
open Autodesk.Revit.Attributes
open Autodesk.Revit.DB.Architecture

[<TransactionAttribute(TransactionMode.Manual)>]
type FindSumOfRoomArea() as this = 
    interface IExternalCommand with
        member x.Execute(cdata, msg, elset) =
            let uidoc = cdata.Application.ActiveUIDocument
            let selected =
                uidoc.Selection.GetElementIds() 
                |> Seq.map uidoc.Document.GetElement
                |> Seq.filter(
                    fun e ->
                        match e with
                        | :? Room -> true
                        | _ -> false
                ) |> Seq.cast<Room> |> List.ofSeq
            match selected with
            | [] -> 
                msg <- "Select Room(s)."
                Result.Cancelled
            | _ ->
                let txt =
                    let unitareas =
                        selected 
                        |> Seq.map(
                            fun rm -> 
                                fsMath.toCurrentUnits uidoc.Document 2.0 rm.Area
                        )
                    let uSys, area = unitareas |> Seq.head |> fun(u, _) -> u, unitareas |> Seq.sumBy(fun(_,a) -> a)
                    match uSys with
                    | fsMath.Metric ->
                        area |> fsMath.toRoundUp 4.0 |> string |> fun s -> s + " m²"
                    | fsMath.Imperial ->
                        area |> fsMath.toRoundUp 4.0 |> string |> fun s -> s + " sq. ft"
                TaskDialog.Show(string this, txt) |> ignore
                Result.Succeeded