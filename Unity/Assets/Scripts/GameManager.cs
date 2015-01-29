using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEngine.UI;

using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
    public enum MovementDirection
    {
        Up,
        Down,
        Left,
        Right
    }

    public enum TurnPhase
    {
        ProposeAction = 1,
        Advise = 2,
        PerformAction = 3
    }

    public static GameManager instance;
    private bool _countdownActive = true;
    public AudioClip AdviseNegative;
    public AudioClip AdvisePositive;
    public Text CollectedText;
    public GameObject CollectiblePrefab;
    private PlayerMovement currentMovementRequest;
    public GameObject FloorTilePrefab;
    public int FreeTurns = 5;
    public GameObject GameOverHUD;
    public AudioClip GameOverSound;
    public GameObject GoalPrefab;
    private bool isGameOver;
    public GameObject LevelPrefab;
    public Text LevelText;
    private int Lives = 3;
    public Text LivesText;
    private readonly Vector3 OffscreenCollectibleLoc = new Vector3(-9000f, 0.747f, -9000f);
    private readonly Vector3 OffscreenGoalLoc = new Vector3(9000f, 0.56f, 9000f);
    private readonly Vector3 origin = new Vector3(0f, 0f, 0f);
    //private GameObject currentPlayerObject;
    //private GameObject currentLevelObject;
    private List<PlayerMovement> playerMovements;
    private readonly Vector3 PlayerOrigin = new Vector3(0f, 0.476f, 0f);
    public GameObject PlayerPrefab;
    private readonly Vector3 playerScale = new Vector3(0.75f, 0.75f, 0.75f);
    public AudioClip RequestDown;
    public AudioClip RequestLeft;
    public AudioClip RequestRight;
    public AudioClip RequestUp;
    public int SecondsPerTileDrop = 3;
    private float tileCountdown;
    public Text TileDisappearsText;
    public int TurnsBetweenTileDrop = 3;
    public GameObject YouWinHUD;

    private bool CountdownActive
    {
        get { return _countdownActive; }
        set
        {
            _countdownActive = value;
            if (_countdownActive)
                tileCountdown = SecondsPerTileDrop;
            TileDisappearsText.gameObject.SetActive(_countdownActive);
        }
    }

    public TurnPhase CurrentPhase { get; private set; }
    public int Level { get; private set; }
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
            if (TurnNumber == 0)
                return 0;
            if (TurnNumber < FreeTurns)
                return FreeTurns - TurnNumber;
            return ((TurnNumber - FreeTurns) % TurnsBetweenTileDrop);
        }
    }

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

    public int TurnNumber { get { return playerMovements == null ? 0 : playerMovements.Count + 1; } }

    public void AdviseDown()
    {
        //ChangeDirection(PlayerMovement.MovementDirection.Down);
    }

    public void AdviseLeft()
    {
        //ChangeDirection(PlayerMovement.MovementDirection.Left);
    }

    public void AdviseNo()
    {
        DoAdvise(false);
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

    public void AdviseUp()
    {
        //ChangeDirection(PlayerMovement.MovementDirection.Up);
    }

    public void AdviseYes()
    {
        DoAdvise(true);
    }

    private void ApplyScale(GameObject targetGameObject, Vector3 targetScale)
    {
        targetGameObject.transform.localScale = targetScale;
    }

    public void Awake()
    {
        if (instance == null)
            instance = this;
        else
        {
            if (instance != this)
                Destroy(gameObject);
        }
        Setup();
    }

    public void CancelDirection()
    {
        if (currentMovementRequest == null)
            return;
        if (currentMovementRequest.RequestDirection.HasValue && CurrentPhase == TurnPhase.ProposeAction)
            currentMovementRequest.RequestDirection = null;
        if (CurrentPhase == TurnPhase.PerformAction && currentMovementRequest.FinalDirection.HasValue)
            currentMovementRequest.FinalDirection = null;
    }

    private void DoAdvise(bool doIt)
    {
        if (CurrentPhase != TurnPhase.Advise)
            return;
        if (currentMovementRequest.Advice.HasValue)
            return;
        AudioSource.PlayClipAtPoint(doIt ? AdvisePositive : AdviseNegative, origin);
        currentMovementRequest.Advice = doIt;
        CurrentPhase = TurnPhase.PerformAction;
    }

    private void DropTile()
    {
        if (currentTileCollection == null)
            return;
        if (currentTileCollection.Count == 0)
            return;
        int tileToDrop = Random.Range(0, currentTileCollection.Count);
        KeyValuePair<XYPoint, GameObject> kvp = currentTileCollection.ElementAt(tileToDrop);
        Destroy(kvp.Value);
        currentTileCollection.Remove(kvp.Key);
    }

    private void LogCurrentRequest(PlayerMovement playerMovement)
    {
        if (playerMovement == null)
        {
            Debug.Log("playerMovement is null");
            return;
        }
        string toPrint = "Initial Direction: ";
        if (playerMovement.RequestDirection.HasValue)
            toPrint += playerMovement.RequestDirection.ToString();
        if (playerMovement.Advice.HasValue)
            toPrint += " advice: " + playerMovement.Advice;
        if (playerMovement.FinalDirection.HasValue)
            toPrint += " final: " + playerMovement.FinalDirection;
        Debug.Log(toPrint);
    }

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
        if (Lives > 0)
        {
            UpdateHUD();
            ResetLevel();
        }
        else
        {
            CountdownActive = false;
            if (GameOverHUD != null)
                GameOverHUD.SetActive(true);
            LivesText.gameObject.SetActive(false);
            AudioSource.PlayClipAtPoint(GameOverSound, Vector3.zero);
            isGameOver = true;
        }
    }

    private void PlayCurrentRequestSound()
    {
        if (currentMovementRequest == null)
            return;
        //TODO: prevent playing while other sounds are playing
        switch (currentMovementRequest.RequestDirection)
        {
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
        if (currentMovementRequest == null)
            currentMovementRequest = new PlayerMovement();
        goalsReachedThisLevel = 0;
        ResetCollectibleLocs();
        AddGoalObject();
    }

    private void ResetPlayerObject()
    {
        PlayerPrefab.transform.position = PlayerOrigin;
        PlayerPrefab.transform.rotation = Quaternion.identity;
    }

    public void Setup()
    {
        isGameOver = false;
        GameOverHUD.SetActive(false);
        LivesText.gameObject.SetActive(true);
        Lives = 3;
        if (playerMovements == null)
            playerMovements = new List<PlayerMovement>();
        Level = 1;
        ResetLevelObject();
        ResetPlayerObject();
        ResetCollectibleLocs();
        AddGoalObject();
        CountdownActive = true;
        UpdateHUD();
    }

    // Use this for initialization
    private void Start() { }
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

    public void StopPlayerMovement()
    {
        //Rigidbody playerRigidbody = currentPlayerObject.GetComponent<Rigidbody>();
        //playerRigidbody.isKinematic = true;
    }

    // Update is called once per frame
    private void Update()
    {
        if (isGameOver && Input.anyKeyDown)
        {
            Setup();
            return;
        }
        if (!CountdownActive)
            return;
        tileCountdown -= Time.deltaTime;
        UpdateHUD();
        if (tileCountdown <= 0f)
        {
            DropTile();
            tileCountdown = SecondsPerTileDrop;
        }
    }

    internal class PlayerMovement
    {
        public bool? Advice { get; set; }
        public MovementDirection? AdviceDirection { get; set; }
        public MovementDirection? FinalDirection { get; set; }
        public MovementDirection? RequestDirection { get; set; }
    }

    internal class XYPoint : IEquatable<XYPoint>
    {
        #region Constructors

        public XYPoint(int x, int y)
        {
            X = x;
            Y = y;
        }

        #endregion

        public int X { get; private set; }
        public int Y { get; private set; }

        #region Implementations

        public bool Equals(XYPoint other)
        {
            if (ReferenceEquals(null, other))
                return false;
            if (ReferenceEquals(this, other))
                return true;
            return X == other.X && Y == other.Y;
        }

        #endregion

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            if (obj.GetType() != GetType())
                return false;
            return Equals((XYPoint)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (X * 397) ^ Y;
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
        if (LivesText != null)
            LivesText.text = String.Format("Lives: {0}", Lives);
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
        if (LevelText != null)
            LevelText.text = String.Format("Level {0}", Level);
    }

    private void UpdateCollectedText()
    {
        if (CollectedText != null)
            CollectedText.text = String.Format("{0} Gems Collected", goalsReachedThisLevel);
    }

    #endregion

    #region level objects

    private const int LevelWidth = 7;
    private const int LevelHeight = 5;

    private void ResetLevelObject()
    {
        if (currentTileCollection != null)
        {
            foreach (KeyValuePair<XYPoint, GameObject> o in currentTileCollection)
                Destroy(o.Value);
            currentTileCollection.Clear();
        }
        currentTileCollection = GenerateLevel(LevelWidth, LevelHeight);
    }

    private readonly Vector3 tileScale = new Vector3(1f, 1f, 1f);
    private Dictionary<XYPoint, GameObject> currentTileCollection;

    private Dictionary<XYPoint, GameObject> GenerateLevel(int width, int height)
    {
        float middleX = width / 2f;
        float middleY = height / 2f;
        float top = 0 - middleY;
        float left = 0 - middleX;
        Dictionary<XYPoint, GameObject> level = new Dictionary<XYPoint, GameObject>();
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                GameObject tile = Instantiate(FloorTilePrefab, origin, Quaternion.identity) as GameObject;
                ApplyScale(tile, Vector3.one);
                float destinationX = left + x;
                float destinationY = top + y;
                Vector3 destination = new Vector3(destinationX, 0f, destinationY);
                tile.transform.position = destination;
                level[new XYPoint(x, y)] = tile;
            }
        }
        return level;
    }

    #endregion

    #region Collectibles and Goals

    private int goalsReachedThisLevel;
    private const float desiredGoalScale = 0.8f;

    private readonly Vector3 goalScale = new Vector3(1.6f * desiredGoalScale, 4.365f * desiredGoalScale, 1f * desiredGoalScale);

    public void ReachGoal()
    {
        //if (currentGoalObject == null) {
        //    return;
        //}
        //Destroy(currentGoalObject);
        if (goalsReachedThisLevel < (Level))
        {
            goalsReachedThisLevel++;
            AddGoalObject();
        }
        else
        {
            Level++;
            ResetLevel();
        }
    }

    private void ResetCollectibleLocs()
    {
        if (CollectiblePrefab != null)
            CollectiblePrefab.transform.position = OffscreenCollectibleLoc;
        if (GoalPrefab != null)
            GoalPrefab.transform.position = OffscreenGoalLoc;
    }

    private void AddGoalObject()
    {
        bool isFinalGoal = goalsReachedThisLevel == Level;
        ResetCollectibleLocs();
        int randX = -1;
        int randY = -1;
        while (!currentTileCollection.ContainsKey(new XYPoint(randX, randY)))
        {
            randX = Random.Range(0, LevelWidth);
            randY = Random.Range(0, LevelHeight);
        }
        int tileOffset = Random.Range(0, currentTileCollection.Keys.Count);
        GameObject randomTile = currentTileCollection.ElementAt(tileOffset).Value;
        GameObject currentGoalObject = isFinalGoal ? GoalPrefab : CollectiblePrefab;
        Vector3 destination = new Vector3(randomTile.transform.position.x, currentGoalObject.transform.position.y, randomTile.transform.position.z);
        currentGoalObject.transform.position = destination;
    }

    #endregion
}