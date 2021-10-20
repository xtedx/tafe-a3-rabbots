using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// this is only to allow us write comments in the inspector window about a particular object
/// </summary>
public class Comment : MonoBehaviour
{
    [SerializeField] [Tooltip("write comment here about the object")] [TextArea]
    private string comment;
}
