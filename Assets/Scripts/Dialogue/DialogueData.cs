using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum DialogueType
{
    Menu,
    PlayerSelection,
    Combat,
    Victory,
    Defeat
}

[CreateAssetMenu(
    fileName = "New Dialogue",
    menuName = "Dialogue/Dialogue Data"
)]
public class DialogueData : ScriptableObject
{
    [Header("Dialogue Information")]
    public DialogueType dialogueType;

    public string speakerName;

    public Sprite speakerPortrait;

    [Header("Dialogue Lines")]
    public DialogueLine[] lines;
}

[System.Serializable]
public class DialogueLine
{
    public string speakerName; //JJR
    
    [TextArea(2, 5)]
    public string text;

    public Sprite portrait;

    public AudioClip soundEffect;
}
