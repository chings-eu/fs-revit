namespace TYB.Tutorial.FSRevit
open Autodesk.Revit.UI
open Autodesk.Revit.DB
open Autodesk.Revit.Attributes
[<TransactionAttribute(TransactionMode.Manual)>]
type ViewRoomReference() as this = 
    interface IExternalCommand with
        member x.Execute(cdata, msg, elset) =
            let d = TYB.Lib.debug()
            let uidoc = cdata.Application.ActiveUIDocument
                
            let catRoom = 
                uidoc.Document.Settings.Categories
                |> Seq.cast<Category>
                |> Seq.filter(fun cat -> cat.Name = "Rooms")
                |> Seq.head
            let subcats =
                catRoom.SubCategories |> Seq.cast<Category>
                |> Seq.filter(
                    fun subcat ->
                        subcat.Name = "Reference" ||
                        subcat.Name = "Interior Fill"
                )
            match uidoc.ActiveView with
            | :? View3D ->
                msg <- "3D View"
                Result.Cancelled
            | _ ->
                let catAll = [catRoom] @ (subcats |> List.ofSeq)
                let isStat = catAll |> List.map(fun cat -> uidoc.ActiveView.GetCategoryHidden(cat.Id))
                let toggle() =
                    match isStat with
                    | [false; false; false] ->
                        catAll |> List.iter(fun cat -> uidoc.ActiveView.SetCategoryHidden(cat.Id, true))
                    | _ ->
                        catAll |> List.iter(fun cat -> uidoc.ActiveView.SetCategoryHidden(cat.Id, false))
                let t = new Transaction(uidoc.Document, string this)
                t.Start() |> ignore
                match uidoc.ActiveView.ViewTemplateId with
                | x when x = ElementId.InvalidElementId ->
                    toggle()
                | _ ->
                    match uidoc.ActiveView.IsTemporaryViewPropertiesModeEnabled() with
                    | true ->
                        uidoc.ActiveView.DisableTemporaryViewMode(TemporaryViewMode.TemporaryViewProperties)
                    | false ->
                        uidoc.ActiveView.EnableTemporaryViewPropertiesMode(uidoc.ActiveView.ViewTemplateId) |> ignore
                        toggle()
                        
                t.Commit() |> ignore
                Result.Succeeded