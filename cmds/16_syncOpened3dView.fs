namespace TYB.Tutorial.FSRevit
open Autodesk.Revit.UI
open Autodesk.Revit.Attributes
open Autodesk.Revit.DB
[<TransactionAttribute(TransactionMode.Manual)>]
type SyncAllOpened3dView() =
    interface IExternalCommand with
        member x.Execute(cdata, msg, elset) =
            let uidoc = cdata.Application.ActiveUIDocument
            let run () =
                let uivs = uidoc.GetOpenUIViews() |> Seq.cast<UIView>
                let v3ds = 
                    uivs 
                    |> Seq.map(fun uiv -> uiv, uiv.ViewId |> uidoc.Document.GetElement :?> View3D)
                    |> Seq.filter(fun (_, v) -> v.Id <> uidoc.ActiveView.Id)
                    |> Seq.filter(
                        fun (_, v) -> 
                            match v with
                            | :? View3D -> true
                            | _ -> false
                    )
                match uidoc.ActiveView with
                | :? View3D as vact ->
                    let sb = vact.GetSectionBox()
                    v3ds
                    |> Seq.iter(
                        fun (uiv, v3d) -> 
                            v3d.SetSectionBox(sb)
                            uiv.ZoomToFit()
                    )
                | _ -> failwith "active view is not 3d view"
                ()
            try
                let t = new Transaction(uidoc.Document, "cmd" + TYB.Lib.fs.dt())
                t.Start() |> ignore
                run()
                uidoc.Document.Regenerate()
                t.Commit() |> ignore
                Result.Succeeded
            with
            | :? System.Exception as exn ->
                msg <- exn.Message + "\n" + exn.StackTrace
                Result.Cancelled