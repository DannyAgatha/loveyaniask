// NosEmu
// 


namespace DiscordNotifier
{
    public enum LogType
    {
        PLAYERS_EVENTS_CHANNEL,

        CHAT_GENERAL, // channel, characterName, message
        CHAT_WHISPERS, // channel, characterName, message
        CHAT_FRIENDS, // channel, characterName, message
        CHAT_FAMILIES, // channel, characterName, message
        CHAT_SPEAKERS, // channel, characterName, message
        CHAT_GROUPS, // channel, characterName, groupId, message

        FARMING_LEVEL_UP, // channel, characterName, levelType, level

        FAMILY_CREATED, // channel, characterName, familyId, familyName, deputies
        FAMILY_DISBANDED, // channel, characterName, familyId
        FAMILY_JOINED, // channel, characterName, familyId, inviterId
        FAMILY_LEFT, // channel, characterName, familyId
        FAMILY_KICK, // channel, characterName, familyId, kickedId, kickedName
        FAMILY_MESSAGES, // channel, characterName, familyId, messageType, message

        MINIGAME_REWARD_CLAIMED,
        MINIGAME_SCORE,

        ITEM_GAMBLED,
        ITEM_UPGRADED,
        ITEM_BAZAAR,
        ITEM_EXCHANGE,

        GENERAL_ITEM_USAGE, // channel, characterName, itemVnum
        GENERAL_EXCHANGE, // channel, characterName, tradeCharacterName, tradeInfos (items, gold...)

        EXPLOITS_WRONG_PACKET, // channel, characterName, sent packet
        EXPLOITS_TRIED_TO_DUPE, // channel, characterName, sent packet
        EXPLOITS_WARNINGS, // channel, characterName, WarningType, warning arguments
        GENERAL_ERROR, 

        STRANGE_BEHAVIORS, // channel, behaviorType, message
        ACT4_DUNGEON_EVENT,
        ACT4_PVP_KILL,
        COMMANDS_PLAYER_COMMAND_EXECUTED, // channel, characterName
        COMMANDS_GM_COMMAND_EXECUTED,// channel, characterName, authority
        WORLD_BOSS_STARTED,
        PRESTIGE_UNLOCKED
    }
}