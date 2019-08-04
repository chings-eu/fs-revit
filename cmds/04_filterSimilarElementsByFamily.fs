namespace TYB.Tutorial.FSRevit
open Autodesk.Revit.UI
open Autodesk.Revit.Attributes
open Autodesk.Revit.DB
[<TransactionAttribute(TransactionMode.Manual)>]
type FilterSimilarElementsByFamily() =
    interface IExternalCommand with
        member x.Execute(cdata, msg, elset) =
            let uiapp = cdata.Application
            let uidoc = uiapp.ActiveUIDocument
            let doc = uidoc.Document
            // Get selected element(s)
            let selected = uidoc.Selection.GetElementIds() |> Seq.cast |> Seq.map (fun (eId:ElementId) -> doc.GetElement eId)
            // Get family name(s) of the selected element(s)
            let bip = BuiltInParameter.ALL_MODEL_FAMILY_NAME
            let namAndcat = 
                selected
                |> Seq.map(
                    fun(e:Element) ->
                    let typ = e.Document.GetElement (e.GetTypeId())
                    try
                        typ.get_Parameter(bip).AsString(), e.Category.Id
                    with
                    | :? System.NullReferenceException ->
                        "", e.Category.Id
                    )
                |> Seq.filter (fun(s:string, id:ElementId) -> s <> "" && id <> ElementId.InvalidElementId)
            // If element(s) in the active view
            let isSameView = true
            // If element(s) in group
            let isInGroup = true
            // Run
            match Seq.length selected with
                | x when x = 0 -> 
                    msg <- "Select Element(s) to have similar family(s) before running command"
                    Result.Failed
                | _ ->
                    // Use FilteredElementCollector
                    let col =
                        match isSameView with
                        | true -> new FilteredElementCollector(doc, uidoc.ActiveView.Id)
                        | false -> new FilteredElementCollector(doc)
                        |> fun c -> c.WhereElementIsNotElementType()
                        |> Seq.cast
                        |> match isInGroup with
                            | false -> Seq.filter (fun(e:Element) -> e.GroupId = ElementId.InvalidElementId)
                            | true -> Seq.filter (fun _ -> true)
                        |> Seq.filter (
                            fun e ->
                                let t = e.Document.GetElement(e.GetTypeId())
                                Seq.exists (
                                    fun (s, id) -> 
                                        try
                                        e.Category.Id = id &&
                                        t.get_Parameter(bip).AsString() = s
                                        with
                                        | :? System.NullReferenceException ->
                                        false
                                    ) namAndcat
                            )
                        |> Seq.map (fun e -> e.Id)
                    uidoc.Selection.SetElementIds(ResizeArray<ElementId> col)
                    Result.Succeeded