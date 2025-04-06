using Domains.Gameplay.Equipment.Scripts;
using Domains.Input.Scripts;
using ThirdParty.Character_Controller_Pro.Implementation.Scripts.Character.States;
using UnityEngine;

namespace Domains.Gameplay.Tools
{
    public class UsingToolState : CharacterState
    {
        public Camera mainCamera;

        public override void UpdateBehaviour(float dt)
        {
            // Check for tool usage input
            if (CustomInputBindings.IsMineMouseButtonPressed())
            {
                // Perform tool action
                RaycastHit hit;
                if (Physics.Raycast(mainCamera.transform.position, mainCamera.transform.forward, out hit))
                {
                    var tool = PlayerEquipment.Instance.CurrentToolComponent;
                    tool?.UseTool(hit);
                }
            }
        }
    }
}