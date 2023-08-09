using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using DG.Tweening;

public class GameManager : MonoBehaviour
{
    [Header("UI Refrense")]
    [SerializeField] private SpriteRenderer boardPrfab;
    [SerializeField] private Node nodePrfab;
    [SerializeField] private Block blockPrfab;

    [Space(5f)]

    [Header("Parametrs")]
    [SerializeField] private List<BlockType> blockTypes;
    [SerializeField] private int winCondition = 2048;
    [SerializeField] private float blockMoveSpeed;
    [SerializeField] private float wight = 4;
    [SerializeField] private float hight = 4;


    private GameState gameState;
    private List<Node> nodes;
    private List<Block> blocks;
    private int round;

    private void Start()
    {
        Generate();
    }
    public void ChangeGameState(GameState state)
    {
        gameState = state;
        switch (state)
        {
            case GameState.GenerateLeve:
                Generate();
                break;
            case GameState.SpawningBlock:
                SpawnBlocks(round++ == 0 ? 2 : 1);
                break;
            case GameState.WaitingInput:
                break;
            case GameState.Moving:
                break;
            case GameState.Win:
                Debug.Log("Win Game");
                break;
            case GameState.Lose:
                Debug.Log("Lose Game");
                break;
        }
    }
    private void Update()
    {
        if (gameState != GameState.WaitingInput)
            return;
        if (Input.GetKeyDown(KeyCode.A))
            Shift(Vector2.left);
        if (Input.GetKeyDown(KeyCode.S))
            Shift(Vector2.down);
        if (Input.GetKeyDown(KeyCode.D))
            Shift(Vector2.right);
        if (Input.GetKeyDown(KeyCode.W))
            Shift(Vector2.up);
    }

    private void Shift(Vector2 direction)
    {
        ChangeGameState(GameState.Moving);
        var orderedBlocks = blocks.OrderBy(b => b.Pos.x).ThenBy(b => b.Pos.y).ToList();
        if (direction == Vector2.right || direction == Vector2.up) orderedBlocks.Reverse();

        foreach (var block in orderedBlocks)
        {
            var next = block.Node;
            do
            {
                block.SetBlock(next);

                var possibelNode = GetNodeAtPosition(next.position + direction);
                if (possibelNode != null)
                {
                    if (possibelNode.currentBlock != null && possibelNode.currentBlock.CanMerge(block.Value))
                    {
                        block.MergBlock(possibelNode.currentBlock);
                    }
                    else if (possibelNode.currentBlock == null) next = possibelNode;
                }
            } while (next != block.Node);

        }

        var sequence = DOTween.Sequence();
        foreach (var block in orderedBlocks)
        {
            var movePoint = block.mergingBlock != null ? block.mergingBlock.Node.position : block.Node.position;

            sequence.Insert(0, block.transform.DOMove(movePoint, blockMoveSpeed));

            sequence.OnComplete(() =>
            {
                foreach (var block in orderedBlocks.Where(b => b.mergingBlock != null))
                {
                    MergeBlock(block.mergingBlock, block);
                }
                ChangeGameState(GameState.SpawningBlock);
            });

        }
    }
    private void MergeBlock(Block baseBlock, Block mergingBlock)
    {
        SpawnBlock(baseBlock.Node, baseBlock.Value * 2);
        RemoveBlock(baseBlock);
        RemoveBlock(mergingBlock);
    }
    private void RemoveBlock(Block block)
    {
        blocks.Remove(block);
        Destroy(block.gameObject);
    }
    private Node GetNodeAtPosition(Vector2 pos)
    {
        return nodes.FirstOrDefault(n => n.position == pos);
    }
    private void Generate()
    {
        round = 0;
        nodes = new List<Node>();
        blocks = new List<Block>();

        for (int x = 0; x < wight; x++)
        {
            for (int y = 0; y < hight; y++)
            {
                var node = Instantiate(nodePrfab, new Vector2(x, y), Quaternion.identity);
                nodes.Add(node);
            }
        }
        Vector2 center = new Vector2((float)wight / 2 - 0.5f, (float)hight / 2 - 0.5f);

        var board = Instantiate(boardPrfab, center, Quaternion.identity);
        board.size = new Vector2(wight, hight);

        Camera.main.transform.position = new Vector3(center.x, center.y, -10);
        ChangeGameState(GameState.SpawningBlock);
    }
    private void SpawnBlocks(int amount)
    {
        var freeNode = nodes.Where(n => n.currentBlock == null).OrderBy(b => UnityEngine.Random.value).ToList();
        foreach (var node in freeNode.Take(amount))
        {
            SpawnBlock(node, UnityEngine.Random.value > 0.8f ? 2 : 4);
        }

        if (freeNode.Count() == 1)
        {
            //Game Over
            ChangeGameState(GameState.Lose);
            return;
        }
        ChangeGameState(blocks.Any(b => b.Value == winCondition) ? GameState.Win : GameState.WaitingInput);
    }
    private void SpawnBlock(Node node, int value)
    {
        var block = Instantiate(blockPrfab, node.position, Quaternion.identity);
        block.Init(GetBlockTypeByValue(value));
        block.SetBlock(node);
        blocks.Add(block);
    }
    private BlockType GetBlockTypeByValue(int value) => blockTypes.First(t => t.value == value);

}

[Serializable]
public struct BlockType
{
    public int value;
    public Color color;
}

public enum GameState
{
    GenerateLeve,
    SpawningBlock,
    WaitingInput,
    Moving,
    Win,
    Lose
}
