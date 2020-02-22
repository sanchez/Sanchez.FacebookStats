namespace Sanchez.FacebookStats

type MessageEntityParticipants = { name: string }
type MessageEntityMessage =
    {
        sender_name: string
        timestamp_ms: int64
        content: string option
//        photos: obj list option
        ``type``: string
    }
type MessageEntity =
    {
        participants: MessageEntityParticipants list
        messages: MessageEntityMessage list
        title: string
        is_still_participant: bool
        thread_type: string
        thread_path: string
    }

