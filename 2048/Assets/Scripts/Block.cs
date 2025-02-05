using NUnit.Framework;
using TMPro;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class Block : MonoBehaviour
{
    int _value;
    Cell _currentCell;

    [SerializeField] TextMeshProUGUI valueTMP;
    [SerializeField] Image image;

    public int Value { get { return _value; } }
    public Cell CurrentCell {  get { return _currentCell; } }
    
    public void InitializeBlock(GameManager.BlockType type)
    {
        _value = type.Value;
        valueTMP.text = _value.ToString();
        image.color = type.Color;
    }

    public void SetCell(Cell cell)
    {
        _currentCell = cell;
    }
}
