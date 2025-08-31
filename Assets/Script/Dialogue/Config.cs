using UnityEngine;

/// <summary>
/// A ScriptableObject to hold global game configuration settings.
/// Create an instance via Assets > Create > AIGirl > Game Configuration.
/// </summary>
[CreateAssetMenu(fileName = "GameConfig", menuName = "AIGirl/Game Configuration", order = 0)]
public class Config : ScriptableObject
{
    [Header("Pet Information")]
    [Tooltip("The name of the pet.")]
    public string PetName = "Aiko";

    [Header("AI Dialogue Settings")]
    [Tooltip("API Key for the Large Language Model service.")]
    public string ApiKey;
    [Tooltip("The endpoint URL for the LLM's chat completion API.")]
    public string ApiEndpoint;
}
