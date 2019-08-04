namespace TYB.Tutorial.FSRevit

open Autodesk.Revit.UI
open Autodesk.Revit.Attributes
open Autodesk.Revit.DB.Architecture
open Autodesk.Revit.DB

[<TransactionAttribute(TransactionMode.Manual)>]
type CreateFloorByRoom() as this =
    interface IExternalCommand with
        member x.Execute(cdata, msg, elset) =
            let uiapp = cdata.Application
            let uidoc = uiapp.ActiveUIDocument
            let selected = 
                uidoc.Selection.GetElementIds() 
                |> Seq.map uidoc.Document.GetElement 
                |> Seq.filter(
                    fun e -> 
                        match e with
                        | :? Room -> true
                        | _ -> false
                ) |> List.ofSeq
            match selected with
            | [] -> 
                msg <- "Select Room(s)"
                Result.Cancelled
            | _ as sel ->
                let t = new Transaction(uidoc.Document, string this)
                t.Start() |> ignore
                sel |> Seq.cast<Room> |> List.ofSeq
                |> List.map(
                    fun rm ->
                        let curvearray =
                            rm.GetBoundarySegments(new SpatialElementBoundaryOptions()) 
                            |> Seq.map(
                                fun seqSeg ->  
                                    let cary = new CurveArray()
                                    seqSeg |> Seq.iter(fun sg -> cary.Append (sg.GetCurve()))
                                    cary
                            ) |> List.ofSeq
                        match curvearray with
                        | [] -> failwith "Unknown error with the room"
                        | [cary] ->
                            uidoc.Document.Create.NewFloor(cary, false) |> ignore
                            ()
                        | hd::tl ->
                            let flr = uidoc.Document.Create.NewFloor(hd, false)
                            tl
                            |> List.iter(fun cary -> uidoc.Document.Create.NewOpening(flr, cary, false) |> ignore)
                            ()
                )
                |> ignore
                t.Commit() |> ignore
                Result.Succeeded