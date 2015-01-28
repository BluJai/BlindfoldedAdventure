using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;

public class GameManager : MonoBehaviour {
    public static GameManager instance;
    public Text LivesText;
    public Text TileDisappearsText;
    public Text LevelText;
    public Text CollectedText;
    public GameObject LevelPrefab;
    public GameObject PlayerPrefab;
    public GameObject FloorTilePrefab;
    public GameObject GameOverHUD;
    public GameObject YouWinHUD;
    public AudioClip RequestUp;
    public AudioClip RequestDown;
    public AudioClip RequestLeft;
    public AudioClip RequestRight;
    public int FreeTurns = 5;
    public int TurnsBetweenTileDrop = 3;
    private int Lives = 3;
    public int SecondsPerTileDrop = 3;
    private float tileCountdown = 0f;
    public GameObject CollectiblePrefab;
    public GameObject GoalPrefab;
    public int Level { get; private set; }

    public AudioClip AdvisePositive;
    public AudioClip AdviseNegative;

    private Vector3 origin = new Vector3(0f,0f,0f);
    //private GameObject currentPlayerObject;
    //private GameObject currentLevelObject;
    private List<PlayerMovement> playerMovements;
    public int TurnNumber { get { return playerMovements == null ? 0 : playerMovements.Count + 1; } }
    private PlayerMovement currentMovementRequest;

    public bool PlayerCanSubmitMove
    {
        get
        {
            return currentMovementRequest != null
                   &&
                   (!currentMovementRequest.RequestDirection.HasValue
                    || (currentMovementRequest.Advice.HasValue && !currentMovementRequest.FinalDirection.HasValue)
                       )
                ;
        }
    }

    public TurnPhase CurrentPhase { get; private set; }

    public enum TurnPhase {
        ProposeAction = 1,
        Advise = 2,
        PerformAction = 3
    }

    //public void PlayerRight()
    //{
    //    ChangeDirection(MovementDirection.Right);
    //}
    //public void PlayerLeft()
    //{
    //    ChangeDirection(MovementDirection.Left);
    //}
    //public void PlayerDown()
    //{
    //    ChangeDirection(MovementDirection.Down);
    //}
    //public void PlayerUp()
    //{
    //    ChangeDirection(MovementDirection.Up);
    //}


    public void AdviseRight()
    {
        //ChangeDirection(PlayerMovement.MovementDirection.Right);
    }
    public void AdviseLeft()
    {
        //ChangeDirection(PlayerMovement.MovementDirection.Left);
    }
    public void AdviseDown()
    {
        //ChangeDirection(PlayerMovement.MovementDirection.Down);
    }
    public void AdviseUp()
    {
        //ChangeDirection(PlayerMovement.MovementDirection.Up);
    }

    private void DropTile()
    {
        if (currentTileCollection == null) { return;}
        if (currentTileCollection.Count == 0) { return; }
        int tileToDrop = UnityEngine.Random.Range(0, currentTileCollection.Count);
        KeyValuePair<XYPoint, GameObject> kvp = currentTileCollection.ElementAt(tileToDrop);
        Destroy(kvp.Value);
        currentTileCollection.Remove(kvp.Key);
    }

    public void AdviseYes()
    {
        DoAdvise(true);
    }

    public void AdviseNo()
    {
        DoAdvise(false);
    }

    private void DoAdvise(bool doIt)
    {
        if (CurrentPhase != TurnPhase.Advise) {
            return;
        }
        if (currentMovementRequest.Advice.HasValue) { return;}
        AudioSource.PlayClipAtPoint(doIt ? AdvisePositive : AdviseNegative, origin);
        currentMovementRequest.Advice = doIt;
        CurrentPhase = TurnPhase.PerformAction;
    }
     
    //private void ChangeDirection(MovementDirection direction)
    //{
    //    LogCurrentRequest(currentMovementRequest);
    //    switch (CurrentPhase) {
    //        case TurnPhase.ProposeAction:
    //            if (currentMovementRequest != null && currentMovementRequest.RequestDirection == direction) {
    //                return;
    //            }
    //            if (currentMovementRequest == null) {
    //                currentMovementRequest = new PlayerMovement {RequestDirection = direction};
    //            }
    //            else {
    //                currentMovementRequest.RequestDirection = direction;
    //            }
    //            PlayCurrentRequestSound();
    //            CurrentPhase = TurnPhase.Advise;
    //            break;
    //        case TurnPhase.Advise:
    //            break;
    //        case TurnPhase.PerformAction:
    //            if (currentMovementRequest != null) {
    //                currentMovementRequest.FinalDirection = direction;
    //            }
    //            break;
    //        default:
    //            throw new ArgumentOutOfRangeException();
    //    }
    //}
    private void StartNextLevel()
    {
        Level++;
        ResetLevel();
    }
    private void LogCurrentRequest(PlayerMovement playerMovement)
    {
        if (playerMovement == null) {
            Debug.Log("playerMovement is null");
            return;
        }
        string toPrint = "Initial Direction: ";
        if (playerMovement.RequestDirection.HasValue) {
            toPrint += playerMovement.RequestDirection.ToString();
        }
        if (playerMovement.Advice.HasValue) {
            toPrint += " advice: " + playerMovement.Advice.ToString();
        }
        if (playerMovement.FinalDirection.HasValue) {
            toPrint += " final: " + playerMovement.FinalDirection.ToString();
        }
        Debug.Log(toPrint);
    }

    //public void PlayerMove()
    //{
    //    if (currentMovementRequest == null || currentMovementRequest.FinalDirection == null) {
    //        return;
    //    }
    //    switch (currentMovementRequest.FinalDirection.Value) {
    //        case MovementDirection.Up:
    //            currentPlayerObject.transform.Translate(0f, 0f, 1f);
    //            break;
    //        case MovementDirection.Down:
    //            currentPlayerObject.transform.Translate(0f, 0f, -1f);
    //            break;
    //        case MovementDirection.Left:
    //            currentPlayerObject.transform.Translate(-1f, 0f, 0f);
    //            break;
    //        case MovementDirection.Right:
    //            currentPlayerObject.transform.Translate(1f, 0f, 0f);
    //            break;
    //        default:
    //            throw new ArgumentOutOfRangeException("currentMovementRequest.FinalDirection");
    //    }
    //    Rigidbody playerRigidbody = currentPlayerObject.GetComponent<Rigidbody>();
    //    RaycastHit hit;
    //    playerRigidbody.SweepTest(new Vector3(0f, -50f, 0f), out hit, 50f);
    //    Debug.Log("Distance to obj: " + hit.distance.ToString());
    //    if (hit.distance < 0.1f) {
    //        AdvanceTurn(currentMovementRequest);
    //    }
    //    else {
    //        // THERE'S NOTHING THERE!
    //        FallOff();
    //    }
    //    UpdateTileDisappearsText();
    //}

    private int NextTileDisappears
    {
        get
        {
            if (TurnNumber == 0) return 0;
            if (TurnNumber < FreeTurns) return FreeTurns - TurnNumber;
            return ((TurnNumber - FreeTurns) % TurnsBetweenTileDrop);
        }
    }
    public void StopPlayerMovement()
    {
        //Rigidbody playerRigidbody = currentPlayerObject.GetComponent<Rigidbody>();
        //playerRigidbody.isKinematic = true;
    }
    private void ResetLevel()
    {
        //if (currentPlayerObject != null) { Destroy(currentPlayerObject);}
        ResetPlayerObject();
        playerMovements.Clear();
        //if (currentLevelObject != null) {
        //    Destroy(currentLevelObject);
        //} 
        ResetLevelObject();
        tileCountdown = SecondsPerTileDrop;
        if (currentMovementRequest == null) {currentMovementRequest = new PlayerMovement();}
        goalsReachedThisLevel = 0;
        ResetCollectibleLocs();
        AddGoalObject();
    }
    private readonly Vector3 OffscreenGoalLoc = new Vector3(9000f, 0.56f, 9000f);
    private readonly Vector3 OffscreenCollectibleLoc = new Vector3(-9000f,0.747f,-9000f);

    //private void AdvanceTurn(PlayerMovement currentMovement)
    //{
    //    if (currentMovement != null) {
    //        playerMovements.Add(currentMovement);
    //    }
    //    currentMovementRequest = new PlayerMovement();
    //    CurrentPhase = TurnPhase.ProposeAction;
    //    if (TurnNumber > 0 && NextTileDisappears == 0)
    //        DropTile();
    //}

    //private void FallOff()
    //{
    //    Debug.Log("Falloff was triggered");
    //    Rigidbody playerRigidbody = currentPlayerObject.GetComponent<Rigidbody>();
    //    playerRigidbody.isKinematic = false;
    //    playerMovements.Add(currentMovementRequest);
    //    currentMovementRequest = null;
    //}

    public void LoseLife()
    {
        Lives--;
        if (Lives > 0) {
            UpdateHUD();
            ResetLevel();
        }
        else {
            CountdownActive = false;
            if (GameOverHUD != null)
                GameOverHUD.SetActive(true);
            LivesText.gameObject.SetActive(false);
            AudioSource.PlayClipAtPoint(GameOverSound, Vector3.zero);
            isGameOver = true;
        }
    }

    private bool isGameOver = false;

    public AudioClip GameOverSound;

    private void PlayCurrentRequestSound()
    {
        if (currentMovementRequest == null) {
            return;
        }
        //TODO: prevent playing while other sounds are playing
        switch (currentMovementRequest.RequestDirection) {
            case MovementDirection.Up:
                AudioSource.PlayClipAtPoint(RequestUp, origin);
                break;
            case MovementDirection.Down:
                AudioSource.PlayClipAtPoint(RequestDown, origin);
                break;
            case MovementDirection.Left:
                AudioSource.PlayClipAtPoint(RequestLeft, origin);
                break;
            case MovementDirection.Right:
                AudioSource.PlayClipAtPoint(RequestRight, origin);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public void CancelDirection()
    {
        if (currentMovementRequest == null)return;
        if (currentMovementRequest.RequestDirection.HasValue && CurrentPhase == TurnPhase.ProposeAction)
            currentMovementRequest.RequestDirection = null;
        if (CurrentPhase == TurnPhase.PerformAction && currentMovementRequest.FinalDirection.HasValue)
            currentMovementRequest.FinalDirection = null;
    }
    public void Awake()
    {
        if (instance == null) instance = this;
        else{ if(instance != this)Destroy(gameObject);}
        Setup();
    }

    public void Setup()
    {
        isGameOver = false;
        GameOverHUD.SetActive(false);
        LivesText.gameObject.SetActive(true);
        Lives = 3;
        if (playerMovements == null) playerMovements = new List<PlayerMovement>();
        Level = 1;
        ResetLevelObject();
        ResetPlayerObject();
        ResetCollectibleLocs();
        AddGoalObject();
        CountdownActive = true;
        UpdateHUD();
    }
    #region HUD

    public void UpdateHUD()
    {
        UpdateLivesDisplay();
        UpdateTileDisappearsDisplay();
        UpdateLevelText();
        UpdateCollectedText();
    }
    private void UpdateLivesDisplay()
    {
       if (LivesText != null) {
            LivesText.text = String.Format("Lives: {0}", Lives);
        } 
    }
    private void UpdateTileDisappearsDisplay()
    {
        if (TileDisappearsText != null)
        {
            int disappearsIn = NextTileDisappears;
            TileDisappearsText.text = disappearsIn == 0
                ? "Tile disappears this turn"
                : String.Format("Next tile disappears in {0:F1} seconds", tileCountdown);
        }
    }

    private void UpdateLevelText()
    {
        if (LevelText != null) {
            LevelText.text = String.Format("Level {0}", Level);
        }
    }

    private void UpdateCollectedText()
    {
        if (CollectedText != null) {
            CollectedText.text = String.Format("{0} Gems Collected", goalsReachedThisLevel);
        }
    }
    #endregion
    #region level objects
    private const int LevelWidth = 7;
    private const int LevelHeight = 5;
    private void ResetLevelObject()
    {
        if (currentTileCollection != null)
        {
            foreach (var o in currentTileCollection)
            {
                Destroy(o.Value);
            }
            currentTileCollection.Clear();
        }
        currentTileCollection = GenerateLevel(LevelWidth, LevelHeight);

    }
    private readonly Vector3 tileScale = new Vector3(1f, 1f, 1f);
    private Dictionary<XYPoint, GameObject> currentTileCollection; 

    private Dictionary<XYPoint, GameObject> GenerateLevel(int width, int height)
    {
        float middleX = (float)width / 2f;
        float middleY = (float)height / 2f;
        Dictionary<XYPoint, GameObject> level = new Dictionary<XYPoint, GameObject>();
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                GameObject tile = Instantiate(FloorTilePrefab, origin, Quaternion.identity) as GameObject;
                ApplyScale(tile, Vector3.one);
                float destinationX = (((float)x + 1f) < middleX) ? 0 - x : x - middleX;
                float destinationY = (((float)y + 1f) < middleY) ? 0 - y : y - middleY;
                Vector3 destination = new Vector3(destinationX, 0f, destinationY);
                tile.transform.position=destination;
                level[new XYPoint(x, y)] = tile;
            }
        }
        return level;
    }

    #endregion

    #region Collectibles and Goals
    private int goalsReachedThisLevel;
    private const float desiredGoalScale = 0.8f;

    private readonly Vector3 goalScale = new Vector3(1.6f * desiredGoalScale,4.365f*desiredGoalScale,1f*desiredGoalScale);

    public void ReachGoal()
    {
        //if (currentGoalObject == null) {
        //    return;
        //}
        //Destroy(currentGoalObject);
        if (goalsReachedThisLevel < (Level)) {
            goalsReachedThisLevel++;
            AddGoalObject();
        } 
        else {
            Level++;
            ResetLevel();
        }
    }

    private void ResetCollectibleLocs()
    {
        if (CollectiblePrefab != null) {
            CollectiblePrefab.transform.position = OffscreenCollectibleLoc;
        }
        if (GoalPrefab != null) {
            GoalPrefab.transform.position = OffscreenGoalLoc;
        }
    }

    private void AddGoalObject()
    {
        float middleX = (float)LevelWidth / 2f;
        float middleY = (float)LevelHeight / 2f;
        bool isFinalGoal = goalsReachedThisLevel == Level;
        ResetCollectibleLocs();
        int randX = -1;
        int randY = -1;
        while (!currentTileCollection.ContainsKey(new XYPoint(randX, randY))) {
            randX = UnityEngine.Random.Range(0, LevelWidth);
            randY = UnityEngine.Random.Range(0, LevelHeight);
        }
        GameObject currentGoalObject = isFinalGoal ? GoalPrefab : CollectiblePrefab;
        float destinationX = (((float)randX + 1f) < middleX) ? 0 - randX : randX - middleX;
        float destinationY = (((float)randY + 1f) < middleY) ? 0 - randY : randY - middleY;
        Vector3 destination = new Vector3(destinationX, currentGoalObject.transform.position.y, destinationY);
        currentGoalObject.transform.position = destination; 
    }

    #endregion

    private readonly Vector3 PlayerOrigin = new Vector3(0f, 0.476f, 0f);
    private void ResetPlayerObject()
    {
        //currentPlayerObject = Instantiate(PlayerPrefab, origin, Quaternion.identity) as GameObject;
        //if (currentPlayerObject != null) {
        //    Renderer playerRenderer = currentPlayerObject.GetComponent<Renderer>();
        //    if (playerRenderer != null) {
        //        float playerDepth = playerRenderer.bounds.max.y - playerRenderer.bounds.min.y;
        //        currentPlayerObject.transform.Translate(new Vector3(0f, playerDepth + 2f, 0f));

        //        ApplyScale(currentPlayerObject, playerScale); 
        //    }
        //} 
        PlayerPrefab.transform.position = PlayerOrigin;
        PlayerPrefab.transform.rotation = Quaternion.identity;
    }

    private void ApplyScale(GameObject targetGameObject, Vector3 targetScale)
    {
        //var size = targetGameObject.transform.localScale;
        ////var scale = new Vector3(targetScale.x / size.x, targetScale.y / size.y, targetScale.z / size.z); 
        //var scale = new Vector3(size.x / targetScale.x, size.y / targetScale.y, size.z / targetScale.z);
        targetGameObject.transform.localScale = targetScale;
    }

    // Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update ()
	{
        if (isGameOver && Input.anyKeyDown)
        {
	        Setup();
            return;
        }
        if (!CountdownActive) return;
	    tileCountdown -= Time.deltaTime;
        UpdateHUD();
	    if (tileCountdown <= 0f) {
	        DropTile();
	        tileCountdown = (float) SecondsPerTileDrop;
	    }
	}

    private bool _countdownActive = true;

    private bool CountdownActive
    {
        get
        {
            return _countdownActive;
        }
        set
        {
            _countdownActive = value;
            if (_countdownActive)
                tileCountdown = SecondsPerTileDrop;
            TileDisappearsText.gameObject.SetActive(_countdownActive);
        }
    }
    private readonly Vector3 playerScale = new Vector3(0.75f, 0.75f, 0.75f);
    

    internal class XYPoint : IEquatable<XYPoint> {
        public bool Equals(XYPoint other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return X == other.X && Y == other.Y;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((XYPoint) obj);
        }

        public override int GetHashCode()
        {
            unchecked {
                return (X*397) ^ Y;
            }
        }

        public static bool operator ==(XYPoint left, XYPoint right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(XYPoint left, XYPoint right)
        {
            return !Equals(left, right);
        }

        public XYPoint(int x, int y)
        {
            X = x;
            Y = y;
        }

        public int X { get; private set; }
        public int Y { get; private set; }
    }

    internal class PlayerMovement {
        public MovementDirection? RequestDirection { get; set; }
        public bool? Advice { get; set; }
        public MovementDirection? AdviceDirection { get; set; }
        public MovementDirection? FinalDirection { get; set; }
    }
        public enum MovementDirection { Up,Down,Left,Right}
}
