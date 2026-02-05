using System.Collections.Generic;
using Mapster;
using PhoenixLib.Caching;
using PhoenixLib.Logging;
using WingsAPI.Data.GameData;
using WingsEmu.DTOs.BCards;
using WingsEmu.DTOs.Buffs;
using WingsEmu.Game.Buffs;
using WingsEmu.Game.Managers.StaticData;

namespace NosEmu.Plugins.BasicImplementations.Managers.StaticData;

public class CardsManager : ICardsManager
{
    private readonly ILongKeyCachedRepository<Card> _cachedCards;
    private readonly IKeyValueCache<List<Card>> _cardByName;
    private readonly IResourceLoader<CardDTO> _cardDao;

    public CardsManager(ILongKeyCachedRepository<Card> cachedCards, IKeyValueCache<List<Card>> cardByName, IResourceLoader<CardDTO> cardDao)
    {
        _cachedCards = cachedCards;
        _cardByName = cardByName;
        _cardDao = cardDao;
    }

    public void Initialize()
    {
        int cardCount = 0;
        IReadOnlyList<CardDTO> cards = _cardDao.LoadAsync().GetAwaiter().GetResult();
        foreach (CardDTO cardDto in cards)
        {
            Card card = cardDto.Adapt<Card>();

            card.BCards = new List<BCardDTO>();
            foreach (BCardDTO bCard in card.Bcards)
            {
                card.BCards.Add(bCard);
            }

            _cachedCards.Set(card.Id, card);
            _cardByName.GetOrSet(card.Name, () => new List<Card>()).Add(card);
            cardCount++;
        }

        Log.Info($"[DATABASE] Loaded {cardCount} cards.");
    }


    public List<Card> GetCardByName(string name) => _cardByName.Get(name);

    public Card GetCardByCardId(int cardId) => _cachedCards.Get(cardId);
}