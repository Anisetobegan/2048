using UnityEngine;
using UnityEngine.EventSystems;

public class Cell : MonoBehaviour, IPointerClickHandler
{
    int x, y;
    Block occupiedBlock = null;
    bool isMerging = false;

    public Block OccupiedBlock { get { return occupiedBlock; } }
    public bool IsMerging { get { return isMerging; } }

    public void FillCell(Block newBlock)
    {
        occupiedBlock = newBlock;
    }

    public void ClearCell()
    {
        occupiedBlock = null;
    }

    public void BeginMerging()
    {
        isMerging = true;
    }

    public void EndMerging()
    {
        isMerging = false;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log($"[{x}, {y}]");
    }

    public void SetArrayPosition(int x, int y)
    {
        this.x = x;
        this.y = y;
    }
}
