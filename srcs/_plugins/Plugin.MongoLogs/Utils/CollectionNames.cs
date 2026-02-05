namespace Plugin.MongoLogs.Utils
{
    public static class CollectionNames
    {
        /* ACT4 */
        public const string ACT4_KILL = "act4.kill";
        public const string ACT4_DUNGEON_STARTED = "act4.dungeon-started";
        public const string ACT4_DUNGEON_WON = "act4.dungeon-won";

        /* NPC */
        public const string NPC_ITEM_PRODUCED = "npc.item-produced";

        /* BAZAAR */
        public const string BAZAAR_ITEM_BOUGHT = "bazaar.item-bought";
        public const string BAZAAR_ITEM_EXPIRED = "bazaar.item-expired";
        public const string BAZAAR_ITEM_INSERTED = "bazaar.item-inserted";
        public const string BAZAAR_ITEM_WITHDRAWN = "bazaar.item-withdrawn";

        /* CONNECTIONS */
        public const string CONNECTION_SESSION = "connection.session";

        /* COMMANDS */
        public const string COMMAND = "command";

        /* FAMILY MANAGEMENT */
        public const string FAMILY_MANAGEMENT_CREATED = "family-management.created";
        public const string FAMILY_MANAGEMENT_DISBANDED = "family-management.disbanded";
        public const string FAMILY_MANAGEMENT_JOINED = "family-management.joined";
        public const string FAMILY_MANAGEMENT_KICKED = "family-management.kicked";
        public const string FAMILY_MANAGEMENT_LEFT = "family-management.left";
        public const string FAMILY_MANAGEMENT_MESSAGES = "family-management.messages";
        public const string FAMILY_MANAGEMENT_UPGRADE_BOUGHT = "family-management.upgrade-bought";
        public const string FAMILY_WAREHOUSE_ITEM_PLACED = "family-management.warehouse-item-placed";
        public const string FAMILY_WAREHOUSE_ITEM_WITHDRAWN = "family-management.warehouse-item-withdrawn";

        /* INVENTORY */
        public const string INVENTORY_PICKED_UP_ITEM = "inventory.pickedupitem";
        public const string INVENTORY_PICKED_UP_PLAYER_ITEM = "inventory.picked-up-player-item";
        public const string INVENTORY_ITEM_USED = "inventory.item-used";
        public const string INVENTORY_ITEM_DELETED = "inventory.item-deleted";

        /* INVITATIONS */
        public const string INVITATION_FAMILY = "invitation.family";
        public const string INVITATION_GROUP = "invitation.group";
        public const string INVITATION_RAID = "invitation.raid";
        public const string INVITATION_TRADE = "invitation.trade";

        /* LEVEL UPS */
        public const string LEVEL_UP_CHARACTER = "level-up.character";
        public const string LEVEL_UP_NOSMATE = "level-up.nosmate";
        
        /* HEROIC LEVEL UPS */
        public const string HEROIC_LEVEL_UP_NOSMATE = "heroic-level-up.nosmate";

        /* MAILS */
        public const string MAIL_CLAIMED = "mail.claimed";
        public const string MAIL_REMOVED = "mail.removed";

        /* NOTES */
        public const string NOTE_SENT = "note.sent";

        /* QUESTS */
        public const string QUEST_ABANDONED = "quest.abandoned";
        public const string QUEST_ADDED = "quest.added";
        public const string QUEST_COMPLETED = "quest.completed";
        public const string QUEST_OBJECTIVE_UPDATED = "quest.objective-updated";

        /* RAID ACTIONS */
        public const string RAID_ACTION_DIED = "raid-action.died";
        public const string RAID_ACTION_LEVER_ACTIVATED = "raid-action.lever-activated";
        public const string RAID_ACTION_LOST = "raid-action.lost";
        public const string RAID_ACTION_REVIVED = "raid-action.revived";
        public const string RAID_ACTION_REWARD_RECEIVED = "raid-action.reward-received";
        public const string RAID_ACTION_TARGET_KILLED = "raid-action.target-killed";
        public const string RAID_ACTION_WON = "raid-action.won";

        /* RAID MANAGEMENT */
        public const string RAID_MANAGEMENT_ABANDONED = "raid-management.abandoned";
        public const string RAID_MANAGEMENT_CREATED = "raid-management.created";
        public const string RAID_MANAGEMENT_JOINED = "raid-management.joined";
        public const string RAID_MANAGEMENT_LEFT = "raid-management.left";
        public const string RAID_MANAGEMENT_STARTED = "raid-management.started";

        /* RAINBOW BATTLE */
        public const string RAINBOW_BATTLE_WON = "rainbow-battle.won";
        public const string RAINBOW_BATTLE_LOSE = "rainbow-battle.lose";
        public const string RAINBOW_BATTLE_TIE = "rainbow-battle.tie";
        public const string RAINBOW_BATTLE_JOIN = "rainbow-battle.join";
        public const string RAINBOW_BATTLE_FROZEN = "rainbow-battle.frozen";
        
        /* ICEBREAKER */
        public const string ICEBREAKER_WON = "icebreaker.won";
        public const string ICEBREAKER_LOSE = "icebreaker.lose";
        public const string ICEBREAKER_JOIN = "icebreaker.join";
        public const string ICEBREAKER_FROZEN = "icebreaker.frozen";

        /* RANDOM BOXES */
        public const string RANDOM_BOX_OPENED = "random-box.opened";

        /* SHOPS */
        public const string SHOP_CLOSED = "shop.closed";
        public const string SHOP_PLAYER_ITEM_BOUGHT = "shop.player-item-bought";
        public const string SHOP_NPC_ITEM_BOUGHT = "shop.npc-item-bought";
        public const string SHOP_NPC_ITEM_SOLD = "shop.npc-item-sold";
        public const string SHOP_OPENED = "shop.opened";
        public const string SHOP_SKILL_BOUGHT = "shop.skill-bought";
        public const string SHOP_SKILL_SOLD = "shop.skill-sold";


        /* UPGRADES */
        public const string UPGRADE_CELLON = "upgrade.cellon";
        public const string UPGRADE_ITEM_GAMBLED = "upgrade.item-gambled";
        public const string UPGRADE_ITEM_UPGRADED = "upgrade.item-upgraded";
        public const string UPGRADE_RESISTANCE_SUMMED = "upgrade.resistance-summed";
        public const string UPGRADE_SHELL_IDENTIFIED = "upgrade.shell-identified";
        public const string UPGRADE_SP_UPGRADED = "upgrade.sp-upgraded";
        public const string UPGRADE_SP_PERFECTED = "upgrade.sp-perfected";

        /* WAREHOUSE */
        public const string WAREHOUSE_ITEM_PLACED = "warehouse.item-placed";
        public const string WAREHOUSE_ITEM_WITHDRAWN = "warehouse.item-withdrawn";

        /* MINIGAMES */
        public const string MINIGAME_SCORE = "minigame.score";
        public const string MINIGAME_REWARDS_CLAIMED = "minigame.rewards-claimed";

        /* CHAT */
        public const string CHAT = "chat";

        /* EXCHANGES */
        public const string EXCHANGES = "exchanges";
    }
}