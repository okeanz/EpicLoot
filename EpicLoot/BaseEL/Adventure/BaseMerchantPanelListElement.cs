using UnityEngine;

namespace EpicLoot.BaseEL.Adventure
{
    public interface IMerchantPanelListElement
    {
        void SetSelected(bool selected);
    }

    public class BaseMerchantPanelListElement<T> : MonoBehaviour, IMerchantPanelListElement where T : class
    {
        public GameObject SelectedBackground;

        public void SetSelected(bool selected)
        {
            SelectedBackground.SetActive(selected);
        }
    }
}
