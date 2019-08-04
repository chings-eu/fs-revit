namespace TYB.Tutorial.FSRevit

open Autodesk.Revit.Attributes
open Autodesk.Revit.UI

[<TransactionAttribute(TransactionMode.Manual)>]
type ASimpleMessage() =
    interface IExternalCommand with
        member x.Execute(cdata, msg, elset) =

            TaskDialog.Show("Title", "Hello World") |> ignore
            Result.Succeeded
