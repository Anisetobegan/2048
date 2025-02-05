using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using System.Collections;
using System;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    int valueWinCondition = 2048;

    [SerializeField] Cell cellPrefab;
    [SerializeField] Block blockPrefab;
    [SerializeField] Transform layout;
    [SerializeField] Transform backgroundPanel;
    [SerializeField] GameObject winScreen;
    [SerializeField] GameObject loseScreen;

    //[SerializeField] List<Cell> _cells;
    Cell[,] _cells = new Cell[4, 4];
    List<Block> _blocks = new List<Block>();
    [SerializeField] List<BlockType> _blockTypes;

    enum GameState
    {
        WaitingInput,
        Moving,
        Win,
        Lose
    }
    [SerializeField] GameState _gameState;

    [Serializable]
    public struct BlockType
    {
        public int Value;
        public Color Color;
    }

    private void Start()
    {        
        SpawnCells();
        StartCoroutine(WaitNextFrameToSpawn());
        _gameState = GameState.WaitingInput;
    }

    private void Update()
    {
        /*if (Input.GetKeyDown(KeyCode.Space))
        {
            SpawnBlock(1);
        }*/
        switch (_gameState)
        {
            case GameState.WaitingInput:

                if (Input.GetKeyDown(KeyCode.W))
                {
                    MoveBlocks(Vector3.up);
                }
                if (Input.GetKeyDown(KeyCode.S))
                {
                    MoveBlocks(Vector3.down);
                }
                if (Input.GetKeyDown(KeyCode.A))
                {
                    MoveBlocks(Vector3.left);
                }
                if (Input.GetKeyDown(KeyCode.D))
                {
                    MoveBlocks(Vector3.right);
                }

                break;

            case GameState.Moving:
                break;

            case GameState.Win:

                OpenWinScreen();

                break;

            case GameState.Lose:

                OpenLoseScreen();

                break;
        }
    }

    IEnumerator WaitNextFrameToSpawn()
    {
        yield return null;
        SpawnBlock(2);
    }

    BlockType GetBlockTypeByValue(int value)
    {
        return _blockTypes.First(t => t.Value == value);
    }

    void SpawnCells()
    {
        for (int x = 0; x < 4; x++)
        {
            for (int y = 0; y < 4; y++)
            {
                _cells[x, y] = Instantiate(cellPrefab, layout);
                _cells[x, y].SetArrayPosition(x, y);
            }
        }
    }
    
    void SpawnBlock(int amount)
    {
        List<Cell> freeCells = CheckFreeCells();

        if (freeCells.Count > 0)
        {
                for (int i = 0; i < amount; i++)
                {
                    int index = UnityEngine.Random.Range(0, freeCells.Count);

                    Block newBlock = Instantiate(blockPrefab, backgroundPanel);
                    //RectTransform newBlockRectTransform = (RectTransform)newBlock.transform;
                    //newBlockRectTransform.anchoredPosition = freeCells[index].transform.position;
                    newBlock.transform.position = freeCells[index].transform.position;
                    newBlock.transform.rotation = Quaternion.identity;

                    newBlock.InitializeBlock(GetBlockTypeByValue(UnityEngine.Random.value > 0.8f ? 4 : 2));
                    newBlock.SetCell(freeCells[index]);
                    freeCells[index].FillCell(newBlock);

                    _blocks.Add(newBlock);
                }
        }
        else
        {
            _gameState = GameState.Lose;
        }
        //_gameState = GameState.WaitingInput;
    }

    void SpawnBlock(Cell cellToSpawnBlock, int combinedValue)
    {
        Block newBlock = Instantiate(blockPrefab, backgroundPanel);
        //RectTransform newBlockRectTransform = (RectTransform)newBlock.transform;
        //newBlockRectTransform.anchoredPosition = freeCells[index].transform.position;
        newBlock.transform.position = cellToSpawnBlock.transform.position;
        newBlock.transform.rotation = Quaternion.identity;

        newBlock.InitializeBlock(GetBlockTypeByValue(combinedValue));
        newBlock.SetCell(cellToSpawnBlock);
        cellToSpawnBlock.FillCell(newBlock);

        _blocks.Add(newBlock);

        if (combinedValue == valueWinCondition)
        {
            _gameState = GameState.Win;
        }
    }

    List<Cell> CheckFreeCells()
    {
        List<Cell> freeCells = new List<Cell>();

        for (int x = 0; x < 4; x++)
        {
            for (int y = 0; y < 4; y++)
            {
                if (_cells[x, y].OccupiedBlock == null)
                {
                    freeCells.Add(_cells[x, y]);
                }
            }
        }

        if (freeCells.Count > 0)
        {
            return freeCells;
        }
        else
        {
            return null;
        }        
    }

    void MoveBlocks(Vector3 dir)
    {
        _gameState = GameState.Moving;

        List<Block> orderedBlocks = new List<Block>();
        Sequence sequence = DOTween.Sequence();
        int blocksMovedCount = 0;

        for (int x = 0;x < 4; x++)
        {
            for (int y = 0;y < 4; y++)
            {
                if (_cells[x, y].OccupiedBlock != null)
                {
                    orderedBlocks.Add(_cells[x, y].OccupiedBlock); //Adds to the list all the blocks in order of the array
                }
            }
        }

        if (dir == Vector3.right || dir == Vector3.down)
        {
            orderedBlocks.Reverse(); //If the direction is right or down, reverses the list so it moves the block accordingly
        }

        foreach (Block block in orderedBlocks)
        {
            Cell nextCell = block.CurrentCell; //Stores the current cell in the nextCell variable            

            do
            {
                block.SetCell(nextCell);

                Cell possibleCell = GetNextCell(nextCell, dir); //Checks if there is a cell available on the direction of the input
                if (possibleCell != null) //If there are no cells available, do nothing
                {
                    if (possibleCell.OccupiedBlock == null)
                    {
                        //If there is a cell available and it´s not occupied
                        nextCell.ClearCell(); //Clears the occupied block of the current cell
                        nextCell = possibleCell;
                        possibleCell.FillCell(block); //Fills the available cell with this block

                        blocksMovedCount++;
                    }
                    else if (possibleCell.OccupiedBlock.Value == block.CurrentCell.OccupiedBlock.Value && !possibleCell.IsMerging)
                    {
                        //If there is a cell available but is occupied, checks if the blocks have the same value and are open to merge
                        possibleCell.BeginMerging();

                        nextCell.ClearCell();
                        nextCell = possibleCell;
                                                
                        block.transform.DOMove(possibleCell.transform.position, 0.5f).SetEase(Ease.InSine).OnComplete(() => MergeBlock(possibleCell.OccupiedBlock, block));

                        blocksMovedCount++;
                    }
                }
            }while (nextCell != block.CurrentCell); //If nextCell did not change, end the loop

            sequence.Insert(0, block.transform.DOMove(block.CurrentCell.transform.position, 0.5f).SetEase(Ease.InSine));
        }

        sequence.OnComplete(() => 
        {
            _gameState = GameState.WaitingInput;

            if (blocksMovedCount > 0)
            {
                SpawnBlock(1);
            }
            else
            {
                if (CheckFreeCells() == null)
                {
                    _gameState = GameState.Lose;
                }
            }
        });
    }

    Cell GetNextCell(Cell cellToCheck, Vector3 dir)
    {
        Cell nextCell = null;
        int xIndex = 0;
        int yIndex = 0;

        for (int x = 0; x < 4; x++)
        {
            for ( int y = 0; y < 4; y++)
            {
                if (_cells[x, y] == cellToCheck)
                {
                    //Stores the row and the column of the cell in the array
                    xIndex = x;
                    yIndex = y;
                    break;
                }
            }
        }

        if (dir == Vector3.right)
        {
            if (yIndex < 3)
            {
                nextCell= _cells[xIndex, yIndex + 1];
            }
        }
        else if (dir == Vector3.left)
        {
            if (yIndex > 0)
            {
                nextCell = _cells[xIndex, yIndex - 1];
            }
        }
        else if (dir == Vector3.up)
        {
            if (xIndex > 0)
            {
                nextCell = _cells[xIndex - 1, yIndex];
            }
        }
        else if (dir == Vector3.down)
        {
            if (xIndex < 3)
            {
                nextCell = _cells[xIndex + 1, yIndex];
            }
        }
        return nextCell;
    }

    void MergeBlock(Block block, Block blockToMergeWith)
    {
        Cell cellToMergeOn = block.CurrentCell;

        RemoveBlock(blockToMergeWith);
        RemoveBlock(block);

        SpawnBlock(cellToMergeOn, block.Value * 2);

        cellToMergeOn.EndMerging();
    }

    void RemoveBlock(Block blockToRemove)
    {
        blockToRemove.CurrentCell.ClearCell();
        _blocks.Remove(blockToRemove);
        Destroy(blockToRemove.gameObject);
    }

    void OpenWinScreen()
    {
        winScreen.SetActive(true);
    }

    void OpenLoseScreen()
    {
        loseScreen.SetActive(true);
    }

    public void Retry()
    {
        SceneManager.LoadScene(0);
    }
}