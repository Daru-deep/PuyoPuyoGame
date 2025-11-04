using Mono.Cecil;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq.Expressions;
using TMPro;
using Unity.Burst.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI.Table;

public class GameManager : MonoBehaviour
{
    [Header("ゲーム設定")]
    [SerializeField] private int boardHeight = 13;
    [SerializeField] private int boardWidth = 9;
    [SerializeField] private float cellSize = 1f;
    [SerializeField] private float fallSpeed = 1f;
    [SerializeField] private int minMatchCount = 3;

    [Header("ゲームオブジェクト")]
    [SerializeField] private GameObject ballPrefab;
    [SerializeField] private GameObject[] ballPrefabs;
    [SerializeField] private GameObject textOb;


    int currentCol = 12;
    int currentRow = 4;
    public bool fallloop = true;//実行スイッチ
    
    public GameObject[,] borld;
    private Vector2Int coordinates;
    private int[] columnHeights;
    private GameObject playingBall;
    [SerializeField] private Transform zero;
    [SerializeField] private int endPoint;


    // Start is called once before the first execution of Update after the MonoBehaviour is created

    public enum BallColor{
        red, green, blue, alpha
        }
   private GameObject InstanceBall()
    {
        
        BallColor color = ColorRandomization();

        GameObject instance = Instantiate(ballPrefab);
        instance.GetComponent<BallDeleter>().SetMyColor(color);

        return instance;
    }

   private BallColor ColorRandomization()
    {
        int rand = 0;
        rand = new System.Random().Next(0, 3);
        if (rand == 0)
        {
            return BallColor.red;
        }
        else if (rand == 1)
        {
            return BallColor.green;
        }
        else if(rand == 2) 
        {
            return BallColor.blue;
        }
        else 
        {
            return BallColor.alpha;
        }
    }

    TextMeshProUGUI tmp;
    void controlFall(bool enter)
    {
        fallloop = enter; 
        
    }

    void Start()
    {
       tmp  = textOb.GetComponent<TextMeshProUGUI>();
       
        //DestroyBall();
        borld = new GameObject[boardHeight, boardWidth];
        columnHeights = new int[boardWidth] ;


        //最初のボールの出現
        playingBall = InstanceBall();
        currentCol = boardHeight - 1;
        currentRow = (boardWidth - 1) / 2;
        playingBall.transform.position = IndexToWorldPos(currentCol, currentRow);
        controlFall(true);
        Debug.Log("columnHeights="+columnHeights[0]);
        StartCoroutine(FallLoop());
    }


    // Update is called once per frame
    void Update()
    {

   

        if (Input.GetKeyDown(KeyCode.A))
        {
            ControllObject(false);
        }
        if (Input.GetKeyDown(KeyCode.D))
        {
            ControllObject(true);
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            if (currentCol > columnHeights[currentRow]&&currentCol>0)
            {
                currentCol--;
            }
        }
        playingBall.transform.position = IndexToWorldPos(currentCol,currentRow);

        
        
    }


    GameObject GetBorld(int localCol, int localRow)
    {
        localCol = Mathf.Clamp(localCol, 0, boardHeight-1);
        localRow = Mathf.Clamp(localRow, 0, boardWidth-1);
        return borld[localCol, localRow];
    }
    void ChengeBorld(int localCol, int localRow, GameObject getObject)
    {
        localCol = Mathf.Clamp(localCol, 0, boardHeight - 1);
        localRow = Mathf.Clamp(localRow, 0, boardWidth - 1);
        borld[localCol, localRow] = getObject;
    }
    Vector3 IndexToWorldPos(int localCol, int localRow)
    {

        localCol = Mathf.Clamp(localCol, 0, boardHeight - 1);
        localRow = Mathf.Clamp(localRow, 0, boardWidth - 1);


        return zero.position + new Vector3(localRow * cellSize,localCol * cellSize,0);

    }
    
    IEnumerator FallLoop()
    {
        

        while (true)
        {
            
            yield return new WaitForSeconds(fallSpeed);
            
            if (currentCol <= columnHeights[currentRow])//横列の立てのストック
            {
                
                int hitRow = currentRow;
                int hitCol = currentCol;
                ChengeBorld(hitCol, hitRow, playingBall);
                OnBallLanded(hitCol, hitRow, playingBall);
            }
            else if (fallloop) 
            { 
                currentCol--;
            }
            else
            {
                continue;
            }
        }
    }
    
    

    void OnBallLanded(int hitCol,int hitRow,GameObject landedBall)
    {
        columnHeights[hitRow] = Mathf.Max(columnHeights[hitRow], hitCol + 1);

        DestroyBall();
        ApplyGravity();
        SpawnNewBall();

    }

    void SpawnNewBall()
    {
        currentCol = boardHeight-1;
        currentRow = (boardWidth-1)/2;

        if (columnHeights[currentRow] >= boardHeight)
        {
            Debug.Log("Game Over!");
            fallloop = false;
            return;
        }

        playingBall = InstanceBall();
        playingBall.transform.position = IndexToWorldPos(currentCol, currentRow);


    }





    void ControllObject(bool right)
    {
        int nextI;
        if (right)
        {

            
           
            nextI = (currentRow + 1) % boardWidth;
        }
        else
        {
            nextI = (currentRow - 1 + boardWidth) % boardWidth;
        }

        if (columnHeights[nextI] <= currentCol)
        {
            currentRow = nextI;
        }
            
    }

    float point = 0;
    float operand = 0;  //被乗数
    float multiple = 0; //乗数

    void PointCounter()
    {
        float sum = operand * multiple;
        point += sum;
        operand = 0;
        multiple = 0;
        tmp.text = ($"ポイント追加！\n：現在{point}点！！");
        fallloop = true;
        if(point > 100)
        {
            tmp.text = ("GAME_CLEAR");
            fallloop= false;
        }
    }

    void DestroyBall()
    {
        bool[,] visited = new bool[boardHeight, boardWidth];
        bool foundMatch = false;

        for (int col = 0; col < boardHeight; col++)
        {
            for (int row = 0; row < boardWidth; row++)
            {
                if(borld[col, row] != null && !visited[col,row])
                {
                    List<Vector2Int> connectedBalls =
                        FindConnectedBalls(col, row, visited);

                    if (connectedBalls.Count >= minMatchCount)
                    {
                        Debug.Log($"マッチ発見: Count={connectedBalls.Count}");
                        //ポイント計算
                        operand+= connectedBalls.Count;
                        

                        //ボールを削除
                        foreach (Vector2Int pos in connectedBalls)
                        {
                            Destroy(borld[pos.x, pos.y]);
                            borld[pos.x, pos.y] = null;
                        }
                        foundMatch = true;
                    }
                }
            }
        }
        if(foundMatch)
        {
            RecalcNextPos();
            StartCoroutine(CheckChainReaction());
        }

    }



    List<Vector2Int> FindConnectedBalls(int startCol, int startRow , bool[,] visited)
    {
        List<Vector2Int> result = new List<Vector2Int> ();
        Queue<Vector2Int> queue = new Queue<Vector2Int>();

        BallColor targetColor = borld[startCol, startRow].GetComponent<BallDeleter>().GetMyColor();

        queue.Enqueue(new Vector2Int(startCol, startRow));
        visited[startCol, startRow] = true;

        int[] dCol = { 1, -1, 0, 0 };
        int[] dRow = { 0, 0, 1, -1 };

        while (queue.Count > 0)
        {
            Vector2Int current = queue.Dequeue();
            result.Add(current);

            for(int i = 0; i < 4; i++)
            {
                int newCol = current.x + dCol[i];
                int newRow = current.y + dRow[i];

                if(newCol < 0 || newCol >= boardHeight ||
                    newRow < 0 || newRow >= boardWidth)
                {
                    continue;
                }

                if (visited[newCol, newRow])
                {
                    continue;
                }

                if(borld[newCol, newRow] == null)
                {
                    continue;
                }

                if (borld[newCol,newRow].GetComponent<BallDeleter>().GetMyColor() != targetColor)
                {
                    continue;
                }

                visited[newCol, newRow] = true;
                queue.Enqueue(new Vector2Int(newCol, newRow));
            }


        }
        return result;
    }


    IEnumerator CheckChainReaction()
    {
        fallloop = false;
        yield return new WaitForSeconds(fallSpeed * (1f*0.7f));
        ApplyGravity();
        yield return new WaitForSeconds(fallSpeed * (1f * 0.3f));

        bool[,] visited = new bool[boardHeight, boardWidth];
        bool hasMatch = false;

        for (int col = 0; col < boardHeight; col++)
        {
            for (int row = 0; row < boardWidth; row++)
            {
                
                    if (borld[col, row] != null && !visited[col, row])
                    {
                        List<Vector2Int> connectedBalls =FindConnectedBalls(col, row , visited);
                       


                        if (connectedBalls.Count >= minMatchCount)
                        {
                            hasMatch = true;
                            break;
                        }
                    }
                
            }
            if (hasMatch) break;
        }
        if (hasMatch)
        {
            Debug.Log("連鎖発生！");
            multiple++;
            DestroyBall();
        }

        if (!hasMatch) PointCounter();
        fallloop = true;
     
    }
    void ApplyGravity()
    {
        for (int row = 0; row < boardWidth; row++) // 各横位置で
        {
            int writeIndex = 0; // 書き込み位置（下から）

            // 下から上に向かって詰める
            for (int readIndex = 0; readIndex < boardHeight; readIndex++)
            {
                if (borld[readIndex, row] != null)
                {
                    if (readIndex != writeIndex)
                    {
                        // ボールを下に詰める
                        borld[writeIndex, row] = borld[readIndex, row];
                        borld[readIndex, row] = null;
                        borld[writeIndex, row].transform.position =
                            IndexToWorldPos(writeIndex, row);
                    }
                    writeIndex++;
                }
            }
        }
        
        RecalcNextPos();
    }

    void RecalcNextPos()
    {
        for(int c = 0; c < boardWidth; c++)
        {
            int top = 0;
            for (int r = 0; r < boardHeight; r++) 
            {
                if (borld[r, c] != null)
                {
                    top = r+1 ;
                }
            }
            columnHeights[c] = top;

        }
    }


        
        
    }

       

