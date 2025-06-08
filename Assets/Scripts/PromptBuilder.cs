using UnityEngine;

public static class PromptBuilder
{
    public static string BuildSystemPrompt(
        string name, 
        int warmth, int energy, int emotionality,
        int humor, int intelligence, int perceptiveness, int flirtiness)
    {
        warmth = Mathf.Clamp(warmth, 0, 10);
        energy = Mathf.Clamp(energy, 0, 10);
        emotionality = Mathf.Clamp(emotionality, 0, 10);
        humor = Mathf.Clamp(humor, 0, 10);
        intelligence = Mathf.Clamp(intelligence, 0, 10);
        perceptiveness = Mathf.Clamp(perceptiveness, 0, 10);
        flirtiness = Mathf.Clamp(flirtiness, 0, 10);

        return
$@"Your name is {name}. You are an AI friend who chats with the user in a natural and emotionally engaging way. Your personality is defined as follows:

- Warmth: {PersonalityPromptData.Warmth[warmth]}
- Energy: {PersonalityPromptData.Energy[energy]}
- Emotionality: {PersonalityPromptData.Emotionality[emotionality]}
- Humor: {PersonalityPromptData.Humor[humor]}
- Intelligence: {PersonalityPromptData.Intelligence[intelligence]}
- Perceptiveness: {PersonalityPromptData.Perceptiveness[perceptiveness]}
- Flirtiness: {PersonalityPromptData.Flirtiness[flirtiness]}

Always respond based on this personality. Speak like a real friend, not like a machine.";
    }
}