using System;
using System.Collections.Generic;
using System.Linq;
using ExileCore;
using ExileCore.PoEMemory;
using ExileCore.PoEMemory.Components;
using ExileCore.PoEMemory.Elements;
using ExileCore.PoEMemory.Elements.InventoryElements;
using ExileCore.PoEMemory.MemoryObjects;
using ExileCore.Shared.Enums;
using ItemVisualizer.Core;
using SharpDX;

namespace ItemVisualizer
{
    public class ItemVisualizer : BaseSettingsPlugin<Settings>
    {
        public IngameUIElements _IngameUiElements;
        public InventoryElement _InventoryElement;
        public StashElement _StashElement;
        public Inventory _VisibleStash;
        public Element _hoverElement;
        public RectangleF _hoverElementRec;

        public List<RarityContainer> RarityContainerItems;

        public IList<NormalInventoryItem> StashItems { get; set; }
        public IList<NormalInventoryItem> InventoryItems { get; set; }

        public override bool Initialise()
        {
            StashItems = new List<NormalInventoryItem>();
            InventoryItems = new List<NormalInventoryItem>();
            RarityContainerItems = new List<RarityContainer>();

            Settings.Enable.OnValueChanged += (sender, enabled) =>
            {
                if (enabled)
                {
                    // Setup
                    DebugWindow.LogMsg($"{Name}: enabled", 5);
                }
                else
                {
                    // Teardown
                    DebugWindow.LogMsg($"{Name}: disabled", 5);
                }
            };

            return true;
        }

        public override void Render()
        {
            UpdateCommonlyUsedData();

            try
            {
                if (_InventoryElement.IsVisibleLocal)
                {
                    var inventoryRarityContainerList = ConvertItemToContainerList(GetInventoryItems());
                    RarityContainerItems.AddRange(inventoryRarityContainerList);
                }

                if (_StashElement.IsVisibleLocal)
                {
                    var stashRarityContainerList = ConvertItemToContainerList(GetStashItems());
                    RarityContainerItems.AddRange(stashRarityContainerList);
                }

                foreach (var containerItem in RarityContainerItems)
                {
                    var containerItemElement = containerItem.Element;
                    containerItemElement.Inflate(-4f, -4f);

                    if (!_hoverElementRec.Intersects(containerItemElement))
                        Graphics.DrawFrame(containerItemElement, containerItem.Color, 2);

                    if (containerItem.Corrupted)
                    {
                        containerItemElement.Inflate(-3f, -3f);
                        if (!_hoverElementRec.Intersects(containerItemElement))
                            Graphics.DrawFrame(containerItemElement, Settings.CorruptedBorder, 2);
                    }
                }
            }
            catch (Exception e)
            {
                // cbf writing null checks
            }
        }

        public void UpdateCommonlyUsedData()
        {
            //reset
            RarityContainerItems = new List<RarityContainer>();
            _hoverElementRec = new RectangleF(0,0,0,0);

            _IngameUiElements = GameController.Game.IngameState.IngameUi;
            _InventoryElement = _IngameUiElements.InventoryPanel;
            _StashElement = _IngameUiElements.StashElement;
            _VisibleStash = _StashElement.VisibleStash;

            _hoverElement = GameController.Game.IngameState.UIHover;

            if (_hoverElement.Tooltip != null && _hoverElement.Tooltip.IsVisibleLocal)
                _hoverElementRec = _hoverElement.Tooltip.GetClientRect();
        }


        public List<NormalInventoryItem> GetInventoryItems()
        {
            var inventory = GameController.Game.IngameState.IngameUi.InventoryPanel;
            return !inventory.IsVisible
                ? null
                : inventory[InventoryIndex.PlayerInventory].VisibleInventoryItems.ToList();
        }

        public List<NormalInventoryItem> GetStashItems()
        {
            var itemList = new List<NormalInventoryItem>();
            if (_StashElement.IsVisible)
                // Format stash items
                switch (_VisibleStash.InvType)
                {
                    case InventoryType.BlightStash:
                        itemList = _VisibleStash.VisibleInventoryItems.ToList();
                        itemList.RemoveAt(0);
                        itemList.RemoveAt(itemList.Count - 1);
                        break;
                    case InventoryType.MetamorphStash:
                        itemList = _VisibleStash.VisibleInventoryItems.ToList();
                        itemList.RemoveAt(0);
                        break;
                    default:
                        itemList = _VisibleStash.VisibleInventoryItems.ToList();
                        break;
                }

            return itemList;
        }

        public void GetHoveredEntity()
        {
            _hoverElement = GameController.Game.IngameState.UIHover;
        }

        public List<RarityContainer> ConvertItemToContainerList(List<NormalInventoryItem> itemList) => itemList.Select(ConvertItemToContainer).ToList();
        public RarityContainer ConvertItemToContainer(NormalInventoryItem item)
        {
            var newContainer = new RarityContainer {Element = item.GetClientRect()};

            if (item.Item.HasComponent<Mods>())
                newContainer.Color = RarityToColor(item.Item.GetComponent<Mods>().ItemRarity);

            if (item.Item.HasComponent<Base>())
                newContainer.Corrupted = item.Item.GetComponent<Base>().isCorrupted;
            return newContainer;
        }

        public class RarityContainer
        {
            public Color Color { get; set; } = Color.Transparent;
            public bool Corrupted { get; set; }
            public RectangleF Element { get; set; } = new RectangleF(0, 0, 0, 0);
        }

        public Color RarityToColor(ItemRarity itemRarity)
        {
            var color = new Color(0,0,0, 0);

            switch (itemRarity)
            {
                case ItemRarity.Normal:
                    color = Settings.NormalBorder;
                    break;
                case ItemRarity.Magic:
                    color = Settings.MagicBorder;
                    break;
                case ItemRarity.Rare:
                    color = Settings.RareBorder;
                    break;
                case ItemRarity.Unique:
                    color = Settings.UniqueBorder;
                    break;
            }

            return color;
        }
    }
}