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