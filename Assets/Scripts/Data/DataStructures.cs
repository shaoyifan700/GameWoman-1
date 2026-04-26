using System;
using System.Collections.Generic;

[Serializable]
public class Character
{
    public int id;
    public string name;
    public string type;
    public string avatarPath;
    public string personality;
}

[Serializable]
public class FavorabilityChange
{
    public int characterId;
    public int change;
}

[Serializable]
public class Choice
{
    public int choiceId;
    public string text;
    public int nextDialogueId;
    public List<FavorabilityChange> favorabilityChanges = new List<FavorabilityChange>();
}

[Serializable]
public class Dialogue
{
    public int id;
    public int characterId;
    public string characterName;
    public string text;
    public string backgroundPath;
        public string expression; // 新增：表情字段（Normal/Smile）

    public List<Choice> choices = new List<Choice>();
}

[Serializable]
public class CharacterDataWrapper
{
    public List<Character> characters = new List<Character>();
}

[Serializable]
public class DialogueDataWrapper
{
    public List<Dialogue> dialogues = new List<Dialogue>();
}

[Serializable]
public class ChatOption
{
    public int optionId;
    public string text;
}

[Serializable]
public class ChatReply
{
    public string optionId;
    public string replyText;
}

[Serializable]
public class ChatRound
{
    public int roundId;
    public List<ChatOption> playerOptions = new List<ChatOption>();
    public List<ChatReply> replies = new List<ChatReply>();
}

[Serializable]
public class CharacterChat
{
    public int characterId;
    public string characterName;
    public List<ChatRound> rounds = new List<ChatRound>();
}

[Serializable]
public class ChatDataWrapper
{
    public List<CharacterChat> chats = new List<CharacterChat>();
}

[Serializable]
public class ChatCharacter
{
    public int id;
    public string name;
    public string tag;
    public string avatar;
    public string systemPrompt;
}

[Serializable]
public class ChatCharacterWrapper
{
    public List<ChatCharacter> characters = new List<ChatCharacter>();
}