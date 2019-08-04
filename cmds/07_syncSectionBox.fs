namespace TYB.Tutorial.FSRevit
open Autodesk.Revit.Attributes
open Autodesk.Revit.UI
open Autodesk.Revit.DB
[<TransactionAttribute(TransactionMode.Manual)>]
type SyncSectionBox() as this =
    interface IExternalCommand with
        member x.Execute(cdata, msg, elset) =
            let uidoc = cdata.Application.ActiveUIDocument
            let vAct = uidoc.ActiveView
            let bbxAct = (vAct:?>View3D).GetSectionBox()
            let uiv3ds = 
                uidoc.GetOpenUIViews()
                |> Seq.filter(fun uiv -> uiv.ViewId <> vAct.Id)
                |> Seq.filter(
                    fun uiv -> 
                    let view = uiv.ViewId |> uidoc.Document.GetElement
                    match view with
                    | :? View3D -> true
                    | _ -> false
                ) |> List.ofSeq
            match uiv3ds with
            | [] ->
                msg <- "No other opened 3d view to sync."
                Result.Cancelled
            | _ ->
                let t = new Transaction(uidoc.Document, string this)
                t.Start() |> ignore
                uiv3ds
                |> Seq.iter(
                    fun uiv ->
                        let view = uiv.ViewId |> uidoc.Document.GetElement
                        (view:?>View3D).SetSectionBox(bbxAct)
                        uiv.ZoomToFit()
                )
                t.Commit() |> ignore
                Result.Succeeded