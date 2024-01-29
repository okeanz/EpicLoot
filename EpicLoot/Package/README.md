# LGH.EpicLoot

Added the "Enchantment" skill.
The maximum enchantment level is limited by skill level.
Magic - 0
Rare - 25
Epic - 50
Legendary - 80
This applies to both the enchantment itself and the augmentation.
Configured in a new enchantingskill.json file

The multipliers for gaining skill experience for each action on the enchantment table are also set there.
Basic values:
"successEnchantSkillMultiplier": 1.5,
"successDisenchantSkillMultiplier": 1.0,
"successAugmentationSkillMultiplier": 1.2,
"successConvertSkillMultiplier": 1.0

For the gambler configuration (adventuredata.json), the "GlobalKeyRequire" modifier has been added, which indicates after killing which of the bosses this or that item can appear in the lottery or store

The rarity of drop items is also limited by current progress

Rare items can drop after killing Bonemass
Epic - Yagluth
Legendary - Queen
There is no configuration for this yet, it is hardcoded

Configuration files are configured to use the LootGoblinHeim modpack. Includes settings for Therzie mods, GoldenJudes Eqipment, etc.

Not all interface changes have been translated into English; this will be corrected in future versions.

# Ru

Добавлен навык "Зачарование".
Максимальный уровень зачарования ограничен уровнем навыка.
Магический - 0
Редкий - 25
Эпический - 50
Легендарный - 80
Это касается как самого зачарования, так и аугментации.
Конфигурируетя в новом файле enchantingskill.json

Там же выставляются множители получения опыта навыка за каждое из действий на столе зачарования.
Базовые значения:
"successEnchantSkillMultiplier": 1.5,
"successDisenchantSkillMultiplier": 1.0,
"successAugmentationSkillMultiplier": 1.2,
"successConvertSkillMultiplier": 1.0

Для конфигурации гэмблера (adventuredata.json) добавлен модификатор "GlobalKeyRequire" который указывает после убийства какого из боссов тот или иной предмет может появиться в лотерее или магазине

Редкость предметов дропа также ограничена текущим прогрессом

Редкие предметы могут выпасть после убийства Bonemass
Эпические - Yagluth
Легендарные - Queen
На это пока нет конфигурации, оно захардкожено

Конфигурационные файлы настроены под использование модпака LootGoblinHeim. Включают в себя настройки с учетом модов Therzie, GoldenJudes Eqipment и т.д.

Не все интерфейсные изменения переведены на английский, будет исправлено в последующих версиях.
