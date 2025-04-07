using Domains.Gameplay.Equipment.Scripts;
using Domains.Input.Scripts;
using Domains.Scripts_that_Need_Sorting;
using ThirdParty.Character_Controller_Pro.Implementation.Scripts.Character.States;
using UnityEngine;

namespace Domains.Gameplay.Tools
{
    public class UsingToolState : CharacterState
    {
        public Camera mainCamera;
        [SerializeField] private TextureDetector textureDetector;


        public override void UpdateBehaviour(float dt)
        {
            // Check for tool usage input
            if (CustomInputBindings.IsMineMouseButtonPressed())
                if (Physics.Raycast(mainCamera.transform.position, mainCamera.transform.forward, out var hit))
                {
                    var tool = PlayerEquipment.Instance.CurrentToolComponent;
                    if (tool == null) return;

                    // Get terrain texture index at hit point
                    Terrain terrain;
                    var textureIndex = textureDetector.GetTextureIndex(hit, out terrain);

                    // Check if the tool supports both terrain and object
                    var canUse = tool.CanInteractWithTextureIndex(textureIndex) &&
                                 tool.CanInteractWithObject(hit.collider.gameObject);

                    if (canUse)
                        tool.UseTool(hit);
                    else
                        // Optional: play denied feedback
                        UnityEngine.Debug.Log("Tool not valid for this surface or object.");
                }
        }
    }
}