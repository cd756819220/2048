using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

enum MoveType
{
    none = 0,
    left = 1,
    right = 2,
    up = 3,
    down = 4
}

public class Game : MonoBehaviour
{
    private Vector2 touchFirst = Vector2.zero;
    private Vector2 touchEnd = Vector2.zero;
    public GameObject MoveItem = null;
    private RectTransform gameBg = null;
    private Transform replayBtn;
    private Text timeText;
    public Color[] moveItemColorList;
    public Color[] moveNumColorList;
    public int maxIndex = 11;

    public int RowNum = 4, ColNum = 4;
    private Dictionary<int, Dictionary<int, MoveItem>> moveItemMap;

    public bool isCreateMoveItem = true;

    private List<MoveItem> _cacheMoveItemList;
    private int _curTime = 0;
    //private GameObject[] moveItem

    // Start is called before the first frame update
    void Start()
    {
        moveItemMap = new Dictionary<int, Dictionary<int, MoveItem>>();
        _cacheMoveItemList = new List<MoveItem>();
        for (int rowIndex = 0; rowIndex < RowNum; rowIndex++)
        {
            Dictionary<int, MoveItem> oneRow = new Dictionary<int, MoveItem>();
            for (int colIndex = 0; colIndex < ColNum; colIndex++)
            {
                oneRow.Add(colIndex, null);
            }
            moveItemMap.Add(rowIndex, oneRow);
        }
        gameBg = this.transform.Find("Bg").Find("GameBg").transform as RectTransform;
        replayBtn = this.transform.Find("ButtonReplay");
        timeText = this.transform.Find("TextTime").GetComponent<Text>();
        this._startCreateGame();
    }

    private bool _checkIsEmptyItem(MoveItem moveItem)
    {
        return moveItem == null;
    }

    private void _timeCountStart()
    {
        CancelInvoke("_timeCount");
        _curTime = 0;
        InvokeRepeating("_timeCount", 1.0f, 1.0f);
        _timeCount();
    }

    private void _timeCount()
    {
        var hour = Mathf.FloorToInt(_curTime / 3600);
        var min = Mathf.FloorToInt((_curTime % 3600) / 60);
        var sec = _curTime % 60;
        Debug.Log(Mathf.Floor(2.6f));
        this.timeText.text = string.Format("time: {0:d2}:{1:d2}:{2:d2}", hour, min, sec);
        _curTime++;
    }

    public void onClickReplay()
    {
        for (var rowIndex = 0; rowIndex < RowNum; rowIndex++)
        {
            for (var colIndex = 0; colIndex < ColNum; colIndex++)
            {
                if (!_checkIsEmptyItem(moveItemMap[rowIndex][colIndex]))
                {
                    this._cacheMoveItemList.Add(moveItemMap[rowIndex][colIndex]);
                    moveItemMap[rowIndex][colIndex].gameObject.SetActive(false);
                    moveItemMap[rowIndex][colIndex] = null;
                }
            }
        }
        this._startCreateGame();
    }

    private void _startCreateGame()
    {
        _randomCreateMoveItem();
        _randomCreateMoveItem();
        this._timeCountStart();
    }

    private void _randomCreateMoveItem(int row = -1, int col = -1, int index = -1)
    {
        if (isCreateMoveItem == false) return;
        List<int> emptyIndexList = new List<int>();
        foreach(var rowIt in moveItemMap)
        {
            foreach(var colIt in rowIt.Value)
            {
                if (_checkIsEmptyItem(colIt.Value))
                {
                    emptyIndexList.Add(rowIt.Key * ColNum + colIt.Key);
                }
            }
        }
        if (emptyIndexList.Count == 0) return;
        var newCreateIndex = emptyIndexList[Mathf.RoundToInt(UnityEngine.Random.value*(emptyIndexList.Count - 1))];
        row = row >= 0 ? row : Mathf.FloorToInt(newCreateIndex / ColNum);
        col = col >= 0 ? col : newCreateIndex % ColNum;
        var moveItem = _createMoveItem();
        var transform = moveItem.transform as RectTransform;
        moveItem.transform.SetParent(gameBg, false);
        setMoveItemPosByRowAndCol(moveItem.transform, row, col);
        var moveItemScript = moveItem.GetComponent<MoveItem>();
        index = index > 0 ? index : (UnityEngine.Random.value <= 0.6 ? 1 : 2);
        moveItemScript.setMoveItemValue(index, moveItemColorList[index - 1], moveNumColorList[index - 1]);
        moveItemMap[row][col] = moveItem;
    }

    private void setMoveItemPosByRowAndCol(Transform transform, int row, int col)
    {
        Vector2 leftTopPos = new Vector2(-150 * 1.5f, -150 * -1.5f);
        transform.localPosition = new Vector3(leftTopPos.x + col * 150, leftTopPos.y - row * 150, 0);
    }

    private MoveItem _createMoveItem()
    {
        if (_cacheMoveItemList.Count > 0)
        {
            var moveItem = _cacheMoveItemList[_cacheMoveItemList.Count - 1];
            _cacheMoveItemList.RemoveAt(_cacheMoveItemList.Count - 1);
            moveItem.gameObject.SetActive(true);
            moveItem.index = -1;
            return moveItem;
        }
        return Instantiate(MoveItem).GetComponent<MoveItem>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnGUI()
    {
        if (Event.current.type == EventType.MouseDown)//判断当前手指是按下事件
        {
            touchFirst = Event.current.mousePosition;//记录开始按下的位置
            //Debug.Log("touchFirst: " + touchFirst.x + "_" + touchFirst.y);
        }
        else if (Event.current.type == EventType.MouseUp)
        {
            touchEnd = Event.current.mousePosition;
            //Debug.Log("touchEnd: " + touchEnd.x + "_" + touchEnd.y);
            this._checkMoveDirection();
        }
    }

    private void _checkMoveDirection()
    {
        if (Vector2.Distance(touchFirst, touchEnd) < 20)
        {
            return;
        }
        MoveType moveType = MoveType.none;
        var dir = (touchEnd - touchFirst).normalized;
        if (dir.y <= 0)
        {
            if (Mathf.Abs(dir.x) <= -dir.y)
            {
                moveType = MoveType.up;
            }
            else
            {
                moveType = dir.x > 0 ? MoveType.right : MoveType.left;
            }
            
        }
        else if (dir.y > 0)
        {
            if (Mathf.Abs(dir.x) <= dir.y)
            {
                moveType = MoveType.down;
            }
            else
            {
                moveType = dir.x > 0 ? MoveType.right : MoveType.left;
            }

        }

        Debug.Log("moveType: " + moveType.ToString());
        this._moveItems(moveType);
    }

    private void _moveItems(MoveType moveType)
    {
        if (moveType == MoveType.up)
        {
            this._moveUp();
        }
        else if (moveType == MoveType.down)
        {
            this._moveDown();
        }
        else if (moveType == MoveType.left)
        {
            this._moveLeft();
        }
        else if (moveType == MoveType.right)
        {
            this._moveRight();
        }
        else
        {
            Debug.Log("no move type! " + moveType.ToString());
            return;
        }
        this._randomCreateMoveItem();
        //Debug.Log("result=======================");
        //for (var rowIndex = 0; rowIndex < RowNum; rowIndex++)
        //{
        //    for (var colIndex = 0; colIndex < ColNum; colIndex++)
        //    {
        //        Debug.Log(rowIndex + "_" + colIndex + ": " + (moveItemMap[rowIndex][colIndex] ? moveItemMap[rowIndex][colIndex].index : -1));
        //    }
        //}
    }

    private void _moveRight()
    {
        int curRow = 0, curCol = 0;
        for (var rowIndex = 0; rowIndex < RowNum; rowIndex++)
        {
            curRow = rowIndex;
            for (var colIndex = ColNum-1; colIndex >= 0; colIndex--)
            {
                curCol = colIndex;
                var curValue = this.moveItemMap[curRow][curCol];
                if (!_checkIsEmptyItem(curValue))
                {
                    for (var checkColIndex = colIndex + 1; checkColIndex < ColNum; checkColIndex++)
                    {
                        var tempValue = this.moveItemMap[curRow][checkColIndex];
                        if (_checkIsEmptyItem(tempValue))
                        {
                            this.moveItemMap[curRow][curCol] = null;
                            setMoveItemPosByRowAndCol(curValue.transform, curRow, checkColIndex);
                            this.moveItemMap[curRow][checkColIndex] = curValue;
                            curCol = checkColIndex;
                        }
                        else
                        {
                            if (curValue.index == tempValue.index)
                            {
                                this._cacheMoveItemList.Add(curValue);
                                curValue.gameObject.SetActive(false);
                                this.moveItemMap[curRow][curCol] = null;
                                this.moveItemMap[curRow][checkColIndex].setMoveItemValue(curValue.index + 1, moveItemColorList[curValue.index], moveNumColorList[curValue.index]);
                            }
                            break;
                        }
                    }
                }
            }
        }
    }

    private void _moveLeft()
    {
        int curRow = 0, curCol = 0;
        for (var rowIndex = 0; rowIndex < RowNum; rowIndex++)
        {
            curRow = rowIndex;
            for (var colIndex = 0; colIndex < ColNum; colIndex++)
            {
                curCol = colIndex;
                var curValue = this.moveItemMap[curRow][curCol];
                if (!_checkIsEmptyItem(curValue))
                {
                    for (var checkColIndex = colIndex - 1; checkColIndex >= 0; checkColIndex--)
                    {
                        var tempValue = this.moveItemMap[curRow][checkColIndex];
                        if (_checkIsEmptyItem(tempValue))
                        {
                            this.moveItemMap[curRow][curCol] = null;
                            setMoveItemPosByRowAndCol(curValue.transform, curRow, checkColIndex);
                            this.moveItemMap[curRow][checkColIndex] = curValue;
                            curCol = checkColIndex;
                        }
                        else
                        {
                            if (curValue.index == tempValue.index)
                            {
                                this._cacheMoveItemList.Add(curValue);
                                curValue.gameObject.SetActive(false);
                                this.moveItemMap[curRow][curCol] = null;
                                this.moveItemMap[curRow][checkColIndex].setMoveItemValue(curValue.index + 1, moveItemColorList[curValue.index], moveNumColorList[curValue.index]);
                            }
                            break;
                        }
                    }
                }
            }
        }
    }

    private void _moveDown()
    {
        int curRow = 0, curCol = 0;
        for (var colIndex = 0; colIndex < ColNum; colIndex++)
        {
            curCol = colIndex;
            for (var rowIndex = RowNum - 1; rowIndex >= 0; rowIndex--)
            {
                curRow = rowIndex;
                var curValue = this.moveItemMap[curRow][curCol];
                if (!_checkIsEmptyItem(curValue))
                {
                    for (var checkRowIndex = rowIndex + 1; checkRowIndex < RowNum; checkRowIndex++)
                    {
                        var tempValue = this.moveItemMap[checkRowIndex][curCol];
                        if (_checkIsEmptyItem(tempValue))
                        {
                            this.moveItemMap[curRow][curCol] = null;
                            setMoveItemPosByRowAndCol(curValue.transform, checkRowIndex, curCol);
                            this.moveItemMap[checkRowIndex][curCol] = curValue;
                            curRow = checkRowIndex;
                        }
                        else
                        {
                            if (curValue.index == tempValue.index)
                            {
                                this._cacheMoveItemList.Add(curValue);
                                curValue.gameObject.SetActive(false);
                                this.moveItemMap[curRow][curCol] = null;
                                this.moveItemMap[checkRowIndex][curCol].setMoveItemValue(curValue.index + 1, moveItemColorList[curValue.index], moveNumColorList[curValue.index]);
                            }
                            break;
                        }
                    }
                }
            }
        }
    }

    private void _moveUp()
    {
        int curRow = 0, curCol = 0;
        for (var colIndex = 0; colIndex < ColNum; colIndex++)
        {
            curCol = colIndex;
            for (var rowIndex = 0; rowIndex < RowNum; rowIndex++)
            {
                curRow = rowIndex;
                var curValue = this.moveItemMap[curRow][curCol];
                if (!_checkIsEmptyItem(curValue))
                {
                    for (var checkRowIndex = rowIndex - 1; checkRowIndex >= 0; checkRowIndex--)
                    {
                        var tempValue = this.moveItemMap[checkRowIndex][curCol];
                        if (_checkIsEmptyItem(tempValue))
                        {
                            this.moveItemMap[curRow][curCol] = null;
                            setMoveItemPosByRowAndCol(curValue.transform, checkRowIndex, curCol);
                            this.moveItemMap[checkRowIndex][curCol] = curValue;
                            curRow = checkRowIndex;
                        }
                        else
                        {
                            if (curValue.index == tempValue.index)
                            {
                                this._cacheMoveItemList.Add(curValue);
                                curValue.gameObject.SetActive(false);
                                this.moveItemMap[curRow][curCol] = null;
                                this.moveItemMap[checkRowIndex][curCol].setMoveItemValue(curValue.index + 1, moveItemColorList[curValue.index], moveNumColorList[curValue.index]);
                            }
                            break;
                        }
                    }
                }
            }
        }
    }
}
