namespace TYB.Tutorial.FSRevit

open Autodesk.Revit.UI
open Autodesk.Revit.UI.Selection
open Autodesk.Revit.DB
open Autodesk.Revit.Attributes

[<TransactionAttribute(TransactionMode.Manual)>]
type CreateDetailLines() = 
  interface IExternalCommand with
    member x.Execute(cdata, msg, elset) =
      let uiapp = cdata.Application
      let uidoc = uiapp.ActiveUIDocument
      let doc = uidoc.Document
      let threepoints = 
        [0..2]
        |> List.map(fun _ -> uidoc.Selection.PickPoint(ObjectSnapTypes.Intersections, "Select intersection point"))
      let ln02 = Line.CreateBound(threepoints.[0], threepoints.[2])
      let ln21 = Line.CreateBound(threepoints.[2], threepoints.[1])
      let pars = [0.0..0.1..1.0]
      let eval (ln:Curve)(par:float) = ln.Evaluate(par * ln.Length, false)
      let pts02 = pars |> List.map (eval ln02)
      let pts21 = pars |> List.map (eval ln21)
      let t = new Transaction(doc, "Create detaillines") 
      t.Start() |> ignore
      let detaillines =
        List.map2 (
          fun pa pb -> Line.CreateBound(pa, pb) 
          ) pts02 pts21
        |> List.map (
          fun (ln:Line) -> doc.Create.NewDetailCurve(uidoc.ActiveView, ln)
          )
      t.Commit() |> ignore
      Result.Succeeded


