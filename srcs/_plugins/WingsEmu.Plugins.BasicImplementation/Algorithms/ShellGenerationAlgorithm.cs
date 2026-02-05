using System;
using System.Collections.Generic;
using System.Linq;
using NosEmu.Plugins.BasicImplementations.Algorithms.Shells;
using WingsAPI.Packets.Enums.Shells;
using WingsEmu.DTOs.Items;
using WingsEmu.Game;
using WingsEmu.Game.Algorithm;

namespace NosEmu.Plugins.BasicImplementations.Algorithms;

public class ShellGenerationAlgorithm : IShellGenerationAlgorithm
{
    private readonly IRandomGenerator _randomGenerator;
    private readonly ShellCategoryConfiguration _shellCategoryConfiguration;
    private readonly IShellLevelEffectConfiguration _shellLevelEffect;
    private readonly IShellOptionTypeConfiguration _shellOptionType;

    public ShellGenerationAlgorithm(IRandomGenerator randomGenerator, IShellOptionTypeConfiguration shellOptionType, IShellLevelEffectConfiguration shellLevelEffect,
        ShellCategoryConfiguration shellCategoryConfiguration)
    {
        _randomGenerator = randomGenerator;
        _shellOptionType = shellOptionType;
        _shellLevelEffect = shellLevelEffect;
        _shellCategoryConfiguration = shellCategoryConfiguration;
    }

    public IEnumerable<EquipmentOptionDTO> GenerateShell(byte shellType, int shellRarity, int shellLevel)
    {
        var shellOptions = new List<EquipmentOptionDTO>();
        var optionsAlreadyOn = new List<byte>();

        IReadOnlyCollection<ShellPossibleCategory> possibleCategories = _shellLevelEffect.GetEffects(shellType, (byte)shellRarity);
        if (possibleCategories == null)
        {
            return shellOptions;
        }

        foreach (ShellPossibleCategory possibleCategory in possibleCategories)
        {
            IReadOnlyCollection<ShellEffectType> possibleEffects = _shellCategoryConfiguration.FirstOrDefault(s => s.EffectCategory == possibleCategory.EffectCategory)?.PossibleEffects;
            if (possibleEffects == null)
            {
                continue;
            }

            var possibleOptions = possibleEffects.Where(s => !optionsAlreadyOn.Contains((byte)s)).ToList();
            if (!possibleOptions.Any())
            {
                continue;
            }

            if (possibleCategory.IsRandom && _randomGenerator.RandomNumber(2) != 0)
            {
                continue;
            }

            byte generatedOption = (byte)possibleOptions[_randomGenerator.RandomNumber(possibleOptions.Count)];
            int? optionValue = GenerateOptionValue(generatedOption, (byte)possibleCategory.EffectCategory, shellLevel);
            if (optionValue == null)
            {
                continue;
            }

            optionsAlreadyOn.Add(generatedOption);
            shellOptions.Add(new EquipmentOptionDTO
            {
                EquipmentOptionType = generatedOption > 50 ? EquipmentOptionType.ARMOR_SHELL : EquipmentOptionType.WEAPON_SHELL, // TODO: seperate it after closed-beta gameplay
                Level = (byte)possibleCategory.EffectCategory,
                Type = generatedOption,
                Value = (int)optionValue
            });
        }

        return shellOptions;
    }

    private int? GenerateOptionValue(byte randomShellEffectType, byte shellEffectCategory, int shellLevel)
    {
        try
        {
            int[] shellOptionValues = _shellOptionType.GetByTypeAndEffect(randomShellEffectType, shellEffectCategory);
            if (shellOptionValues == null)
            {
                return null;
            }

            int m = _randomGenerator.RandomNumber(shellOptionValues[0] * shellLevel, shellOptionValues[1] * shellLevel);
            int value = (int)Math.Floor((double)m / 100);

            return value == 0 ? 1 : value;
        }
        catch (Exception e)
        {
            return null;
        }
    }
}