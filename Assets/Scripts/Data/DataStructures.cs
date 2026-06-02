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

// 用于本地存储的聊天记录，比 DoubaoMessage 多一个 reaction 字段
[Serializable]
public class ChatMessageRecord
{
    public string role;
    public string content;
    public int reaction; // 0=无 1=赞 -1=踩
}

[Serializable]
public class ChatHistoryWrapper
{
    public List<ChatMessageRecord> messages = new List<ChatMessageRecord>();
}

// 成就定义（从 achievements.json 加载）
[Serializable]
public class AchievementDef
{
    public string id;
    public string name;
    public string description;
    public string category;
    public bool isHidden;
}

[Serializable]
public class AchievementDefWrapper
{
    public List<AchievementDef> achievements = new List<AchievementDef>();
}

// 商品定义
[Serializable]
public class Product
{
    public string id;
    public string name;
    public string description;
    public int price;
    public string category;   // "ticket" / "stamina"
    public int value;          // 该商品的效果数值（射击次数1/5，体力10/30/60）
}

[Serializable]
public class ProductWrapper
{
    public List<Product> products = new List<Product>();
}

// 玩家成就进度（持久化）
[Serializable]
public class AchievementProgress
{
    public string id;
    public bool unlocked;
    public string unlockedTime;
}

[Serializable]
public class AchievementProgressWrapper
{
    public List<AchievementProgress> items = new List<AchievementProgress>();
}

// 单个存档槽的数据
[Serializable]
public class SaveSlot
{
    public int slotIndex;          // 1-5
    public string saveName;        // 存档名
    public string timestamp;       // 存档时间，如 "2026/5/11 15:23"
    public string sceneName;       // 所在场景名
    public int currentDialogueId;
    public int selectedCharacter;
    public string characterName;   // 角色名
    public int favorability1;
    public int favorability2;
    public int favorability3;
    public bool isEmpty = true;    // 是否空槽
}