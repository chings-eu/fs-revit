namespace TYB.Tutorial.FSRevit

open Autodesk.Revit.UI
open Autodesk.Revit.DB
open Autodesk.Revit.Attributes

[<TransactionAttribute(TransactionMode.Manual)>]
type FitWorkset() as this =
    interface IExternalCommand with
        member x.Execute(cdata, msg, elset) =
            let uidoc = cdata.Application.ActiveUIDocument
            let selected = uidoc.Selection.GetElementIds() |> Seq.map uidoc.Document.GetElement |> List.ofSeq
            match uidoc.Document.IsWorkshared with
            | false ->
                msg <- "File is not workshared."
                Result.Cancelled
            | true ->
                let namWsTo = "02"
                let col = new FilteredWorksetCollector(uidoc.Document)
                let ws = col.ToWorksets() |> Seq.filter(fun w -> namWsTo = Array.item 0 (w.Name.Split '_')) |> List.ofSeq
                match ws with
                | [] ->
                    msg <- "No such keyword of workset"
                    Result.Cancelled
                | [ws] ->
                    let tblWorkset = uidoc.Document.GetWorksetTable()
                    match selected with
                    | [] -> 
                        tblWorkset.SetActiveWorksetId(ws.Id)
                    | _ ->
                        let bip = BuiltInParameter.ELEM_PARTITION_PARAM
                        let t = new Transaction(uidoc.Document, string this)
                        t.Start() |> ignore
                        selected |> List.iter(fun e -> (e.get_Parameter(bip)).Set(ws.Id.IntegerValue) |> ignore)
                        t.Commit() |> ignore
                    Result.Succeeded
                | _ ->
                    msg <- "Keyword refer to more than one workset"
                    Result.Cancelled