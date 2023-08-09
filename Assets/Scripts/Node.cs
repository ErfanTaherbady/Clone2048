using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node : MonoBehaviour
{
    [HideInInspector] public Vector2 position => transform.position;
    [HideInInspector] public Block currentBlock;
}
