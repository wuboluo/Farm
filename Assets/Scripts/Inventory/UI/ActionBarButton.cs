using System;
using UnityEngine;

namespace Y.Inventory
{
    [RequireComponent(typeof(SlotUI))]
    public class ActionBarButton : MonoBehaviour
    {
        public KeyCode key;
        private SlotUI slotUI;

        private bool canUse;

        private void Awake()
        {
            slotUI = GetComponent<SlotUI>();
            canUse = true;
        }

        private void Update()
        {
            if (Input.GetKeyDown(key) && canUse)
            {
                if (slotUI.itemDetails != null)
                {
                    slotUI.isSelected = !slotUI.isSelected;

                    if (slotUI.isSelected)
                        slotUI.inventoryUI.UpdateSlotHighlight(slotUI.slotIndex);
                    else
                        slotUI.inventoryUI.UpdateSlotHighlight(-1);

                    EventHandler.CallItemSelectedEvent(slotUI.itemDetails, slotUI.isSelected);
                }
            }
        }

        private void OnEnable()
        {
            EventHandler.UpdateGameStateEvent += OnUpdateGameState;
        }

        private void OnDisable()
        {
            EventHandler.UpdateGameStateEvent += OnUpdateGameState;
        }

        private void OnUpdateGameState(GameState state)
        { 
            canUse = state == GameState.GamePlay;
        }
    }
}