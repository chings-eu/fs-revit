namespace TYB.Tutorial.FSRevit
open Autodesk.Revit.Attributes
open Autodesk.Revit.UI
open Autodesk.Revit.DB
open System
[<TransactionAttribute(TransactionMode.Manual)>]
type FitColorOfOptionElement() as this =
    interface IExternalCommand with
        member x.Execute(cdata, msg, elset) =
            let d = TYB.Lib.debug()
            let uidoc = cdata.Application.ActiveUIDocument
            let selected =
                let col = new FilteredElementCollector(uidoc.Document, uidoc.ActiveView.Id)
                col.WhereElementIsNotElementType()
                |> Seq.filter(
                    fun e -> 
                        try
                            match e.DesignOption.Id.IntegerValue with
                            | x when x = ElementId.InvalidElementId.IntegerValue -> false
                            | _ -> true
                        with
                        | :? NullReferenceException -> false
                )
                |> Seq.map(
                    fun e -> e.DesignOption, e
                )
                |> Seq.sortBy(fun(o, _) -> o.Id.IntegerValue)
            match selected |> List.ofSeq with
            | [] ->
                msg <- "No DesignOption in this document"
                Result.Cancelled
            | _ ->
                let options =
                    selected |> Seq.map(fun(o, _) -> o) |> Seq.distinctBy(fun o -> o.Id.IntegerValue)
                let colors =
                    options 
                    |> Seq.indexed
                    |> Seq.map(
                        fun(i, o) -> 
                            let ratio = float i / (float (Seq.length options))
                            let color = new Color(byte (int(255.0 * ratio)), byte 128, byte 250)
                            let ogs = new OverrideGraphicSettings()
                            ogs.SetCutLineColor(color) |> ignore
                            ogs.SetCutFillColor(color) |> ignore
                            ogs.SetProjectionLineColor(color) |> ignore
                            ogs.SetProjectionFillColor(color) |> ignore
                            //d.info (string o.Name + string color)
                            o, ogs
                    )
                let t = new Transaction(uidoc.Document, string this)
                t.Start() |> ignore
                selected
                |> Seq.iter(
                    fun(o, e) ->
                        let _, ogs = 
                            colors 
                            |> Seq.find(fun(opt, _) -> opt.Id.IntegerValue = o.Id.IntegerValue)
                        uidoc.ActiveView.SetElementOverrides(e.Id, ogs)
                )
                t.Commit() |> ignore
                Result.Succeeded