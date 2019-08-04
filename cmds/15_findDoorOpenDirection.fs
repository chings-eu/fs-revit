namespace TYB.Tutorial.FSRevit
open Autodesk.Revit.UI
open Autodesk.Revit.Attributes
open Autodesk.Revit.DB
type OpenDirection =
    | Left
    | Right
    member x.ops = 
        match x with
        | Left -> Right
        | Right -> Left
[<TransactionAttribute(TransactionMode.Manual)>]
type FindDoorOpenDirection() as this =
    interface IExternalCommand with
        member x.Execute(cdata, msg, elset) =
            let uidoc = cdata.Application.ActiveUIDocument
            let dir = Left
            let selected =
                uidoc.Selection.GetElementIds() |> Seq.map uidoc.Document.GetElement
                |> Seq.filter(
                    fun s ->
                        match s with
                        | :? FamilyInstance -> true
                        | _ -> false
                ) 
                |> Seq.filter(
                    fun s ->
                        match s.Category.Id.IntegerValue with
                        | x when x = int BuiltInCategory.OST_Doors -> true
                        | _ -> false
                ) |> List.ofSeq
            match selected with
            | [] -> msg <- "Select door(s)"; Result.Cancelled
            | _ ->
                let t = new Transaction(uidoc.Document, string this)
                t.Start() |> ignore
                selected |> Seq.cast<FamilyInstance>
                |> Seq.map(
                    fun dr ->
                        let opendir = 
                            match dr.HandFlipped, dr.FacingFlipped with
                            | true, true | false, false -> dir
                            | _, _ -> dir.ops
                        let par = dr.get_Parameter(BuiltInParameter.DOOR_NUMBER)
                        par.Set(par.AsString() + " | " + string opendir)
                ) |> List.ofSeq |> ignore
                t.Commit() |> ignore
                Result.Succeeded