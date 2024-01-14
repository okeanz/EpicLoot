﻿using System;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace EpicLoot.BaseEL.Adventure.Feature
{
    public interface IMerchantListPanel
    {
        bool NeedsRefresh(bool currenciesChanged);
        void RefreshButton(Currencies playerCurrencies);
        void UpdateRefreshTime();
        void RefreshItems(Currencies currencies);

        Button GetMainButton();
    }

    public abstract class MerchantListPanel<T> : IMerchantListPanel where T : Component, IMerchantPanelListElement
    {
        public RectTransform List;
        public T ElementPrefab;
        public Button MainButton;
        public Text RefreshTime;

        protected int _currentInterval = -1;
        protected int _selectedItemIndex = -1;

        protected MerchantListPanel(RectTransform list, T elementPrefab, Button button, [CanBeNull] Text refreshTime)
        {
            List = list;
            ElementPrefab = elementPrefab;
            ElementPrefab.gameObject.SetActive(false);
            MainButton = button;
            MainButton.onClick.AddListener(OnMainButtonClicked);
            RefreshTime = refreshTime;
        }

        public abstract bool NeedsRefresh(bool currenciesChanged);
        public abstract void RefreshItems(Currencies currencies);
        public abstract void UpdateRefreshTime();
        public abstract void RefreshButton(Currencies playerCurrencies);
        protected abstract void OnMainButtonClicked();

        public virtual Button GetMainButton()
        {
            return MainButton;
        }

        protected void UpdateRefreshTime(int seconds)
        {
            if (RefreshTime != null)
            {
                RefreshTime.text = Localization.instance.Localize(ConvertSecondsToDisplayTime(seconds));
            }
        }

        protected virtual T GetSelectedItem()
        {
            for (var i = 0; i < List.childCount; i++)
            {
                if (i == _selectedItemIndex)
                {
                    return List.GetChild(i).GetComponent<T>();
                }
            }

            return null;
        }

        protected virtual void OnItemSelected(int index)
        {
            _selectedItemIndex = index;

            for (int i = 0; i < List.childCount; i++)
            {
                var child = List.GetChild(i).GetComponent<T>();
                child.SetSelected(i == _selectedItemIndex);
            }
        }

        protected static string ConvertSecondsToDisplayTime(int seconds)
        {
            if (seconds < 0)
            {
                return "$mod_epicloot_merchant_unknown";
            }

            var timeSpan = new TimeSpan(0, 0, 0, seconds);
            if (timeSpan.Days > 0)
            {
                return timeSpan.ToString("d'$mod_epicloot_merchant_days 'h'$mod_epicloot_merchant_hours 'm'$mod_epicloot_merchant_minutes 's'$mod_epicloot_merchant_seconds'");
            }
            else if (timeSpan.Hours > 0)
            {
                return timeSpan.ToString("h'$mod_epicloot_merchant_hours 'm'$mod_epicloot_merchant_minutes 's'$mod_epicloot_merchant_seconds'");
            }
            else
            {
                return timeSpan.ToString("m'$mod_epicloot_merchant_minutes 's'$mod_epicloot_merchant_seconds'");
            }
        }

        protected void DestroyAllListElementsInList()
        {
            foreach (Transform child in List)
            {
                Object.Destroy(child.gameObject);
            }
        }
    }
}
