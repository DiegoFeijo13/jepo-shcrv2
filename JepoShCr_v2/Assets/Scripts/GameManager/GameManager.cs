using Assets.Scripts.GameManager;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class GameManager : SingletonMonoBehaviour<GameManager>
{
    [SerializeField] private TextMeshProUGUI messageTextTMP;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private List<DungeonLevelSO> dungeonLevelList;
    [SerializeField] private int currentDungeonLevelListIndex = 0;

    private Room currentRoom;
    private Room previousRoom;
    private PlayerDetailsSO playerDetails;
    private Player player;
    private long gameScore;
    private int scoreMultiplier;
    private InstantiatedRoom bossRoom;

    [HideInInspector] public GameState gameState;
    [HideInInspector] public GameState previousGameState;

    protected override void Awake()
    {
        // Call base class
        base.Awake();

        // Set player details - saved in current player scriptable object from the main menu
        playerDetails = GameResources.Instance.currentPlayer.playerDetails;

        // Instantiate player
        InstantiatePlayer();

    }

    private void Start()
    {
        previousGameState = GameState.gameStarted;
        gameState = GameState.gameStarted;

        gameScore = 0;
        scoreMultiplier = 1;

        StartCoroutine(Fade(0f, 1f, 0f, Color.black));
    }

    private void Update()
    {
        HandleGameState();

        //// For testing
        //if (Input.GetKeyDown(KeyCode.P))
        //{
        //    gameState = GameState.gameStarted;
        //}

    }

    private void OnEnable()
    {
        StaticEventHandler.OnRoomChanged += StaticEventHandler_OnRoomChanged;
        StaticEventHandler.OnRoomEnemiesDefeated += StaticEventHandler_OnRoomEnemiesDefeated;
        StaticEventHandler.OnPointScored += StaticEventHandler_OnPointScored;
        StaticEventHandler.OnMultiplier += StaticEventHandler_OnMultiplier;

        player.destroyedEvent.OnDestroyed += Player_OnDestroyed;
    }

    private void OnDisable()
    {        
        StaticEventHandler.OnRoomChanged -= StaticEventHandler_OnRoomChanged;
        StaticEventHandler.OnRoomEnemiesDefeated -= StaticEventHandler_OnRoomEnemiesDefeated;
        StaticEventHandler.OnPointScored -= StaticEventHandler_OnPointScored;
        StaticEventHandler.OnMultiplier -= StaticEventHandler_OnMultiplier;

        player.destroyedEvent.OnDestroyed -= Player_OnDestroyed;
    }

    private void InstantiatePlayer()
    {
        GameObject playerGameObject = Instantiate(playerDetails.playerPrefab);

        player = playerGameObject.GetComponent<Player>();

        player.Initialize(playerDetails);
    }

    private void StaticEventHandler_OnRoomChanged(RoomChangedEventArgs args)
    {
        SetCurrentRoom(args.room);
    }

    private void StaticEventHandler_OnPointScored(PointScoredArgs args)
    {
        gameScore += args.points * scoreMultiplier;

        StaticEventHandler.CallScoreChangedEvent(gameScore, scoreMultiplier);
    }

    private void StaticEventHandler_OnMultiplier(MultiplierArgs args)
    {
        if (args.multiplier)
            scoreMultiplier++;
        else
            scoreMultiplier--;

        scoreMultiplier = Mathf.Clamp(scoreMultiplier, 1, 30);

        StaticEventHandler.CallScoreChangedEvent(gameScore, scoreMultiplier);
    }

    private void StaticEventHandler_OnRoomEnemiesDefeated(RoomEnemiesDefeatedArgs args)
    {
        RoomEnemiesDefeated();
    }

    private void Player_OnDestroyed(DestroyedEvent destroyedEvent, DestroyedEventArgs args)
    {
        previousGameState = gameState;
        gameState = GameState.gameLost;
    }

    private void HandleGameState()
    {        
        switch (gameState)
        {
            case GameState.gameStarted:
                
                PlayDungeonLevel(currentDungeonLevelListIndex);

                gameState = GameState.playingLevel;

                RoomEnemiesDefeated();

                break;

            case GameState.levelCompleted:
                StartCoroutine(LevelCompleted());
                break;
            case GameState.gameWon:
                if (previousGameState != GameState.gameWon)
                    StartCoroutine(GameWon());
                break;
            case GameState.gameLost:
                if (previousGameState != GameState.gameLost)
                {
                    StopAllCoroutines();
                    StartCoroutine(GameLost());
                }
                break;
            case GameState.restartGame:
                RestartGame();
                break;
        }
    }

    public void SetCurrentRoom(Room room)
    {
        previousRoom = currentRoom;
        currentRoom = room;
    }

    private void RoomEnemiesDefeated()
    {
        bool isDungeonClearOfRegularEnemies = true;
        bossRoom = null;

        foreach (var keyValuePair in DungeonBuilder.Instance.dungeonBuilderRoomDictionary)
        {
            if(keyValuePair.Value.roomNodeType.isBossRoom)
            {
                bossRoom = keyValuePair.Value.instantiatedRoom;
                continue;
            }

            if(!keyValuePair.Value.isClearedOfEnemies)
            {
                isDungeonClearOfRegularEnemies = false;
                break;
            }
        }

        if((isDungeonClearOfRegularEnemies && bossRoom == null) || (isDungeonClearOfRegularEnemies && bossRoom.room.isClearedOfEnemies))
        {
            if(currentDungeonLevelListIndex < dungeonLevelList.Count -1) 
            {
                gameState = GameState.levelCompleted;
            }
        }
        else if(isDungeonClearOfRegularEnemies)
        {
            gameState = GameState.bossStage;

            StartCoroutine(BossStage());
        }
    }

    private void PlayDungeonLevel(int dungeonLevelListIndex)
    {
        bool dungeonBuiltSucessfully = DungeonBuilder.Instance.GenerateDungeon(dungeonLevelList[dungeonLevelListIndex]);

        if (!dungeonBuiltSucessfully)
        {
            Debug.LogError("Couldn't build dungeon from specified rooms and node graphs");
        }

        StaticEventHandler.CallRoomChangedEvent(currentRoom);

        // Set player roughly mid-room
        player.gameObject.transform.position = new Vector3((currentRoom.lowerBounds.x + currentRoom.upperBounds.x) / 2f, (currentRoom.lowerBounds.y + currentRoom.upperBounds.y) / 2f, 0f);

        // Get nearest spawn point in room nearest to player
        player.gameObject.transform.position = HelperUtilities.GetSpawnPositionNearestToPlayer(player.gameObject.transform.position);

        StartCoroutine(DisplayDungeonLevelText());
    }

    private IEnumerator BossStage()
    {
        bossRoom.gameObject.SetActive(true);
        bossRoom.UnlockDoors(0f);

        yield return new WaitForSeconds(2f);

        yield return StartCoroutine(Fade(0f, 1f, 2f, new Color(0f, 0f, 0f, 0.4f)));

        string messageText = $"WELL DONE {GameResources.Instance.currentPlayer.playerName}! YOU SUMMONED THIS STAGE BOSS. FIND AND KILL THAT MF";
        yield return StartCoroutine(DisplayMessageRoutine(messageText, Color.white, 5f));

        yield return StartCoroutine(Fade(1f, 0f, 2f, new Color(0f, 0f, 0f, 0.4f)));
    }

    private IEnumerator LevelCompleted() 
    {
        gameState = GameState.playingLevel;

        yield return new WaitForSeconds(2f);

        yield return StartCoroutine(Fade(0f, 1f, 2f, new Color(0f, 0f, 0f, 0.4f)));
        
        string messageText = $"WELL DONE {GameResources.Instance.currentPlayer.playerName}! YOU PROVED YOUR WORTH.";
        yield return StartCoroutine(DisplayMessageRoutine(messageText, Color.white, 5f));

        messageText = $"PRESS RETURN WHEN READY TO GO FURTHER.";
        yield return StartCoroutine(DisplayMessageRoutine(messageText, Color.white, 5f));

        yield return StartCoroutine(Fade(1f, 0f, 2f, new Color(0f, 0f, 0f, 0.4f)));

        while (!Input.GetKeyDown(KeyCode.Return))
        {
            yield return null;
        }

        yield return null;

        currentDungeonLevelListIndex++;

        PlayDungeonLevel(currentDungeonLevelListIndex);
    }

    private IEnumerator GameWon()
    {
        previousGameState = GameState.gameWon;

        GetPlayer().playerControl.DisablePlayer();

        yield return StartCoroutine(Fade(0f, 1f, 2f, new Color(0f, 0f, 0f, 0.4f)));

        string messageText = $"WELL DONE {GameResources.Instance.currentPlayer.playerName}! YOU PROVED YOUR WORTH.";
        yield return StartCoroutine(DisplayMessageRoutine(messageText, Color.white, 5f));

        messageText = $"SCORE {gameScore}.";
        yield return StartCoroutine(DisplayMessageRoutine(messageText, Color.white, 5f));

        messageText = $"PRESS RETURN TO RESTART THE GAME.";
        yield return StartCoroutine(DisplayMessageRoutine(messageText, Color.white, 5f));

        yield return StartCoroutine(Fade(1f, 0f, 2f, new Color(0f, 0f, 0f, 0.4f)));

        gameState = GameState.restartGame;
    }

    private IEnumerator GameLost()
    {
        previousGameState = GameState.gameLost;
        
        GetPlayer().playerControl.DisablePlayer();

        yield return StartCoroutine(Fade(0f, 1f, 2f, Color.black));

        var enemyArray = GameObject.FindObjectsOfType<Enemy>();
        foreach(var enemy in enemyArray)
        {
            enemy.gameObject.SetActive(false);
        }

        string messageText = $"{GameResources.Instance.currentPlayer.playerName} YOU FOOL! YOU DON'T GET TO DEFEAT ME.";
        yield return StartCoroutine(DisplayMessageRoutine(messageText, Color.white, 5f));

        messageText = $"SCORE {gameScore}.";
        yield return StartCoroutine(DisplayMessageRoutine(messageText, Color.white, 5f));

        messageText = $"PRESS RETURN TO RESTART THE GAME.";
        yield return StartCoroutine(DisplayMessageRoutine(messageText, Color.white, 0f));

        gameState = GameState.restartGame;
    }

    private IEnumerator Fade(float startFadeAlpha, float targetFadeAlpha, float fadeSeconds, Color color)
    {
        var image = canvasGroup.GetComponent<Image>();
        image.color = color;

        float time = 0;

        while (time <= fadeSeconds)
        {
            time += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(startFadeAlpha, targetFadeAlpha, time / fadeSeconds);
            yield return null;
        }
    }

    private IEnumerator DisplayDungeonLevelText()
    {
        StartCoroutine(Fade(0f, 1f, 0f, Color.black));

        GetPlayer().playerControl.DisablePlayer();

        string messageText = "LEVEL " + (currentDungeonLevelListIndex + 1).ToString() + "\n\n" + dungeonLevelList[currentDungeonLevelListIndex].levelName.ToUpper();        

        yield return StartCoroutine(DisplayMessageRoutine(messageText, Color.white, 0f));

        GetPlayer().playerControl.EnablePlayer();

        yield return StartCoroutine(Fade(1f, 0f, 2f, Color.black));
    }

    private IEnumerator DisplayMessageRoutine(string text, Color textColor, float displaySeconds)
    {
        messageTextTMP.SetText(text);
        messageTextTMP.color = textColor;

        if(displaySeconds > 0f)
        {
            float timer = displaySeconds;

            while(timer > 0f && !Input.GetKeyDown(KeyCode.Return))
            {
                timer -= Time.deltaTime;
                yield return null;
            }
        }
        else
        {
            while (!Input.GetKeyDown(KeyCode.Return))
            {
                yield return null;
            }
        }

        yield return null;
        messageTextTMP.SetText(string.Empty);
    }

    private void RestartGame()
    {
        SceneManager.LoadScene(SceneNames.MainGameScene);
    }

    public Player GetPlayer()
    {
        return player;
    }

    public Sprite GetPlayerMiniMapIcon()
    {
        return playerDetails.playerMiniMapIcon;
    }

    public Room GetCurrentRoom()
    {
        return currentRoom;
    }
  
    public DungeonLevelSO GetCurrentDungeonLevel()
    {
        return dungeonLevelList[currentDungeonLevelListIndex];
    }

    #region Validation

#if UNITY_EDITOR

    private void OnValidate()
    {
        HelperUtilities.ValidateCheckEnumerableValues(this, nameof(dungeonLevelList), dungeonLevelList);
        HelperUtilities.ValidateCheckNullValue(this, nameof(canvasGroup), canvasGroup);
        HelperUtilities.ValidateCheckNullValue(this, nameof(messageTextTMP), messageTextTMP);
    }

#endif

    #endregion Validation

}

