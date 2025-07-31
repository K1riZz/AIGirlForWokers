using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class DesktopInputHandler : MonoBehaviour, IPointerClickHandler
{
    private PetInteractionSystem petInteractionSystem;

    void Start()
    {
        // Find the pet's interaction system in the scene.
        petInteractionSystem = FindObjectOfType<PetInteractionSystem>();
        if (petInteractionSystem == null)
        {
            Debug.LogError("Scene is missing a PetInteractionSystem component!");
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // When the background is clicked, send a guide request to the pet.
        // We only care about left-clicks for guiding.
        if (petInteractionSystem != null && eventData.button == PointerEventData.InputButton.Left)
            petInteractionSystem.RequestGuide(eventData.position);
    }

    
}