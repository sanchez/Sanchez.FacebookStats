// Learn more about F# at http://fsharp.org

open System
open System
open System
open FSharp.Json
open Sanchez.FacebookStats
open System.IO
open System.Text
open System.Text.RegularExpressions

type MessageFile = { location: string; index: int }

type SummaryInfo =
    {
        YearCount: Map<int, int>
        MonthCount: Map<int, int>
        DayCount: Map<DayOfWeek, int>
        HourCount: Map<int, int>
        MessageDupes: Map<string, int>
        
        WordCount: Map<string, int>
        CharacterCount: Map<char, int>
        
        LastMessage: DateTime
        CurrentMessageChain: int
        LongestMessageCount: int
        
        AuthorCount: Map<string, int>
        LettersPerAuthor: Map<string, int>
        
        MessageCount: int
    }
module SummaryInfo =
    let create () =
        {
            SummaryInfo.YearCount = Map.empty
            MonthCount = Map.empty
            DayCount = Map.empty
            HourCount = Map.empty
            MessageDupes = Map.empty
            
            WordCount = Map.empty
            CharacterCount = Map.empty
            
            LastMessage = DateTime.MinValue
            CurrentMessageChain = 0
            LongestMessageCount = 0
            
            AuthorCount = Map.empty
            LettersPerAuthor = Map.empty
            
            MessageCount = 0
        }
        
let addOrSetValueMap key value map =
    let currentValue =
        map
        |> Map.tryFind key
        |> Option.defaultValue 0
    map |> Map.add key (currentValue + value)
        
let addOrSetOneMap key = addOrSetValueMap key 1
    
let addListOfItems items map =
    items |> List.fold (fun acc x -> addOrSetOneMap x acc) map

let trimString (input: string) =
    let sb = new StringBuilder()
    input |> Seq.iter (fun x ->
        if ((x >= 'A' && x <= 'Z') || (x >= 'a' && x <= 'z')) then
            sb.Append x |> ignore)
    sb.ToString()
        
let onEachMessage (state: SummaryInfo) (message: Message) =
    match message with
    | Generic m ->
        let year = state.YearCount |> addOrSetOneMap m.Posted.Year
        let month = state.MonthCount |> addOrSetOneMap m.Posted.Month
        let day = state.DayCount |> addOrSetOneMap m.Posted.DayOfWeek
        let hour = state.HourCount |> addOrSetOneMap m.Posted.Hour
        let message = state.MessageDupes |> addOrSetOneMap (m.Message.ToLower ())
        
        let words = 
            m.Message.ToLower().Split(' ')
            |> Array.toList |> List.map trimString
        let wordsMap =
            state.WordCount |> addListOfItems words
            
        let characters =
            state.CharacterCount |> addListOfItems (m.Message.ToLower() |> Seq.toList)
        
        let betweenMessageLength =
            state.LastMessage.Subtract m.Posted
            |> (fun x -> x.TotalMinutes)
        let currentMessageChain =
            if betweenMessageLength >= 10.0 then 0
            else state.CurrentMessageChain + 1
        let longestMessageChain =
            if state.CurrentMessageChain > state.LongestMessageCount then state.CurrentMessageChain
            else state.LongestMessageCount
            
        let authorCount = state.AuthorCount |> addOrSetOneMap (match m.Sender with | Name s -> s)
        let lettersPerAuthor = state.LettersPerAuthor |> addOrSetValueMap (match m.Sender with | Name s -> s) (m.Message.Length)

        { state with
            YearCount = year
            MonthCount = month
            DayCount = day
            HourCount = hour
            MessageDupes = message
            
            WordCount = wordsMap
            CharacterCount = characters
            
            LastMessage = m.Posted
            CurrentMessageChain = currentMessageChain
            LongestMessageCount = longestMessageChain
            
            AuthorCount = authorCount
            LettersPerAuthor = lettersPerAuthor
            
            MessageCount = state.MessageCount + 1
        }        

[<EntryPoint>]
let main argv =
    let messagesDir = @"C:\Users\dfitz\Downloads\facebook-dfitz360\messages\inbox\LorraineChung_WC9mZ9sxCg"
    
    let messageFiles =
        messagesDir
        |> DirectoryInfo
        |> (fun x -> x.GetFiles "message_*.json")
        |> Array.toList
        |> List.choose (fun x ->
            let res = Regex.Match(x.Name, @"^message_(\d*)\.json$")
            match res.Success with
            | true ->
                let index = res.Groups.[1].Value |> int
                { MessageFile.location = x.FullName; index = index } |> Some
            | false -> None)
        |> List.sortBy (fun x -> x.index)
        |> List.toSeq
        |> Seq.map (fun file ->
                let fileContent = File.ReadAllText file.location
                let content = Json.deserialize<MessageEntity> fileContent
                
                content.messages
                |> List.choose (fun x ->
                    let dt =
                        x.timestamp_ms
                        |> float
                        |> DateTime(1970, 1, 1).AddMilliseconds
                        |> (fun x -> x.AddHours 10.0)
                    let sender = x.sender_name |> Name
                    
                    match (x.content) with
                    | (Some m) -> { GenericMessage.Message = m; Sender = sender; Posted = dt } |> Generic |> Some
                    | _ -> None))
        |> Seq.collect id
        
    let results =
        messageFiles
        |> Seq.fold onEachMessage (SummaryInfo.create ())
        
    let mostFrequentYear = results.YearCount |> Map.toList |> List.sortByDescending snd
    let mostFrequentMonth = results.MonthCount |> Map.toList |> List.sortByDescending snd
    let mostFrequentDay = results.DayCount |> Map.toList |> List.sortByDescending snd
    let mostFrequentHour = results.HourCount |> Map.toList |> List.sortByDescending snd
    let mostDuplicatedMessage = results.MessageDupes |> Map.toList |> List.sortByDescending snd
    let mostWord = results.WordCount |> Map.toList |> List.sortByDescending snd
    let mostCharacter = results.CharacterCount |> Map.toList |> List.sortByDescending snd
    let longestChain = results.LongestMessageCount
    let mostAuthorMessages = results.AuthorCount |> Map.toList |> List.sortByDescending snd
    let mostAuthorCharacters = results.LettersPerAuthor |> Map.toList |> List.sortByDescending snd
    let totalMessages = results.MessageCount

    messageFiles |> ignore
    
    printfn "Hello World from F#!"
    0 // return an integer exit code
