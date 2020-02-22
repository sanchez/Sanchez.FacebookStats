namespace Sanchez.FacebookStats

open System

type Name = Name of string

type GenericMessage =
    {
        Sender: Name
        Message: string
        Posted: DateTime
    }
    
type Message =
    | Generic of GenericMessage