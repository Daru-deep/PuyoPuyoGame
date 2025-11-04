using UnityEngine;

public class BallDeleter : MonoBehaviour
{

    private GameObject gmOB;
    private GameManager gameManager;
    private GameManager.BallColor mycolor;
    public int nowI;
    public int nowN;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        gmOB = GameObject.Find("GameManager");
        gameManager = gmOB.GetComponent<GameManager>();
        
    }

   public void SetMyColor(GameManager.BallColor get)
    {
        Color chengeColor;
        mycolor = get;
        if (mycolor == GameManager.BallColor.red)
        {
          chengeColor = new Color(100, 0, 0);
        }
        else if (mycolor == GameManager.BallColor.blue) 
        {
            chengeColor = new Color(0, 100, 0);
        }
        else if(mycolor == GameManager.BallColor.green)
        {
        
            chengeColor = new Color(0, 0, 100);
        }
        else
        { 
            chengeColor = new Color(0,0,0);
        }


        GetComponent<SpriteRenderer>().color = chengeColor;
    }

    public GameManager.BallColor GetMyColor()
    {
        return mycolor;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ChengeMyTag(string tagName)
    {
        gameObject.tag = tagName;
    }

   /* void DestroyBalls()
    {
        GameObject[] destroyPoint = new GameObject[gameManager.boardHeight*gameManager.boardWidth];

        for (int currentCol = 0; currentCol < destroyPoint.Length; currentCol++)
        {
            if (nowI >= 1 && nowI <= gameManager.boardWidth - 1 && nowN >= 1 && nowN <= gameManager.boardHeight - 1)
            {


                if (gameManager.LeadBorld(nowI % gameManager.boardWidth, nowN) == gameManager.LeadBorld(nowI - 1 % gameManager.boardWidth, nowN)||
                    gameManager.)//¶‚ª“¯‚¶Žž
                {

                    //if (gameManager.LeadBorld(nowI, nowN) == gameManager.LeadBorld();
                    destroyPoint[0] = gameManager.borld[nowI, nowN];

                    gameManager.borld[(nowI - 1 + gameManager.boardWidth) % gameManager.boardWidth, nowN] = null;

                }
            }
        }
        
    }
   */

    public void SetPosition(int i,int n)
    {
        nowI = i;
        nowN = n;
    }
}
