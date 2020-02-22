namespace Sanchez.FacebookStats

open System.Collections.Generic

//type EnumeratorCallBack<'T, 'State> = 'State -> ('T * 'State) option 
//
//type EnumeratorInvokerEnumerator<'T, 'State> (invoker: EnumeratorCallBack<'T, 'State>, initialState: 'State) =
//    let mutable currentState = initialState
//    let fetchNext () =
//        match invoker currentState with
//        | Some (result, state) ->
//            currentState <- state
//            result
//        | None -> None
//    
//    interface IEnumerator<'T> with
//        member x.MoveNext () =
//            match fetchNext () with
//            | Some item ->
//                true
//            | None -> false
//            
//        member x.get_Current () =
//            failWith "Failed to load next item"
////            currentItem
////            |> Option.defaultValue (fun _ -> failwith "Failed to load next item")
//            
//        member x.Reset () =
//            ()
//            
//        member x.Dispose () = ()
//            
//        
//
//type EnumeratorInvoker<'T, 'State> (invoker: EnumeratorCallBack<'T, 'State>, initialState: 'State) =
//    interface IEnumerable<'T> with
//        member x.GetEnumerator () =
//            new EnumeratorInvokerEnumerator<'T, 'State>(invoker, initialState)
//                    
