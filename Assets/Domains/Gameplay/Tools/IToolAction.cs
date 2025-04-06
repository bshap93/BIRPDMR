using Domains.Scripts_that_Need_Sorting;
using UnityEngine;

namespace Domains.Gameplay.Tools
{
    public interface IToolAction
    {
        ToolType ToolType { get; }
        void UseTool(RaycastHit hit);
    }
}