using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Block : MonoBehaviour
{
    [HideInInspector] public int Value;
    [HideInInspector] public Vector2 Pos => transform.position;
    [HideInInspector] public Node Node;
    [HideInInspector] public Block mergingBlock;
    [HideInInspector] public bool merging;
    [SerializeField] private SpriteRenderer rendrer;
    [SerializeField] private TextMeshPro text;
    public void Init(BlockType type)
    {
        Value = type.value;
        rendrer.color = type.color;
        text.text = Value.ToString();
    }
    public void SetBlock(Node node)
    {
        if (Node != null) Node.currentBlock = null;
        Node = node;
        Node.currentBlock = this;
    }
    public void MergBlock(Block blockMergingwith)
    {
        mergingBlock = blockMergingwith;
        Node.currentBlock = null;
        blockMergingwith.merging = true;
    }
    public bool CanMerge(int value) => value == Value && !merging && mergingBlock == null;
}
