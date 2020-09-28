using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;


public class Board_Manager : MonoBehaviour
{
    // values array to put value for each piece
    public static Dictionary<char, int> values_of_pieces = new Dictionary<char, int>();
    public Chessman[,] tmp_chessmans = new Chessman[8, 8];
    KeyValuePair<int, int> initPos, finPos;

    public int calculate_value_nodes = 0;

    public static Board_Manager Instance { set; get; }
    private bool[,] allowedMoves { set; get; }

    public Chessman[,] Chessmans { set; get; }
    private Chessman selectedChessman;

    private const float TILE_SIZE = 1.0F;
    private const float TILE_OFFSET = 0.5F;

    private int selectionX = -1;
    private int selectionY = -1;
    private int alpha = -9999999, beta = 9999999;

    public List<GameObject> chessManPrefabs;
    private List<GameObject> activeChessMan;

    public bool isWhiteTurn = true;

    private void Start()
    {
        // initializing the values of pieces
        values_of_pieces.Add('p', 1); // pawn 
        values_of_pieces.Add('b', 3); // bishop 
        values_of_pieces.Add('n', 3); // knight 
        values_of_pieces.Add('r', 5); // Rook
        values_of_pieces.Add('q', 9); // queen
        values_of_pieces.Add('k', 20); // king

        Instance = this;
        SpawnAllChessMans();
    }
    void Update()
    {
        Debug.Log("update = " + Sellected_Difficulty.difficulty_level);
        UpdateSelection();
        DrawChessBoard();
        if (!isWhiteTurn)
        {
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    tmp_chessmans[j, i] = Chessmans[j, i];
                }
            }
            if (!Check_mate())
            {
                // diffucilty
                int dif = 3;
                Debug.Log(dif);
                if (Sellected_Difficulty.difficulty_level == 0)
                    dif = 3;
                else if (Sellected_Difficulty.difficulty_level == 1)
                    dif = 5;
                else if (Sellected_Difficulty.difficulty_level == 2)
                    dif = 7;
                Debug.Log(dif);
                Debug.Log("sel = "+ Sellected_Difficulty.difficulty_level);


                var watch = System.Diagnostics.Stopwatch.StartNew();
                maximize(dif, ' ', dif, alpha, beta);
                watch.Stop();
                var elapsedMS = watch.ElapsedMilliseconds;
                print("elaspedMS: "+elapsedMS);
                if (Chessmans[initPos.Key, initPos.Value] != null)
                {
                    SelectChessman(initPos.Key, initPos.Value);
                    move_chessman_black(finPos.Key, finPos.Value);
                }
                else
                {
                    Debug.Log("no initial");
                    isWhiteTurn = true;
                }
            }
            else
            {
                if (black_moves())
                {
                    SelectChessman(initPos.Key, initPos.Value);
                    move_chessman_black(finPos.Key, finPos.Value);
                    if (check_mate_white())
                    {
                        Debug.Log("check mate white");
                        // highlight it red
                    }
                }
                else
                {
                    SceneManager.LoadScene(2);
                    //EndGame();
                }
            }
        }
        //move
        if (Input.GetMouseButtonDown(0) && isWhiteTurn)
        {
            if (selectionX >= 0 && selectionY >= 0)
            {
                if (selectedChessman == null)
                {
                    //select 
                    SelectChessman(selectionX, selectionY);
                }
                else
                {
                    //Move
                    MoveChessMan(selectionX, selectionY);
                }
            }
        }
    }

    private void SelectChessman(int x, int y)
    {
        if (Chessmans[x, y] == null)
            return;
        if (Chessmans[x, y].isWhite != isWhiteTurn)
            return;
        allowedMoves = Chessmans[x, y].possibleMoves();
        selectedChessman = Chessmans[x, y];
        BoardHighlights.Instance.HighlightAllowedMoves(allowedMoves);

    }
    private void MoveChessMan(int x, int y)
    {
        if (allowedMoves[x, y])
        {
            //distroy piece
            Chessman c = Chessmans[x, y];
            if (c != null && c.isWhite != isWhiteTurn)
            {
                ////end Game
                //if (c.GetType() == typeof(King))
                //{
                //    EndGame();
                //    return;
                //}
               
                activeChessMan.Remove(c.gameObject);
                Destroy(c.gameObject);
            }
            if (selectedChessman.GetType() == typeof(Pawn))
            {
                Promotion_pawn(x, y);
            }
            Chessmans[selectedChessman.currentX, selectedChessman.currentY] = null;
            selectedChessman.transform.position = GetTileCenter(x, y);
            selectedChessman.SetPosition(x, y);
            Chessmans[x, y] = selectedChessman;
            isWhiteTurn = !isWhiteTurn;
        }
        BoardHighlights.Instance.HideHighlights();
        selectedChessman = null;
    }
    private void move_chessman_black(int x, int y)
    {
        if (allowedMoves[x, y])
        {
            Chessman c = Chessmans[x, y];
            if (c != null && c.isWhite != isWhiteTurn)
            {
                //end Game
                if (c.GetType() == typeof(King))
                {
                    EndGame();
                    return;
                }
             
                activeChessMan.Remove(c.gameObject);
                Destroy(c.gameObject);
            }
            if (selectedChessman.GetType() == typeof(Pawn))
            {
                Promotion_pawn(x, y);
            }
            Chessmans[selectedChessman.currentX, selectedChessman.currentY] = null;
            selectedChessman.transform.position = GetTileCenter(x, y);
            selectedChessman.SetPosition(x, y);
            Chessmans[x, y] = selectedChessman;
            isWhiteTurn = !isWhiteTurn;
        }
        BoardHighlights.Instance.HideHighlights();
        selectedChessman = null;
    }
    private void DrawChessBoard()
    {
        //draw board
        Vector3 widthLine = Vector3.right * 8;
        Vector3 hightLine = Vector3.forward * 8;

        for (int i = 0; i <= 8; i++)
        {
            Vector3 start = Vector3.forward * i;
            Debug.DrawLine(start, start + widthLine);
            for (int j = 0; j <= 8; j++)
            {
                start = Vector3.right * j;
                Debug.DrawLine(start, start + hightLine);
            }
        }
        //Draw the selection
        if (selectionX >= 0 && selectionY >= 0)
        {
            Debug.DrawLine(
                Vector3.forward * selectionY + Vector3.right * selectionX,
                Vector3.forward * (selectionY + 1) + Vector3.right * (selectionX + 1));

            Debug.DrawLine(
                Vector3.forward * (selectionY + 1) + Vector3.right * selectionX,
                Vector3.forward * selectionY + Vector3.right * (selectionX + 1));
        }


    }

    //curser on Tile
    private void UpdateSelection()
    {
        if (!Camera.main)
            return;
        RaycastHit hit;
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 25.0f, LayerMask.GetMask("ChessPlane")))
        {
            selectionX = (int)hit.point.x;
            selectionY = (int)hit.point.z;
        }
        else
        {
            selectionX = -1;
            selectionY = -1;
        }
    }

    private void SpawnChessMan(int index, int x, int y)
    {
        GameObject go = Instantiate(chessManPrefabs[index], GetTileCenter(x, y), Quaternion.identity) as GameObject;
        go.transform.SetParent(transform);
        Chessmans[x, y] = go.GetComponent<Chessman>();
        Chessmans[x, y].SetPosition(x, y);
        activeChessMan.Add(go);
    }

    private void SpawnAllChessMans()
    {
        activeChessMan = new List<GameObject>();
        Chessmans = new Chessman[8, 8];
        //white team
        //(index in list, x, y)
        //king
        SpawnChessMan(0, 3, 0);
        //queen
        SpawnChessMan(1, 4, 0);
        //Rook
        SpawnChessMan(2, 0, 0);
        SpawnChessMan(2, 7, 0);
        //Bishops
        SpawnChessMan(3, 2, 0);
        SpawnChessMan(3, 5, 0);
        //knights
        SpawnChessMan(4, 1, 0);
        SpawnChessMan(4, 6, 0);
        //pawns
        for (int i = 0; i < 8; i++)
        {
            SpawnChessMan(5, i, 1);
        }
        //white team
        //king
        SpawnChessMan(6, 4, 7);
        //queen
        SpawnChessMan(7, 3, 7);
        //Rook
        SpawnChessMan(8, 0, 7);
        SpawnChessMan(8, 7, 7);
        //Bishops
        SpawnChessMan(9, 2, 7);
        SpawnChessMan(9, 5, 7);
        //knights
        SpawnChessMan(10, 1, 7);
        SpawnChessMan(10, 6, 7);
        //pawns
        for (int i = 0; i < 8; i++)
        {
            SpawnChessMan(11, i, 6);
        }


    }
    private Vector3 GetTileCenter(int x, int y)
    {
        Vector3 origin = Vector3.zero;
        origin.x += (TILE_SIZE * x) + TILE_OFFSET;
        origin.z += (TILE_SIZE * y) + TILE_OFFSET;
        return origin;
    }

    private void EndGame()
    {
        if (isWhiteTurn)
            Debug.Log("white team wins");
        else
            Debug.Log("Black team wins");

        //foreach (GameObject go in activeChessMan)
        //    Destroy(go);
        isWhiteTurn = true;
        // BoardHighlights.Instance.HideHighlights();
        // SpawnAllChessMans();
    }

    int maximize(int depth, char type, int N, int alpha, int beta)
    {
        if (depth == 0)
        {
            return evaluation();
        }
        int Max = -9999999; //Max value for maximizing
        bool kill = false; //kill and kill type if these's option to kill
        char killed_type = ' ';
        for (int i = 0; i < 8; i++) // loop on the whole chess board moving through ever piece
        {
            for (int j = 0; j < 8; j++)
            {
                if (tmp_chessmans[j, i] != null && !tmp_chessmans[j, i].isWhite) //if there's a piece and it's black (AI is Black)...
                {
                    bool[,] possible = tmp_chessmans[j, i].possibleMoves(); //get all it's possible solutions
                    char tmp_type = get_chessman_type(j, i); //get the type of this current black piece 
                    bool alphaBetaBreakBool = false; //when we break from the inner loop we want to break from the outer loop also
                    Chessman c = tmp_chessmans[j, i];
                    for (int y = 0; y < 8; y++)//loop on this black piece's possible moves
                    {
                        for (int x = 0; x < 8; x++)
                        {
                            if (possible[x, y])//check if it's a valid move
                            {
                                if (tmp_chessmans[x, y] != null && tmp_chessmans[x, y].isWhite) //check if this the square we are moving to is empty and check if it's white(The player)
                                {
                                    kill = true; // then there's some kill 
                                    killed_type = get_chessman_type(x, y); /// we take the killed one
                                }
                                move(c, i, j, x, y);
                                int ret = minimize(depth - 1, tmp_type, N, alpha, beta); //go to the next level
                                if (kill)
                                {
                                    ret += (values_of_pieces[killed_type] * 30);
                                    kill = false;
                                }
                                undo_move(c, i, j, x, y);
                                if (ret > Max && depth == N)
                                {
                                    initPos = new KeyValuePair<int, int>(j, i);
                                    finPos = new KeyValuePair<int, int>(x, y);
                                    Max = ret;
                                }
                                else
                                {
                                    Max = Mathf.Max(Max, ret);
                                }
                                //Alph-Beta Part
                                alpha = Mathf.Max(alpha, ret);
                                if (beta <= alpha)
                                {
                                    alphaBetaBreakBool = true;
                                    break;
                                }
                            }
                        }
                        if (alphaBetaBreakBool)
                        {
                            break;
                        }
                    }
                }
            }
        }
        return Max;
    }

    int minimize(int depth, char type, int N, int alpha, int beta)
    {
        if (depth == 0)
        {
            return evaluation() * -1;
        }
        int Min = 9999999;
        bool kill = false;
        char killed_type = ' ';
        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                if (tmp_chessmans[j, i] != null && tmp_chessmans[j, i].isWhite)
                {
                    bool[,] possible = tmp_chessmans[j, i].possibleMoves();
                    char tmp_type = get_chessman_type(j, i);
                    bool alphaBetaBreakBool = false; //when we break from the inner loop we want to break from the outer loop also
                    Chessman c = tmp_chessmans[j, i];
                    for (int y = 0; y < 8; y++)
                    {
                        for (int x = 0; x < 8; x++)
                        {
                            if (possible[x, y])
                            {
                                if (tmp_chessmans[x, y] != null && !tmp_chessmans[x, y].isWhite)
                                {
                                    kill = true;
                                    killed_type = get_chessman_type(x, y);
                                }
                                move(c, i, j, x, y);
                                int ret = maximize(depth - 1, tmp_type, N, alpha, beta);
                                if (kill)
                                {
                                    ret -= (values_of_pieces[killed_type] * 30);
                                    kill = false;
                                }
                                if (ret <= Min && depth == N)
                                {
                                    initPos = new KeyValuePair<int, int>(j, i);
                                    finPos = new KeyValuePair<int, int>(x, y);
                                    Min = ret;
                                }
                                undo_move(c, i, j, x, y);
                                Min = Mathf.Min(Min, ret);
                                //Alph-Beta Part
                                beta = Mathf.Min(beta, ret);
                                if (beta <= alpha)
                                {
                                    alphaBetaBreakBool = true;
                                    break;
                                }
                            }
                        }
                        if (alphaBetaBreakBool)
                        {
                            break;
                        }
                    }
                }
            }
        }
        return Min;
    }

    private char get_chessman_type(int i, int j)
    {
        if (tmp_chessmans[i, j].GetType().ToString() == "King")
        {
            return 'k';
        }
        else if (tmp_chessmans[i, j].GetType().ToString() == "Queen")
        {
            return 'q';
        }
        else if (tmp_chessmans[i, j].GetType().ToString() == "Rook")
        {
            return 'r';
        }
        else if (tmp_chessmans[i, j].GetType().ToString() == "Knight")
        {
            return 'n';
        }
        else if (tmp_chessmans[i, j].GetType().ToString() == "Bishop")
        {
            return 'b';
        }
        else if (tmp_chessmans[i, j].GetType().ToString() == "Pawn")
        {
            return 'p';
        }
        return ' ';
    }

    private bool black_moves()
    {
        int Minimize_value = 999999;
        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                if (tmp_chessmans[j, i] != null && !tmp_chessmans[j, i].isWhite)
                {
                    bool[,] positions = tmp_chessmans[j, i].possibleMoves();
                    Chessman c = tmp_chessmans[j, i];
                    for (int x = 0; x < 8; x++)
                    {
                        for (int y = 0; y < 8; y++)
                        {
                            if (positions[y, x])
                            {
                                move(c, i, j, y, x);
                                if (Check_mate())
                                {
                                    undo_move(c, i, j, y, x);
                                }
                                else
                                {
                                    undo_move(c, i, j, y, x);


                                    Minimize_value = values_of_pieces[get_chessman_type(j, i)];
                                    initPos = new KeyValuePair<int, int>(j, i);
                                    finPos = new KeyValuePair<int, int>(y, x);
                                    return true;
                                }
                            }
                        }
                    }
                }
            }
        }
        return false;
    }
    private void Promotion_pawn(int x, int y)
    {
        if (y == 7)
        {
            activeChessMan.Remove(selectedChessman.gameObject);
            Destroy(selectedChessman.gameObject);
            SpawnChessMan(1, x, y);
            selectedChessman = Chessmans[x, y];
        }
        else if (y == 0)
        {
            activeChessMan.Remove(selectedChessman.gameObject);
            Destroy(selectedChessman.gameObject);
            SpawnChessMan(7, x, y);
            selectedChessman = Chessmans[x, y];

        }
    }
    private bool Check_mate()
    {
        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                if (tmp_chessmans[j, i] != null && tmp_chessmans[j, i].isWhite)
                {
                    bool[,] positions = tmp_chessmans[j, i].possibleMoves();
                    for (int x = 0; x < 8; x++)
                    {
                        for (int y = 0; y < 8; y++)
                        {
                            if (positions[y, x] && tmp_chessmans[y, x] != null && tmp_chessmans[y, x].GetType() == typeof(King))
                            {
                                return true;
                            }
                        }
                    }
                }
            }
        }
        return false;
    }

    public int evaluation()
    {
        int nwb = 0, nbb = 0, // bishop
            nwr = 0, nbr = 0, // rook
            nwn = 0, nbn = 0, // knight
            nwp = 0, nbp = 0, // pawn
            nwq = 0, nbq = 0, // queen
            nwk = 0, nbk = 0; // kings

        int additional = 0;

        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                if (tmp_chessmans[j, i] != null)
                {
                    char type = get_chessman_type(j, i);
                    if (type == 'k')
                    {
                        if (tmp_chessmans[j, i].isWhite)
                        {
                            nwk++;
                        }
                        else
                        {
                            nbk++;
                        }
                    }
                    else if (type == 'q')
                    {
                        if (tmp_chessmans[j, i].isWhite)
                        {
                            nwq++;
                        }
                        else
                        {
                            nbq++;
                        }
                    }
                    else if (type == 'r')
                    {
                        if (tmp_chessmans[j, i].isWhite)
                        {
                            nwr++;
                           
                        }
                        else
                        {
                            nbr++;
                         
                        }
                    }
                    else if (type == 'n')
                    {
                        if (tmp_chessmans[j, i].isWhite)
                        {
                            nwn++;
                          
                        }
                        else
                        {
                            nbn++;
                         
                        }
                    }
                    else if (type == 'b')
                    {
                        if (tmp_chessmans[j, i].isWhite)
                        {
                            nwb++;
                         
                        }
                        else
                        {
                            nbb++;
                         
                        }
                    }
                    else if (type == 'p')
                    {
                        if (tmp_chessmans[j, i].isWhite)
                        {
                            nwp++;
                          
                        }
                        else
                        {
                            nbp++;
                      
                        }
                    }
                }
            }
        }

        if (Check_mate())
        {
            return (-99999999);
        }
        else if (check_mate_white())
        {
            additional = 200;
        }
        return (((nbp * 1) - (nwp * 1)) +
              ((nbn * 3) - (nwn * 3)) +
              ((nbb * 3) - (nwb * 3)) +
              ((nbr * 5) - (nwr * 5)) +
              ((nbq * 9) - (nwq * 9)) +
              ((nbk * 20) - (nwk * 20))) + additional;
    }
    public void undo_move(Chessman c, int i, int j, int x, int y)
    {
        tmp_chessmans[j, i] = c;
        c.SetPosition(j, i);
        tmp_chessmans[x, y] = null;
    }

    public void move(Chessman c, int i, int j, int x, int y)
    {

        tmp_chessmans[x, y] = c;
        c.SetPosition(x, y);
        tmp_chessmans[j, i] = null;
    }

    public bool check_mate_white()
    {
        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                if (tmp_chessmans[j, i] != null && !tmp_chessmans[j, i].isWhite)
                {
                    bool[,] possible = tmp_chessmans[j, i].possibleMoves();
                    for (int y = 0; y < 8; y++)
                    {
                        for (int x = 0; x < 8; x++)
                        {
                            if (possible[x, y] && tmp_chessmans[x, y] != null && tmp_chessmans[x, y].GetType().ToString() == "King")
                            {

                                return true;
                            }
                        }
                    }
                }
            }
        }
        return false;
    }

}
